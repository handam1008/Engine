using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Unity.CodeEditor;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Papyrus.Ide.Editor
{
    /// <summary>
    /// Registers PAPyrus as an External Script Editor (Preferences > External Tools).
    /// </summary>
    [InitializeOnLoad]
    public class PapyrusCodeEditor : IExternalCodeEditor
    {
        internal const string DisplayName = "PAPyrus";

        static readonly HashSet<string> s_SupportedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".cs", ".shader", ".hlsl", ".hlslinc", ".cginc", ".compute", ".glsl", ".raytrace", ".json", ".asmdef", ".asmref",
            ".txt", ".xml", ".uxml", ".uss", ".tss", ".md", ".yaml", ".yml", ".rsp", ".inputactions", ".csproj", ".sln", ".editorconfig"
        };

        readonly ProjectGeneration m_Generator = new ProjectGeneration();
        string m_Installation;

        static PapyrusCodeEditor()
        {
            CodeEditor.Register(new PapyrusCodeEditor());
            EditorApplication.delayCall += RepairLegacySelection;
        }

        static void RepairLegacySelection()
        {
            var launcher = Discovery.DevelopmentLauncherFor(CodeEditor.CurrentEditorInstallation);
            if (launcher != null) CodeEditor.Editor.SetCodeEditor(launcher);
        }

        public CodeEditor.Installation[] Installations
        {
            get
            {
                return Discovery.Find()
                    .Select(p => new CodeEditor.Installation { Name = DisplayName, Path = p })
                    .ToArray();
            }
        }

        public bool TryGetInstallationForPath(string editorPath, out CodeEditor.Installation installation)
        {
            if (!string.IsNullOrEmpty(editorPath) && Discovery.IsPapyrus(editorPath))
            {
                installation = new CodeEditor.Installation { Name = DisplayName, Path = editorPath };
                return true;
            }

            installation = default(CodeEditor.Installation);
            return false;
        }

        public void Initialize(string editorInstallationPath)
        {
            m_Installation = editorInstallationPath;
        }

        public void OnGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("PAPyrus", EditorStyles.boldLabel);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("프로젝트 파일(.csproj)을 만들 패키지", EditorStyles.miniBoldLabel);
            EditorGUILayout.HelpBox("체크하지 않은 패키지(DOTween, UniTask 등)도 Unity가 컴파일한 DLL로 참조되어 자동완성이 됩니다. " +
                                    "체크하면 패키지 소스까지 IDE에서 열고 탐색할 수 있어요. 바꾸면 바로 다시 생성되고 IDE가 자동으로 다시 불러옵니다.", MessageType.None);
            EditorGUI.BeginChangeCheck();
            EditorGUI.indentLevel++;
            PapyrusSettings.Embedded = EditorGUILayout.Toggle("Embedded 패키지", PapyrusSettings.Embedded);
            PapyrusSettings.Local = EditorGUILayout.Toggle("Local 패키지", PapyrusSettings.Local);
            PapyrusSettings.Registry = EditorGUILayout.Toggle("Registry 패키지", PapyrusSettings.Registry);
            PapyrusSettings.Git = EditorGUILayout.Toggle("Git 패키지", PapyrusSettings.Git);
            PapyrusSettings.BuiltIn = EditorGUILayout.Toggle("Built-in 패키지", PapyrusSettings.BuiltIn);
            EditorGUI.indentLevel--;
            // the generated projects have to follow the switches right away, or the IDE keeps the old set
            if (EditorGUI.EndChangeCheck())
                EditorApplication.delayCall += () => m_Generator.Sync();

            EditorGUILayout.Space();
            PapyrusSettings.SyncOnOpen = EditorGUILayout.Toggle("열 때마다 프로젝트 파일 갱신", PapyrusSettings.SyncOnOpen);

            EditorGUILayout.Space();
            if (GUILayout.Button("프로젝트 파일 다시 생성", GUILayout.Width(200)))
                m_Generator.Sync();
        }

        public void SyncIfNeeded(string[] addedFiles, string[] deletedFiles, string[] movedFiles, string[] movedFromFiles, string[] importedFiles)
        {
            var affected = (addedFiles ?? new string[0])
                .Concat(deletedFiles ?? new string[0])
                .Concat(movedFiles ?? new string[0])
                .Concat(movedFromFiles ?? new string[0]);
            m_Generator.SyncIfNeeded(affected, importedFiles ?? new string[0]);
        }

        public void SyncAll()
        {
            AssetDatabase.Refresh();
            m_Generator.Sync();
        }

        public bool OpenProject(string filePath = "", int line = -1, int column = -1)
        {
            if (!string.IsNullOrEmpty(filePath) && !s_SupportedExtensions.Contains(Path.GetExtension(filePath)))
                return false; // let Unity open it with the default app

            var root = ProjectGeneration.ProjectDirectory;
            var fullPath = string.IsNullOrEmpty(filePath) ? string.Empty : Path.GetFullPath(filePath);
            line = Math.Max(1, line);
            column = Math.Max(1, column);

            var selected = m_Installation ?? CodeEditor.CurrentEditorInstallation;
            var exe = Discovery.IsPapyrus(selected) ? selected :
                Discovery.DevelopmentLauncherFor(selected) ?? Discovery.Find().FirstOrDefault();
            if (exe == null)
            {
                Debug.LogWarning("[PAPyrus] 실행 파일을 찾을 수 없어요. Preferences > External Tools에서 PAPyrus 실행 파일을 선택해 주세요.");
                return false;
            }

            var template = string.IsNullOrEmpty(fullPath) ? "--project $(ProjectPath)" :
                "--project $(ProjectPath) --file $(File) --line $(Line) --column $(Column)";
            var args = CodeEditor.ParseArgument(template, fullPath, line, column);

            try
            {
                if (PapyrusSettings.SyncOnOpen || !m_Generator.HasSolutionBeenGenerated())
                    m_Generator.Sync();
            }
            catch (Exception e)
            {
                Debug.LogWarning("[PAPyrus] 프로젝트 파일 갱신 실패: " + e.Message);
            }

            try
            {
                PapyrusBridge.AllowForeground();
                ProcessStartInfo psi;
                if (exe.EndsWith(".app", StringComparison.OrdinalIgnoreCase))
                    psi = new ProcessStartInfo("open", "-n " + CodeEditor.QuoteForProcessStart(exe) + " --args " + args) { UseShellExecute = false };
                else
                    psi = new ProcessStartInfo(exe, args) { UseShellExecute = true, WorkingDirectory = root };
                Process.Start(psi);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("[PAPyrus] 실행 실패: " + e.Message);
                return false;
            }
        }

        [MenuItem("Tools/PAPyrus/프로젝트 파일 다시 생성", priority = 100)]
        static void RegenerateMenu()
        {
            new ProjectGeneration().Sync();
            Debug.Log("[PAPyrus] 프로젝트 파일을 다시 생성했어요.");
        }

        [MenuItem("Tools/PAPyrus/PAPyrus에서 열기", priority = 101)]
        static void OpenMenu()
        {
            new PapyrusCodeEditor().OpenProject();
        }
    }

    internal static class Discovery
    {
        public static bool IsPapyrus(string path)
        {
            if (string.IsNullOrEmpty(path) || !Path.IsPathRooted(path)) return false;
            var name = Path.GetFileName(path.TrimEnd('/', '\\'));
            if (!name.StartsWith("papyrus", StringComparison.OrdinalIgnoreCase)) return false;
#if UNITY_EDITOR_WIN
            return name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) && File.Exists(path);
#else
            if (name.EndsWith(".app", StringComparison.OrdinalIgnoreCase)) return Directory.Exists(path);
            return File.Exists(path);
#endif
        }

        public static string DevelopmentLauncherFor(string path)
        {
            if (string.IsNullOrEmpty(path) || !Path.IsPathRooted(path) ||
                !path.Replace('\\', '/').EndsWith("/node_modules/electron/dist/electron.exe", StringComparison.OrdinalIgnoreCase))
                return null;
            var launcher = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(path), "..", "..", ".cache", "papyrus", "PAPyrus.exe"));
            return File.Exists(launcher) ? launcher : null;
        }

        static IEnumerable<string> UserDataDirs()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var home = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            yield return Path.Combine(appData, "PAPyrus");
            yield return Path.Combine(home, "Library", "Application Support", "PAPyrus");
            yield return Path.Combine(home, ".config", "PAPyrus");
        }

        /// <summary>Microsoft.Unity.Analyzers downloaded by the IDE, if present.</summary>
        public static string UnityAnalyzers
        {
            get
            {
                foreach (var dir in UserDataDirs())
                {
                    var p = Path.Combine(dir, "analyzers", "Microsoft.Unity.Analyzers.dll");
                    if (File.Exists(p))
                        return p;
                }
                return null;
            }
        }

        public static IEnumerable<string> Find()
        {
            var candidates = new List<string>();
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            // The IDE writes its own location here on every launch (covers portable installs).
            foreach (var hint in UserDataDirs().Select(d => Path.Combine(d, "install-path.txt")))
            {
                try
                {
                    if (File.Exists(hint))
                        candidates.Add(File.ReadAllText(hint).Trim());
                }
                catch (Exception)
                {
                    // ignore unreadable hint files
                }
            }

            candidates.Add(Path.Combine(localAppData, "Programs", "PAPyrus", "PAPyrus.exe"));
            candidates.Add(Path.Combine(localAppData, "Programs", "papyrus", "PAPyrus.exe"));
            candidates.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "PAPyrus", "PAPyrus.exe"));
            candidates.Add("/Applications/PAPyrus.app");

            var result = new List<string>();
            foreach (var c in candidates)
            {
                if (string.IsNullOrEmpty(c))
                    continue;
                var path = c;
                // macOS: install-path.txt points inside the bundle
                var idx = path.IndexOf(".app/", StringComparison.OrdinalIgnoreCase);
                if (idx > 0)
                    path = path.Substring(0, idx + 4);
                if (!IsPapyrus(path)) path = DevelopmentLauncherFor(path);
                if (path != null && !result.Contains(path, StringComparer.OrdinalIgnoreCase))
                    result.Add(path);
            }

            return result;
        }
    }

    internal static class PapyrusSettings
    {
        const string k_Prefix = "Papyrus.Ide.";

        public static bool Embedded { get { return Get("Embedded", true); } set { Set("Embedded", value); } }
        public static bool Local { get { return Get("Local", true); } set { Set("Local", value); } }
        public static bool Registry { get { return Get("Registry", false); } set { Set("Registry", value); } }
        public static bool Git { get { return Get("Git", false); } set { Set("Git", value); } }
        public static bool BuiltIn { get { return Get("BuiltIn", false); } set { Set("BuiltIn", value); } }
        public static bool SyncOnOpen { get { return Get("SyncOnOpen", false); } set { Set("SyncOnOpen", value); } }

        static bool Get(string key, bool def)
        {
            return EditorPrefs.GetBool(k_Prefix + key, def);
        }

        static void Set(string key, bool value)
        {
            var k = k_Prefix + key;
            if (EditorPrefs.HasKey(k) && EditorPrefs.GetBool(k) == value)
                return;
            EditorPrefs.SetBool(k, value);
        }
    }
}
