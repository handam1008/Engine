using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Papyrus.Ide.Editor
{
    /// <summary>
    /// Localhost TCP bridge between the Unity Editor and PAPyrus.
    /// Protocol: one JSON object per line, UTF-8.
    ///   Unity -> IDE: log, history, state, playStarting, compileFinished
    ///   IDE -> Unity: hello, play, stop, pause, step, refresh, sync, inspector.* (answered with "reply")
    /// The port is written to Temp/PAPyrus/bridge.json after every domain reload.
    /// </summary>
    [InitializeOnLoad]
    internal static class PapyrusBridge
    {
        const int k_HistorySize = 400;

        static TcpListener s_Listener;
        static Thread s_AcceptThread;
        static Thread s_SendThread;
        static volatile bool s_Running;
        static readonly List<TcpClient> s_Clients = new List<TcpClient>();
        static readonly object s_Lock = new object();
        static readonly ConcurrentQueue<string> s_Incoming = new ConcurrentQueue<string>();
        static readonly BlockingCollection<string> s_Outgoing = new BlockingCollection<string>(new ConcurrentQueue<string>());
        static readonly LinkedList<string> s_History = new LinkedList<string>();
        static int s_Port;
        static bool s_LastPlaying, s_LastPaused, s_LastCompiling;
        static bool s_StateDirty = true;
        static int s_CompileErrors, s_CompileWarnings;

        public static int ConnectedClients
        {
            get { lock (s_Lock) return s_Clients.Count; }
        }

        static PapyrusBridge()
        {
            if (Application.isBatchMode)
                return;

            Start();
            Application.logMessageReceivedThreaded += OnLog;
            EditorApplication.update += Update;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            CompilationPipeline.compilationStarted += OnCompilationStarted;
            CompilationPipeline.assemblyCompilationFinished += OnAssemblyCompiled;
            CompilationPipeline.compilationFinished += OnCompilationFinished;
            AssemblyReloadEvents.beforeAssemblyReload += Stop;
            EditorApplication.quitting += OnQuit;
        }

        // ------------------------------------------------------------------------------ lifecycle

        static void Start()
        {
            try
            {
                s_Listener = new TcpListener(IPAddress.Loopback, 0);
                s_Listener.Start();
                s_Port = ((IPEndPoint)s_Listener.LocalEndpoint).Port;
                s_Running = true;

                s_AcceptThread = new Thread(AcceptLoop) { IsBackground = true, Name = "PAPyrus accept" };
                s_AcceptThread.Start();
                s_SendThread = new Thread(SendLoop) { IsBackground = true, Name = "PAPyrus send" };
                s_SendThread.Start();

                WriteBridgeFile();
            }
            catch (Exception e)
            {
                s_Running = false;
                Debug.LogWarning("[PAPyrus] 연동 서버를 시작하지 못했어요: " + e.Message);
            }
        }

        static void Stop()
        {
            s_Running = false;
            try
            {
                if (s_Listener != null)
                    s_Listener.Stop();
            }
            catch (Exception)
            {
                // ignored
            }

            lock (s_Lock)
            {
                foreach (var c in s_Clients)
                {
                    try { c.Close(); }
                    catch (Exception) { /* ignored */ }
                }
                s_Clients.Clear();
            }

            s_Outgoing.Add(null); // wake the sender so it can exit
        }

        static void OnQuit()
        {
            Stop();
            try
            {
                File.Delete(BridgeFile);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        static string BridgeFile
        {
            get { return Path.Combine(ProjectGeneration.ProjectDirectory, "Temp", "PAPyrus", "bridge.json"); }
        }

        static void WriteBridgeFile()
        {
            var dir = Path.GetDirectoryName(BridgeFile);
            Directory.CreateDirectory(dir);
            var json = "{" +
                       "\"port\":" + s_Port +
                       ",\"pid\":" + Process.GetCurrentProcess().Id +
                       ",\"unityVersion\":" + Json.Str(Application.unityVersion) +
                       ",\"projectName\":" + Json.Str(Path.GetFileName(ProjectGeneration.ProjectDirectory)) +
                       ",\"projectPath\":" + Json.Str(ProjectGeneration.ProjectDirectory) +
                       "}";
            File.WriteAllText(BridgeFile, json, new UTF8Encoding(false));
        }

        // ------------------------------------------------------------------------------ networking

        static void AcceptLoop()
        {
            while (s_Running)
            {
                TcpClient client;
                try
                {
                    client = s_Listener.AcceptTcpClient();
                }
                catch (Exception)
                {
                    break; // listener stopped
                }

                client.NoDelay = true;
                lock (s_Lock)
                    s_Clients.Add(client);

                var reader = new Thread(() => ReadLoop(client)) { IsBackground = true, Name = "PAPyrus read" };
                reader.Start();

                // Send the recent console output and current state to the new client.
                string[] history;
                lock (s_History)
                {
                    history = new string[s_History.Count];
                    s_History.CopyTo(history, 0);
                }
                var sb = new StringBuilder("{\"type\":\"history\",\"logs\":[");
                for (var i = 0; i < history.Length; i++)
                {
                    if (i > 0) sb.Append(',');
                    sb.Append(history[i]);
                }
                sb.Append("]}");
                SendTo(client, sb.ToString());
                s_StateDirty = true;
            }
        }

        static void ReadLoop(TcpClient client)
        {
            try
            {
                using (var reader = new StreamReader(client.GetStream(), new UTF8Encoding(false)))
                {
                    string line;
                    while (s_Running && (line = reader.ReadLine()) != null)
                    {
                        if (line.Length > 0)
                            s_Incoming.Enqueue(line);
                    }
                }
            }
            catch (Exception)
            {
                // disconnected
            }

            Remove(client);
        }

        static void SendLoop()
        {
            while (s_Running)
            {
                string msg;
                try
                {
                    msg = s_Outgoing.Take();
                }
                catch (Exception)
                {
                    break;
                }

                if (msg == null)
                    continue;

                TcpClient[] clients;
                lock (s_Lock)
                    clients = s_Clients.ToArray();
                foreach (var c in clients)
                    SendTo(c, msg);
            }
        }

        static void SendTo(TcpClient client, string msg)
        {
            try
            {
                var bytes = Encoding.UTF8.GetBytes(msg + "\n");
                lock (client)
                    client.GetStream().Write(bytes, 0, bytes.Length);
            }
            catch (Exception)
            {
                Remove(client);
            }
        }

        static void Remove(TcpClient client)
        {
            lock (s_Lock)
                s_Clients.Remove(client);
            try { client.Close(); }
            catch (Exception) { /* ignored */ }
        }

        static void Broadcast(string json)
        {
            if (!s_Running)
                return;
            if (ConnectedClients == 0)
                return;
            s_Outgoing.Add(json);
        }

        // ------------------------------------------------------------------------------ events

        static void OnLog(string condition, string stackTrace, LogType type)
        {
            var json = "{\"type\":\"log\",\"logType\":" + Json.Str(type.ToString()) +
                       ",\"message\":" + Json.Str(condition) +
                       ",\"stackTrace\":" + Json.Str(stackTrace) +
                       ",\"time\":" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture) + "}";
            lock (s_History)
            {
                s_History.AddLast(json);
                while (s_History.Count > k_HistorySize)
                    s_History.RemoveFirst();
            }
            Broadcast(json);
        }

        static void OnPlayModeChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.ExitingEditMode)
            {
                lock (s_History)
                    s_History.Clear();
                Broadcast("{\"type\":\"playStarting\"}");
            }
            s_StateDirty = true;
        }

        static void OnCompilationStarted(object context)
        {
            s_CompileErrors = 0;
            s_CompileWarnings = 0;
            s_StateDirty = true;
        }

        static void OnAssemblyCompiled(string assemblyPath, CompilerMessage[] messages)
        {
            foreach (var m in messages)
            {
                if (m.type == CompilerMessageType.Error) s_CompileErrors++;
                else if (m.type == CompilerMessageType.Warning) s_CompileWarnings++;
            }
        }

        static void OnCompilationFinished(object context)
        {
            Broadcast("{\"type\":\"compileFinished\",\"errors\":" + s_CompileErrors + ",\"warnings\":" + s_CompileWarnings + "}");
            s_StateDirty = true;
        }

        static void Update()
        {
            string line;
            var handled = 0;
            while (handled++ < 32 && s_Incoming.TryDequeue(out line))
                Handle(line);

            var playing = EditorApplication.isPlaying;
            var paused = EditorApplication.isPaused;
            var compiling = EditorApplication.isCompiling;
            if (s_StateDirty || playing != s_LastPlaying || paused != s_LastPaused || compiling != s_LastCompiling)
            {
                s_StateDirty = false;
                s_LastPlaying = playing;
                s_LastPaused = paused;
                s_LastCompiling = compiling;
                Broadcast("{\"type\":\"state\",\"isPlaying\":" + Json.Bool(playing) +
                          ",\"isPaused\":" + Json.Bool(paused) +
                          ",\"isCompiling\":" + Json.Bool(compiling) + "}");
            }
        }

#pragma warning disable 0649 // filled by JsonUtility
        [Serializable]
        internal class Command
        {
            public string type;
            public int pid;
            // inspector.* requests (see PapyrusInspector)
            public int id;
            public int target;
            public int parent;
            public string path;
            public string script;
            public string name;
            public string value;
            // inspector.mirror* (see PapyrusInspectorMirror)
            public float width;
            public float height;
            public float x;
            public float y;
            public float dx;
            public float dy;
            public int button;
            public int clicks;
            public int mods;
            public string key;
            public string chr;
            // inspector.mirrorPlace: the IDE window and whether its inspector area is showing
            public long hwnd;
            public bool visible;
        }
#pragma warning restore 0649

        static int s_IdePid;

        /// <summary>The connected IDE's main process (it owns the IDE's windows), or 0 before its hello.</summary>
        internal static uint IdePid => s_IdePid > 0 ? (uint)s_IdePid : 0;

        static void Handle(string line)
        {
            Command cmd;
            try
            {
                cmd = JsonUtility.FromJson<Command>(line);
            }
            catch (Exception)
            {
                return;
            }

            if (cmd == null || string.IsNullOrEmpty(cmd.type))
                return;

            if (PapyrusInspector.Handle(cmd))
                return;

            switch (cmd.type)
            {
                case "hello":
                    s_IdePid = cmd.pid;
                    s_StateDirty = true;
                    break;
                case "play":
                    if (!EditorApplication.isPlaying)
                        EditorApplication.isPlaying = true;
                    break;
                case "stop":
                    if (EditorApplication.isPlaying)
                        EditorApplication.isPlaying = false;
                    break;
                case "pause":
                    EditorApplication.isPaused = !EditorApplication.isPaused;
                    break;
                case "step":
                    EditorApplication.Step();
                    break;
                case "refresh":
                    if (!EditorApplication.isCompiling && !EditorApplication.isUpdating)
                        AssetDatabase.Refresh();
                    break;
                case "sync":
                    new ProjectGeneration().Sync();
                    break;
            }
        }

        // ------------------------------------------------------------------------------ API

        /// <summary>Send one JSON message to every connected IDE.</summary>
        public static void Send(string json)
        {
            Broadcast(json);
        }

#if UNITY_EDITOR_WIN
        [DllImport("user32.dll")]
        static extern bool AllowSetForegroundWindow(int dwProcessId);
#endif

        /// <summary>Unity is in the foreground; let the IDE bring its window to the front.</summary>
        public static void AllowForeground()
        {
#if UNITY_EDITOR_WIN
            try
            {
                AllowSetForegroundWindow(s_IdePid > 0 ? s_IdePid : -1);
            }
            catch (Exception)
            {
                // ignored
            }
#endif
        }
    }

    internal static class Json
    {
        public static string Bool(bool b)
        {
            return b ? "true" : "false";
        }

        public static string Str(string s)
        {
            if (s == null)
                return "null";
            var sb = new StringBuilder(s.Length + 2);
            sb.Append('"');
            foreach (var c in s)
            {
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    case '\b': sb.Append("\\b"); break;
                    case '\f': sb.Append("\\f"); break;
                    default:
                        if (c < 0x20)
                            sb.Append("\\u").Append(((int)c).ToString("x4"));
                        else
                            sb.Append(c);
                        break;
                }
            }
            sb.Append('"');
            return sb.ToString();
        }
    }
}
