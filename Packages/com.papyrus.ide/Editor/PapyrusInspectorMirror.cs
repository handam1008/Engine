using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Papyrus.Ide.Editor
{
    /// <summary>
    /// Shows Unity's real inspector (custom editors, property drawers, UI Toolkit inspectors) inside
    /// PAPyrus. A Unity "Properties" window (the same inspector Unity uses for "Properties...") is opened
    /// for the target. On Windows the IDE then sends its inspector area (mirrorPlace) and the real window
    /// is laid over it, so it is used exactly like Unity. Elsewhere its pixels are captured and streamed
    /// to the IDE as PNG frames, and the IDE's mouse / keyboard input is replayed with
    /// <see cref="EditorWindow.SendEvent"/>.
    ///   IDE -> Unity: inspector.mirrorOpen / mirrorPlace / mirrorResize / mirrorInput / mirrorClose
    ///   Unity -> IDE: inspector.mirrorOverlay, inspector.mirrorFrame {w,h,ppp,png}, inspector.mirrorClosed, focusIde
    /// The window has to stay open (it can sit behind other windows); closing it ends the mirror.
    /// </summary>
    [InitializeOnLoad]
    internal static class PapyrusInspectorMirror
    {
        const double k_FrameInterval = 0.12; // seconds between updates during interaction
        const double k_IdleInterval = 0.5;   // ... once nothing changed for k_IdleAfter seconds
        const double k_IdleAfter = 2.0;
        const int k_MaxPixels = 4096;
        const float k_MinSize = 120f;
        const string k_Title = "PAPyrus 인스펙터 (닫지 마세요)";
        const string k_WindowKey = "Papyrus.InspectorMirror.Window";

        static EditorWindow s_Window;
        static bool s_Active;
        static bool s_Overlay; // the real window sits on the IDE's inspector area (no capture needed)
        static double s_NextCapture;
        static double s_LastChange;
        static bool s_Forced;
        static Hash128 s_LastHash;
        static RenderTexture s_RT;
        static Texture2D s_Tex;

        static readonly BindingFlags k_Any = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        static readonly FieldInfo s_ParentField = typeof(EditorWindow).GetField("m_Parent", k_Any);
        static readonly Type s_PropertyEditorType = typeof(EditorWindow).Assembly.GetType("UnityEditor.PropertyEditor");
        static readonly PropertyInfo s_Tracker = s_PropertyEditorType?.GetProperty("tracker", k_Any);
        static readonly MethodInfo s_RebuildContents = s_PropertyEditorType?.GetMethod("RebuildContentsContainers", k_Any, null, Type.EmptyTypes, null);
#if UNITY_EDITOR_WIN
        static double s_LastOverlayRepaint;
#endif

        static PapyrusInspectorMirror()
        {
            if (Application.isBatchMode)
                return;
            EditorApplication.update += Update;
            // the window survives a domain reload but these statics don't: close it, the IDE reopens it
            AssemblyReloadEvents.beforeAssemblyReload += BeforeReload;
            EditorApplication.quitting += Close;
            EditorApplication.delayCall += CloseLeftover;
        }

        static void BeforeReload()
        {
            var id = PapyrusIds.Of(s_Window);
            Close();
            if (id != 0) SessionState.SetInt(k_WindowKey, id); // checked again after the reload
        }

        /// <summary>A mirror window Unity restored after a reload (closing it before the reload doesn't always stick).</summary>
        static void CloseLeftover()
        {
            var id = SessionState.GetInt(k_WindowKey, 0);
            SessionState.EraseInt(k_WindowKey);
            if (id == 0) return;
#pragma warning disable 0618
            var win = PapyrusIds.Find(id) as EditorWindow;
#pragma warning restore 0618
            if (win && win != s_Window)
            {
                try { win.Close(); }
                catch (Exception) { /* already gone */ }
            }
        }

        // ------------------------------------------------------------------------------ commands

        /// <summary>Open (or retarget) the mirror. Returns an error message or null.</summary>
        internal static string Open(Object target, float width, float height)
        {
            if (!target) return "대상을 찾지 못했어요.";
            if (s_ParentField == null) return "이 Unity 버전에서는 인스펙터 화면을 가져올 수 없어요.";
            var size = ClampSize(width, height);
            s_LastHash = default(Hash128); // always send the first frame

            if (s_Window && Retarget(s_Window, target))
            {
                Resize(size.x, size.y);
                Invalidate();
                return null;
            }

            Close();
#if UNITY_EDITOR_WIN
            var before = WindowCapture.Snapshot();
            var fg = WindowOverlay.Foreground(); // who had the focus before the window appeared
#endif
            var win = CreatePropertyEditor(target);
            if (!win) return "Unity 인스펙터 창을 열지 못했어요. (Unity 2021.3 이상이 필요해요)";
            s_Window = win;
            s_Active = true;
            SessionState.SetInt(k_WindowKey, PapyrusIds.Of(win));
#if UNITY_EDITOR_WIN
            WindowCapture.Adopt(before); // the native window that just appeared is ours
#endif
            win.titleContent = new GUIContent(k_Title);
            Place(win, size);
            RebuildInspector(win);
            Invalidate();
#if UNITY_EDITOR_WIN
            // Showing a window activates it. Only when the IDE was in front is the focus handed back to
            // it. When the user was in Unity (the window reopening after a script reload, a selection made
            // in Unity), it goes back to that Unity window: left active, the mirror would send Unity's
            // main window behind the IDE. Another app in front keeps it. Never pull the IDE up otherwise:
            // it jumped back over whatever the user had just Alt+Tabbed to.
            if (fg != IntPtr.Zero && PapyrusBridge.IdePid != 0 && WindowOverlay.ProcessOf(fg) == PapyrusBridge.IdePid)
            {
                PapyrusBridge.AllowForeground();
                PapyrusBridge.Send("{\"type\":\"focusIde\",\"fg\":" + fg.ToInt64() + "}");
            }
            else if (fg != IntPtr.Zero && WindowOverlay.IsOurs(fg))
                WindowOverlay.Activate(fg);
#else
            // showing a window brings Unity to the front; hand the focus back to the IDE
            PapyrusBridge.AllowForeground();
            PapyrusBridge.Send("{\"type\":\"focusIde\"}");
#endif
            return null;
        }

        internal static void Resize(float width, float height)
        {
            if (!s_Window || s_Overlay) return; // over the IDE the IDE's rect decides the size
            var size = ClampSize(width, height);
            var r = s_Window.position;
            if (Mathf.Approximately(r.width, size.x) && Mathf.Approximately(r.height, size.y)) return;
            s_Window.position = new Rect(r.x, r.y, size.x, size.y);
            Invalidate();
        }

        /// <summary>
        /// Put the real window exactly on the IDE's inspector area (screen pixels), so it is used like
        /// Unity itself: real cursor, popups and dropdowns where they belong, keyboard and IME.
        /// Windows only; elsewhere the window stays in Unity and the IDE shows captured frames.
        /// </summary>
        internal static void PlaceOverIde(PapyrusBridge.Command c)
        {
#if UNITY_EDITOR_WIN
            if (!s_Window || c.width < 1 || c.height < 1) return;
            var placed = WindowOverlay.Place(s_Window, EditorGUIUtility.pixelsPerPoint, new IntPtr(c.hwnd),
                Mathf.RoundToInt(c.x), Mathf.RoundToInt(c.y), Mathf.RoundToInt(c.width), Mathf.RoundToInt(c.height),
                c.visible, out var changed);
            if (!placed) return;
            if (changed) Invalidate();
            if (!s_Overlay)
            {
                s_Overlay = true;
                PapyrusBridge.Send("{\"type\":\"inspector.mirrorOverlay\",\"on\":true}");
            }
#endif
        }

        internal static void Close()
        {
            s_Active = false;
            s_Overlay = false;
            SessionState.EraseInt(k_WindowKey);
#if UNITY_EDITOR_WIN
            WindowOverlay.Forget();
#endif
            if (s_Window)
            {
                try { s_Window.Close(); }
                catch (Exception) { /* already gone */ }
            }
            s_Window = null;
        }

        /// <summary>Replay one input event from the IDE into the window.</summary>
        internal static string Input(PapyrusBridge.Command c)
        {
            if (!s_Window) return "인스펙터 화면이 닫혀 있어요.";
            var mods = (EventModifiers)c.mods;
            Event e;
            switch (c.value)
            {
                case "down":
                    e = new Event { type = EventType.MouseDown, button = c.button, clickCount = Math.Max(1, c.clicks) };
                    break;
                case "up":
                    e = new Event { type = EventType.MouseUp, button = c.button, clickCount = Math.Max(1, c.clicks) };
                    break;
                case "drag":
                    e = new Event { type = EventType.MouseDrag, button = c.button, delta = new Vector2(c.dx, c.dy) };
                    break;
                case "move":
                    e = new Event { type = EventType.MouseMove, delta = new Vector2(c.dx, c.dy) };
                    break;
                case "wheel":
                    e = new Event { type = EventType.ScrollWheel, delta = new Vector2(c.dx, c.dy) };
                    break;
                case "keydown":
                case "keyup":
                {
                    KeyCode code = KeyCode.None;
                    if (!string.IsNullOrEmpty(c.key)) Enum.TryParse(c.key, out code);
                    e = new Event
                    {
                        type = c.value == "keydown" ? EventType.KeyDown : EventType.KeyUp,
                        keyCode = code,
                        character = string.IsNullOrEmpty(c.chr) ? '\0' : c.chr[0],
                    };
                    break;
                }
                case "command":
                    return Command(c.name);
                default:
                    return "알 수 없는 입력: " + c.value;
            }
            e.mousePosition = new Vector2(c.x, c.y);
            e.modifiers = mods;
            s_Window.SendEvent(e);
            Invalidate();
            return null;
        }

        /// <summary>Copy / Paste / Cut / SelectAll / Duplicate / Delete go through IMGUI commands, Undo through Undo.</summary>
        static string Command(string name)
        {
            switch (name)
            {
                case "Undo":
                    Undo.PerformUndo();
                    break;
                case "Redo":
                    Undo.PerformRedo();
                    break;
                default:
                    var validate = EditorGUIUtility.CommandEvent(name);
                    validate.type = EventType.ValidateCommand;
                    s_Window.SendEvent(validate);
                    s_Window.SendEvent(EditorGUIUtility.CommandEvent(name));
                    break;
            }
            Invalidate();
            return null;
        }

        static void Invalidate()
        {
            s_Forced = true;
            s_LastChange = EditorApplication.timeSinceStartup;
        }

        // ------------------------------------------------------------------------------ window

        static Vector2 ClampSize(float w, float h)
        {
            return new Vector2(Mathf.Clamp(w, k_MinSize, 2000f), Mathf.Clamp(h, k_MinSize, 3000f));
        }

        /// <summary>Unity's Properties window for one object (internal API, reached by reflection).</summary>
        static EditorWindow CreatePropertyEditor(Object target)
        {
            if (s_PropertyEditorType == null) return null;
            foreach (var m in s_PropertyEditorType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (m.Name != "OpenPropertyEditor") continue;
                var ps = m.GetParameters();
                if (ps.Length == 0 || ps[0].ParameterType != typeof(Object)) continue;
                var args = new object[ps.Length];
                args[0] = target;
                var shown = true;
                for (var i = 1; i < ps.Length; i++)
                {
                    if (ps[i].ParameterType == typeof(bool))
                    {
                        args[i] = false; // showWindow: we show it ourselves
                        shown = false;
                    }
                    else args[i] = ps[i].HasDefaultValue ? ps[i].DefaultValue : null;
                }
                EditorWindow win = null;
                try
                {
                    win = m.Invoke(null, args) as EditorWindow;
                }
                catch (Exception e)
                {
                    Debug.LogWarning("[PAPyrus] 인스펙터 창을 여는 중 오류: " + (e.InnerException ?? e).Message);
                }
                if (!win)
                {
                    // older signature returning void: pick the newest Properties window
                    var all = Resources.FindObjectsOfTypeAll(s_PropertyEditorType);
                    win = all.Length > 0 ? all[all.Length - 1] as EditorWindow : null;
                    shown = true;
                }
                if (win && !shown) win.ShowUtility();
                return win;
            }
            return null;
        }

        /// <summary>Point an open Properties window at another object without reopening it.</summary>
        static bool Retarget(EditorWindow win, Object target)
        {
            try
            {
                var tracker = s_Tracker?.GetValue(win, null) as ActiveEditorTracker;
                if (tracker == null) return false;
                var setLocked = typeof(ActiveEditorTracker).GetMethod("SetObjectsLockedByThisTracker", k_Any);
                if (setLocked == null) return false;
                setLocked.Invoke(tracker, new object[] { new List<Object> { target } });
                RebuildInspector(win, tracker);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        static void RebuildInspector(EditorWindow win, ActiveEditorTracker tracker = null)
        {
            try
            {
                tracker = tracker ?? s_Tracker?.GetValue(win, null) as ActiveEditorTracker;
                tracker?.ForceRebuild();
                // The first OnGUI event can be deferred while Unity's main window is minimized.
                s_RebuildContents?.Invoke(win, null);
            }
            catch (TargetInvocationException e)
            {
                Debug.LogWarning("[PAPyrus] 인스펙터 내용을 갱신하지 못했어요: " + (e.InnerException ?? e).Message);
            }
            win.Repaint();
        }

        /// <summary>Bottom-right of Unity's main window, where it's least in the way.</summary>
        static void Place(EditorWindow win, Vector2 size)
        {
            Rect main;
            try
            {
                main = EditorGUIUtility.GetMainWindowPosition();
            }
            catch (Exception)
            {
                main = new Rect(0, 0, 1600, 900);
            }
            var x = Mathf.Max(main.x, main.xMax - size.x - 24);
            var y = Mathf.Max(main.y + 40, main.yMax - size.y - 40);
            win.position = new Rect(x, y, size.x, size.y);
        }

        // ------------------------------------------------------------------------------ capture

        static void Update()
        {
            if (!s_Active) return;
            if (!s_Window)
            {
                s_Active = false;
                PapyrusBridge.Send("{\"type\":\"inspector.mirrorClosed\"}");
                return;
            }
            if (PapyrusBridge.ConnectedClients == 0)
            {
                Close();
                return;
            }
            var now = EditorApplication.timeSinceStartup;
#if UNITY_EDITOR_WIN
            if (s_Overlay)
            {
                WindowOverlay.KeepAboveIde();
                if (!WindowOverlay.Visible) return;
                var active = now - s_LastChange <= k_IdleAfter || EditorWindow.mouseOverWindow == s_Window ||
                             EditorWindow.focusedWindow == s_Window;
                if (s_Forced || now - s_LastOverlayRepaint >= (active ? k_FrameInterval : k_IdleInterval))
                {
                    s_Forced = false;
                    s_LastOverlayRepaint = now;
                    s_Window.Repaint();
                }
                return;
            }
#endif
            if (!s_Forced && now < s_NextCapture) return;
            s_Forced = false;
            string error;
            if (!CaptureAndSend(out error))
            {
                PapyrusBridge.Send("{\"type\":\"inspector.mirrorClosed\",\"error\":" + Json.Str(error) + "}");
                Close();
                return;
            }
            s_NextCapture = now + (now - s_LastChange > k_IdleAfter ? k_IdleInterval : k_FrameInterval);
        }

        static bool CaptureAndSend(out string error)
        {
            error = null;
            var ppp = EditorGUIUtility.pixelsPerPoint;
            byte[] raw = null;
            int pw = 0, ph = 0;
#if UNITY_EDITOR_WIN
            // the window as Windows shows it (DWM): upright and complete, also while it's covered
            if (!WindowCapture.Grab(s_Window, ppp, out raw, out pw, out ph)) raw = null;
#endif
            if (raw == null && !GrabFromView(ppp, out raw, out pw, out ph, out error))
                return false;
            if (raw == null || IsBlank(raw))
                return true; // not drawn yet: try again next tick

            var hash = Hash128.Compute(raw);
            if (hash == s_LastHash) return true;
            s_LastHash = hash;
            s_LastChange = EditorApplication.timeSinceStartup;

            var png = ImageConversion.EncodeArrayToPNG(raw, UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8_SRGB, (uint)pw, (uint)ph);
            PapyrusBridge.Send("{\"type\":\"inspector.mirrorFrame\",\"w\":" + pw + ",\"h\":" + ph +
                               ",\"ppp\":" + ppp.ToString(System.Globalization.CultureInfo.InvariantCulture) +
                               ",\"png\":\"" + Convert.ToBase64String(png) + "\"}");
            return true;
        }

        /// <summary>
        /// Fallback (macOS / Linux): read the view's back buffer. Bottom-up RGB24, or null when the view
        /// isn't there yet. Returns false only when this Unity can't do it at all.
        /// </summary>
        static bool GrabFromView(float ppp, out byte[] raw, out int pw, out int ph, out string error)
        {
            raw = null;
            pw = ph = 0;
            error = null;
            var parent = s_ParentField.GetValue(s_Window);
            if (parent == null) return true;
            var grab = parent.GetType().GetMethod("GrabPixels", k_Any, null, new[] { typeof(RenderTexture), typeof(Rect) }, null);
            if (grab == null)
            {
                error = "이 Unity 버전에서는 인스펙터 화면을 가져올 수 없어요.";
                return false;
            }

            var r = s_Window.position;
            pw = Mathf.Clamp(Mathf.RoundToInt(r.width * ppp), 1, k_MaxPixels);
            ph = Mathf.Clamp(Mathf.RoundToInt(r.height * ppp), 1, k_MaxPixels);
            if (s_RT == null || s_RT.width != pw || s_RT.height != ph)
            {
                if (s_RT != null) Object.DestroyImmediate(s_RT);
                if (s_Tex != null) Object.DestroyImmediate(s_Tex);
                s_RT = new RenderTexture(pw, ph, 0, RenderTextureFormat.ARGB32) { hideFlags = HideFlags.HideAndDontSave };
                s_Tex = new Texture2D(pw, ph, TextureFormat.RGB24, false) { hideFlags = HideFlags.HideAndDontSave };
            }

            try
            {
                grab.Invoke(parent, new object[] { s_RT, new Rect(0, 0, pw, ph) });
            }
            catch (Exception e)
            {
                error = "인스펙터 화면을 가져오지 못했어요: " + (e.InnerException ?? e).Message;
                return false;
            }

            var prev = RenderTexture.active;
            RenderTexture.active = s_RT;
            s_Tex.ReadPixels(new Rect(0, 0, pw, ph), 0, 0, false);
            s_Tex.Apply(false);
            RenderTexture.active = prev;

            raw = s_Tex.GetRawTextureData();
            // with the UV origin at the top (Metal / Vulkan / D3D) GrabPixels leaves the rows top-down
            if (SystemInfo.graphicsUVStartsAtTop) raw = FlipRows(raw, pw * 3, ph);
            return true;
        }

        /// <summary>One flat color: a window that hasn't painted its inspector yet.</summary>
        static bool IsBlank(byte[] rgb)
        {
            for (var i = 3; i + 2 < rgb.Length; i += 3)
                if (rgb[i] != rgb[0] || rgb[i + 1] != rgb[1] || rgb[i + 2] != rgb[2])
                    return false;
            return true;
        }

        static byte[] s_Flipped;

        static byte[] FlipRows(byte[] src, int stride, int rows)
        {
            if (s_Flipped == null || s_Flipped.Length != src.Length) s_Flipped = new byte[src.Length];
            for (var y = 0; y < rows; y++)
                Buffer.BlockCopy(src, y * stride, s_Flipped, (rows - 1 - y) * stride, stride);
            return s_Flipped;
        }
    }

#if UNITY_EDITOR_WIN
    /// <summary>
    /// Captures an editor window through Windows (PrintWindow + PW_RENDERFULLCONTENT). Unlike GUIView's
    /// back buffer this is what is actually on screen, top-down, and it keeps working while the window
    /// is behind other windows.
    /// </summary>
    internal static class WindowCapture
    {
        const uint k_PrintClientOnly = 1, k_PrintRenderFullContent = 2;

        static IntPtr s_Hwnd;
        static byte[] s_Bgra, s_Rgb;

        // window lookup state for the EnumWindows callback
        static readonly EnumProc s_Enum = OnWindow;
        static uint s_Pid;
        static Vector2Int s_WantSize;
        static string s_WantTitle;
        static IntPtr s_Best;
        static int s_BestScore;
        static HashSet<IntPtr> s_Collect; // Snapshot() in progress

        /// <summary>The native window of an editor window (found once, then remembered while it lives).</summary>
        internal static IntPtr Handle(EditorWindow win, float ppp)
        {
            if (s_Hwnd != IntPtr.Zero && IsWindow(s_Hwnd)) return s_Hwnd;
            var want = new Vector2Int(Mathf.RoundToInt(win.position.width * ppp), Mathf.RoundToInt(win.position.height * ppp));
            s_Hwnd = Find(win, want);
            return s_Hwnd;
        }

        internal static void Forget()
        {
            s_Hwnd = IntPtr.Zero;
        }

        /// <summary>This process's visible top-level windows right now.</summary>
        internal static HashSet<IntPtr> Snapshot()
        {
            var set = new HashSet<IntPtr>();
            try
            {
                s_Pid = (uint)System.Diagnostics.Process.GetCurrentProcess().Id;
                s_Collect = set;
                EnumWindows(s_Enum, IntPtr.Zero);
            }
            catch (Exception) { /* no user32: size matching later */ }
            finally
            {
                s_Collect = null;
            }
            return set;
        }

        /// <summary>Remember the window that appeared since <paramref name="before"/> (just shown: ours).</summary>
        internal static void Adopt(HashSet<IntPtr> before)
        {
            s_Hwnd = IntPtr.Zero;
            foreach (var h in Snapshot())
            {
                if (before.Contains(h)) continue;
                s_Hwnd = h;
                break;
            }
        }

        /// <summary>Bottom-up RGB24 pixels of the window's client area, or false when it can't be captured.</summary>
        internal static bool Grab(EditorWindow win, float ppp, out byte[] rgb, out int w, out int h)
        {
            rgb = null;
            w = h = 0;
            try
            {
                if (Handle(win, ppp) == IntPtr.Zero || !IsWindowVisible(s_Hwnd)) return false;

                var size = ClientSize(s_Hwnd);
                w = Mathf.Clamp(size.x, 1, 4096);
                h = Mathf.Clamp(size.y, 1, 4096);
                if (!Print(s_Hwnd, w, h)) return false;

                // BGRA top-down -> RGB bottom-up (Unity's texture row order, which EncodeArrayToPNG expects)
                if (s_Rgb == null || s_Rgb.Length != w * h * 3) s_Rgb = new byte[w * h * 3];
                for (var y = 0; y < h; y++)
                {
                    var i = y * w * 4;
                    var j = (h - 1 - y) * w * 3;
                    for (var x = 0; x < w; x++, i += 4, j += 3)
                    {
                        s_Rgb[j] = s_Bgra[i + 2];
                        s_Rgb[j + 1] = s_Bgra[i + 1];
                        s_Rgb[j + 2] = s_Bgra[i];
                    }
                }
                rgb = s_Rgb;
                return true;
            }
            catch (Exception)
            {
                s_Hwnd = IntPtr.Zero;
                return false; // e.g. no user32: the GrabPixels fallback takes over
            }
        }

        static bool Print(IntPtr hwnd, int w, int h)
        {
            var info = new BitmapInfoHeader
            {
                size = (uint)Marshal.SizeOf(typeof(BitmapInfoHeader)),
                width = w,
                height = -h, // negative: top-down rows
                planes = 1,
                bitCount = 32,
            };
            var screen = GetDC(IntPtr.Zero);
            var dc = CreateCompatibleDC(screen);
            IntPtr bits;
            var bmp = CreateDIBSection(dc, ref info, 0, out bits, IntPtr.Zero, 0);
            var old = SelectObject(dc, bmp);
            try
            {
                if (bmp == IntPtr.Zero || !PrintWindow(hwnd, dc, k_PrintClientOnly | k_PrintRenderFullContent)) return false;
                GdiFlush();
                var len = w * h * 4;
                if (s_Bgra == null || s_Bgra.Length != len) s_Bgra = new byte[len];
                Marshal.Copy(bits, s_Bgra, 0, len);
                return true;
            }
            finally
            {
                SelectObject(dc, old);
                if (bmp != IntPtr.Zero) DeleteObject(bmp);
                DeleteDC(dc);
                ReleaseDC(IntPtr.Zero, screen);
            }
        }

        /// <summary>
        /// The top-level window of this process whose client area has the editor window's size
        /// (the title breaks ties: PropertyEditor names itself after the target).
        /// </summary>
        static IntPtr Find(EditorWindow win, Vector2Int want)
        {
            s_Pid = (uint)System.Diagnostics.Process.GetCurrentProcess().Id;
            s_WantSize = want;
            s_WantTitle = win.titleContent != null ? win.titleContent.text : null;
            s_Best = IntPtr.Zero;
            s_BestScore = int.MaxValue;
            EnumWindows(s_Enum, IntPtr.Zero);
            return s_Best;
        }

        static bool OnWindow(IntPtr hwnd, IntPtr _)
        {
            uint pid;
            GetWindowThreadProcessId(hwnd, out pid);
            if (pid != s_Pid || !IsWindowVisible(hwnd)) return true;
            if (s_Collect != null)
            {
                s_Collect.Add(hwnd);
                return true;
            }
            var score = Distance(ClientSize(hwnd), s_WantSize);
            if (score > 6) return true;
            if (!string.IsNullOrEmpty(s_WantTitle))
            {
                var sb = new System.Text.StringBuilder(256);
                GetWindowText(hwnd, sb, sb.Capacity);
                if (sb.ToString() != s_WantTitle) score += 3;
            }
            if (score < s_BestScore)
            {
                s_BestScore = score;
                s_Best = hwnd;
            }
            return true;
        }

        static Vector2Int ClientSize(IntPtr hwnd)
        {
            Rect32 r;
            return GetClientRect(hwnd, out r) ? new Vector2Int(r.right - r.left, r.bottom - r.top) : Vector2Int.zero;
        }

        static int Distance(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }

        delegate bool EnumProc(IntPtr hwnd, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        struct Rect32 { public int left, top, right, bottom; }

        [StructLayout(LayoutKind.Sequential)]
        struct BitmapInfoHeader
        {
            public uint size;
            public int width, height;
            public ushort planes, bitCount;
            public uint compression, sizeImage;
            public int xPelsPerMeter, yPelsPerMeter;
            public uint clrUsed, clrImportant;
        }

        [DllImport("user32.dll")] static extern bool EnumWindows(EnumProc proc, IntPtr lParam);
        [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint pid);
        [DllImport("user32.dll")] static extern bool IsWindow(IntPtr hwnd);
        [DllImport("user32.dll")] static extern bool IsWindowVisible(IntPtr hwnd);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)] static extern int GetWindowText(IntPtr hwnd, System.Text.StringBuilder text, int max);
        [DllImport("user32.dll")] static extern bool GetClientRect(IntPtr hwnd, out Rect32 rect);
        [DllImport("user32.dll")] static extern bool PrintWindow(IntPtr hwnd, IntPtr hdc, uint flags);
        [DllImport("user32.dll")] static extern IntPtr GetDC(IntPtr hwnd);
        [DllImport("user32.dll")] static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);
        [DllImport("gdi32.dll")] static extern IntPtr CreateCompatibleDC(IntPtr hdc);
        [DllImport("gdi32.dll")] static extern IntPtr CreateDIBSection(IntPtr hdc, ref BitmapInfoHeader info, uint usage, out IntPtr bits, IntPtr section, uint offset);
        [DllImport("gdi32.dll")] static extern IntPtr SelectObject(IntPtr hdc, IntPtr obj);
        [DllImport("gdi32.dll")] static extern bool DeleteObject(IntPtr obj);
        [DllImport("gdi32.dll")] static extern bool DeleteDC(IntPtr hdc);
        [DllImport("gdi32.dll")] static extern bool GdiFlush();
    }

    /// <summary>
    /// Keeps the inspector window glued onto the IDE's inspector area: borderless, owned by nobody
    /// (so clicking it doesn't pull Unity's main window over the IDE), out of the taskbar, and always
    /// directly above the IDE window in the z-order without being topmost.
    /// </summary>
    internal static class WindowOverlay
    {
        const int GWL_STYLE = -16, GWL_EXSTYLE = -20, GWLP_HWNDPARENT = -8;
        const long WS_CAPTION = 0x00C00000, WS_THICKFRAME = 0x00040000, WS_SYSMENU = 0x00080000,
                   WS_MINIMIZEBOX = 0x00020000, WS_MAXIMIZEBOX = 0x00010000;
        const long WS_EX_TOOLWINDOW = 0x80, WS_EX_APPWINDOW = 0x40000, WS_EX_TOPMOST = 0x8;
        const uint SWP_NOSIZE = 0x1, SWP_NOMOVE = 0x2, SWP_NOZORDER = 0x4, SWP_NOACTIVATE = 0x10,
                   SWP_FRAMECHANGED = 0x20, SWP_NOOWNERZORDER = 0x200;
        const int SW_HIDE = 0, SW_SHOWNOACTIVATE = 4;
        const uint GW_HWNDPREV = 3, GW_OWNER = 4;

        static IntPtr s_Hwnd, s_Ide;
        static IntPtr s_Main; // Unity's main window (the mirror window's original owner)
        static IntPtr s_LastFg; // foreground window at the previous tick
        static readonly HashSet<IntPtr> s_Adopted = new HashSet<IntPtr>(); // popups re-owned to our window
        static uint s_Pid;
        static bool s_Styled, s_Visible;
        static RectInt s_Rect;

        internal static bool Visible => s_Visible;

        internal static bool Place(EditorWindow win, float ppp, IntPtr ide, int x, int y, int w, int h, bool visible, out bool changed)
        {
            changed = false;
            try
            {
                var hwnd = WindowCapture.Handle(win, ppp);
                if (hwnd == IntPtr.Zero) return false;
                if (hwnd != s_Hwnd)
                {
                    s_Hwnd = hwnd;
                    s_Styled = false;
                    s_Visible = true;
                    s_Rect = default(RectInt);
                }
                s_Ide = ide;
                var restyle = !s_Styled;
                if (restyle)
                {
                    var owner = GetWindow(hwnd, GW_OWNER); // Unity's main window, before we cut the tie
                    if (owner != IntPtr.Zero) s_Main = owner;
                    var style = GetWindowLongPtr(hwnd, GWL_STYLE).ToInt64();
                    style &= ~(WS_CAPTION | WS_THICKFRAME | WS_SYSMENU | WS_MINIMIZEBOX | WS_MAXIMIZEBOX);
                    SetWindowLongPtr(hwnd, GWL_STYLE, new IntPtr(style));
                    var ex = GetWindowLongPtr(hwnd, GWL_EXSTYLE).ToInt64();
                    SetWindowLongPtr(hwnd, GWL_EXSTYLE, new IntPtr((ex | WS_EX_TOOLWINDOW) & ~(WS_EX_APPWINDOW | WS_EX_TOPMOST)));
                    SetWindowLongPtr(hwnd, GWLP_HWNDPARENT, IntPtr.Zero);
                    s_Styled = true;
                }
                var rect = new RectInt(x, y, w, h);
                if (restyle || !rect.Equals(s_Rect))
                {
                    SetWindowPos(hwnd, IntPtr.Zero, x, y, w, h,
                        SWP_NOZORDER | SWP_NOACTIVATE | SWP_NOOWNERZORDER | (restyle ? SWP_FRAMECHANGED : 0));
                    s_Rect = rect;
                    changed = true;
                }
                if (visible != s_Visible)
                {
                    ShowWindow(hwnd, visible ? SW_SHOWNOACTIVATE : SW_HIDE);
                    s_Visible = visible;
                    changed = true;
                }
                KeepAboveIde();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>Called every editor tick: slot the window back right above the IDE window.</summary>
        internal static void KeepAboveIde()
        {
            if (!s_Visible || s_Hwnd == IntPtr.Zero || s_Ide == IntPtr.Zero) return;
            try
            {
                var fg = GetForegroundWindow();
                if (fg != s_LastFg)
                {
                    ReleaseClosedPopups();
                    AdoptPopup(fg, s_LastFg);
                    s_LastFg = fg;
                }
                // Clicking a field in the window activates Unity, and Unity lifts its main window along
                // (on top of the IDE). While the user works in our window or a popup it opened, send the
                // main window back behind the IDE. Any other Unity window (its main window, a floating Game
                // view or Console, Project Settings, a dialog or menu opened in Unity) means the user is in
                // Unity: leave it, or Alt+Tab to one of those dropped Unity under the IDE.
                if (fg != IntPtr.Zero && (fg == s_Hwnd || s_Adopted.Contains(fg))) SendMainBehindIde();
                if (fg == s_Hwnd) return; // in use: Windows already put it on top
                var prev = GetWindow(s_Ide, GW_HWNDPREV);
                var above = prev;
                while (above != IntPtr.Zero && above != s_Hwnd && !IsWindowVisible(above))
                    above = GetWindow(above, GW_HWNDPREV);
                if (above == s_Hwnd) return;
                // right below whatever is right above the IDE; at the top of the normal band when that's a topmost window
                var after = prev != IntPtr.Zero && (GetWindowLongPtr(prev, GWL_EXSTYLE).ToInt64() & WS_EX_TOPMOST) == 0 ? prev : IntPtr.Zero;
                SetWindowPos(s_Hwnd, after, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_NOOWNERZORDER);
            }
            catch (Exception)
            {
                s_Ide = IntPtr.Zero;
            }
        }

        internal static bool IsOurs(IntPtr hwnd)
        {
            if (s_Pid == 0) s_Pid = (uint)System.Diagnostics.Process.GetCurrentProcess().Id;
            return ProcessOf(hwnd) == s_Pid;
        }

        internal static uint ProcessOf(IntPtr hwnd)
        {
            try
            {
                uint pid;
                GetWindowThreadProcessId(hwnd, out pid);
                return pid;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        static bool CursorOnWindow()
        {
            try
            {
                POINT p;
                if (!GetCursorPos(out p)) return true; // can't tell: as before
                return p.x >= s_Rect.x && p.x < s_Rect.x + s_Rect.width && p.y >= s_Rect.y && p.y < s_Rect.y + s_Rect.height;
            }
            catch (Exception)
            {
                return true;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        struct POINT
        {
            public int x, y;
        }

        internal static IntPtr Foreground()
        {
            try { return GetForegroundWindow(); }
            catch (Exception) { return IntPtr.Zero; }
        }

        /// <summary>Give the focus back to one of Unity's own windows (Unity is in front, so it may).</summary>
        internal static void Activate(IntPtr hwnd)
        {
            try
            {
                if (IsWindowVisible(hwnd)) SetForegroundWindow(hwnd);
            }
            catch (Exception) { /* ignored */ }
        }

        /// <summary>
        /// Popups Unity opens from our window (object picker for sprites/materials, color picker, curve
        /// editor, dropdowns) are owned by Unity's main window, and Windows raises an owner together with
        /// the owned window it activates: every picker pulled the main window over the IDE, and it could
        /// not be pushed back while the picker hung on it. A popup that comes up while the user is in our
        /// window (or the IDE) is handed to our window instead; owned windows stay above their owner, so
        /// it shows over the inspector and closing it returns focus there, not to Unity.
        /// Popups opened from Unity's main window itself are left alone.
        /// </summary>
        static void AdoptPopup(IntPtr fg, IntPtr prevFg)
        {
            if (fg == IntPtr.Zero || fg == s_Hwnd || s_Adopted.Contains(fg)) return;
            if (prevFg != s_Hwnd && prevFg != s_Ide && !s_Adopted.Contains(prevFg)) return;
            // Opened from our window means clicked there: the cursor is on it. A Unity window the user
            // Alt+Tabbed to from the IDE (Project Settings, a floating view) is theirs to keep in front.
            if (!s_Adopted.Contains(prevFg) && !CursorOnWindow()) return;
            if (!IsOurs(fg)) return;
            var main = s_Main != IntPtr.Zero ? s_Main : System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
            if (main == IntPtr.Zero || fg == main || GetWindow(fg, GW_OWNER) != main) return;
            if (s_Adopted.Count > 64) s_Adopted.Clear(); // closed popups' handles; never needed again
            SetWindowLongPtr(fg, GWLP_HWNDPARENT, s_Hwnd);
            s_Adopted.Add(fg);
            SendMainBehindIde();
        }

        /// <summary>
        /// Unity hides and reuses some popups (object picker, color picker) instead of closing them. One
        /// adopted while it was opened from our window stayed "ours": opened later from Unity itself, it
        /// sent Unity's main window behind the IDE, so the IDE jumped over Unity while the user worked
        /// there. Once a popup is gone or hidden it goes back to Unity's main window.
        /// </summary>
        static void ReleaseClosedPopups()
        {
            if (s_Adopted.Count == 0) return;
            List<IntPtr> gone = null;
            foreach (var h in s_Adopted)
            {
                if (IsWindow(h) && IsWindowVisible(h)) continue;
                (gone ?? (gone = new List<IntPtr>())).Add(h);
            }
            if (gone == null) return;
            foreach (var h in gone)
            {
                s_Adopted.Remove(h);
                if (s_Main != IntPtr.Zero && IsWindow(h) && GetWindow(h, GW_OWNER) == s_Hwnd)
                    SetWindowLongPtr(h, GWLP_HWNDPARENT, s_Main);
            }
        }

        /// <summary>Unity's main window directly below the IDE window, if it is somewhere above it.</summary>
        static void SendMainBehindIde()
        {
            if (s_Main == IntPtr.Zero) s_Main = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
            if (s_Main == IntPtr.Zero || s_Main == s_Hwnd || !IsWindowVisible(s_Main) || IsIconic(s_Main)) return;
            var above = false;
            var h = GetWindow(s_Ide, GW_HWNDPREV);
            for (var i = 0; h != IntPtr.Zero && i < 500; i++, h = GetWindow(h, GW_HWNDPREV))
            {
                if (h != s_Main) continue;
                above = true;
                break;
            }
            if (above)
                SetWindowPos(s_Main, s_Ide, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_NOOWNERZORDER);
        }

        internal static void Forget()
        {
            s_Hwnd = s_Ide = s_LastFg = IntPtr.Zero;
            s_Adopted.Clear();
            s_Styled = false;
            WindowCapture.Forget();
        }

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")] static extern IntPtr GetWindowLongPtr(IntPtr hwnd, int index);
        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")] static extern IntPtr SetWindowLongPtr(IntPtr hwnd, int index, IntPtr value);
        [DllImport("user32.dll")] static extern bool SetWindowPos(IntPtr hwnd, IntPtr after, int x, int y, int w, int h, uint flags);
        [DllImport("user32.dll")] static extern bool ShowWindow(IntPtr hwnd, int cmd);
        [DllImport("user32.dll")] static extern IntPtr GetWindow(IntPtr hwnd, uint cmd);
        [DllImport("user32.dll")] static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")] static extern bool SetForegroundWindow(IntPtr hwnd);
        [DllImport("user32.dll")] static extern bool GetCursorPos(out POINT p);
        [DllImport("user32.dll")] static extern bool IsWindow(IntPtr hwnd);
        [DllImport("user32.dll")] static extern bool IsWindowVisible(IntPtr hwnd);
        [DllImport("user32.dll")] static extern bool IsIconic(IntPtr hwnd);
        [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint pid);
    }
#endif
}
