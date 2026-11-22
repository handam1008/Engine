using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_6000_5_OR_NEWER
using System.Collections.Generic;
using System.Globalization;
using System.Text;
#endif

namespace Papyrus.Ide.Editor
{
    /// <summary>
    /// The int ids the IDE uses for Unity objects (hierarchy, inspector, selection).
    /// Before Unity 6.5 they are the instance ids. Unity 6.5 removed int instance ids
    /// (Object.GetInstanceID is an error, EntityId has no int form and its raw value is 64-bit, too big
    /// for a JavaScript number), so there the package hands out its own small ints for EntityIds and
    /// keeps the table across domain reloads, like instance ids survived them before.
    /// </summary>
    internal static class PapyrusIds
    {
#if UNITY_6000_5_OR_NEWER
        const string k_Key = "Papyrus.Ids";
        const int k_Max = 400000; // a fresh table after this many objects (old ids just stop resolving)
        static readonly Dictionary<ulong, int> s_ToId = new Dictionary<ulong, int>();
        static readonly Dictionary<int, ulong> s_ToRaw = new Dictionary<int, ulong>();
        static int s_Next = 1;

        static PapyrusIds()
        {
            Load();
            AssemblyReloadEvents.beforeAssemblyReload += Save;
        }

        public static int Of(Object o)
        {
            if (!o) return 0;
            var raw = EntityId.ToULong(o.GetEntityId());
            if (s_ToId.TryGetValue(raw, out var id)) return id;
            if (s_ToRaw.Count >= k_Max) { s_ToId.Clear(); s_ToRaw.Clear(); }
            id = s_Next++;
            s_ToId[raw] = id;
            s_ToRaw[id] = raw;
            return id;
        }

        public static Object Find(int id)
        {
            if (id == 0 || !s_ToRaw.TryGetValue(id, out var raw)) return null;
            return EditorUtility.EntityIdToObject(EntityId.FromULong(raw));
        }

        static void Save()
        {
            var sb = new StringBuilder();
            sb.Append(s_Next.ToString(CultureInfo.InvariantCulture));
            foreach (var kv in s_ToRaw)
                sb.Append(';').Append(kv.Key.ToString(CultureInfo.InvariantCulture)).Append(':').Append(kv.Value.ToString(CultureInfo.InvariantCulture));
            SessionState.SetString(k_Key, sb.ToString());
        }

        static void Load()
        {
            var s = SessionState.GetString(k_Key, "");
            if (string.IsNullOrEmpty(s)) return;
            var parts = s.Split(';');
            if (int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var next)) s_Next = next;
            for (var i = 1; i < parts.Length; i++)
            {
                var c = parts[i].IndexOf(':');
                if (c <= 0) continue;
                if (!int.TryParse(parts[i].Substring(0, c), NumberStyles.Integer, CultureInfo.InvariantCulture, out var id)) continue;
                if (!ulong.TryParse(parts[i].Substring(c + 1), NumberStyles.Integer, CultureInfo.InvariantCulture, out var raw)) continue;
                s_ToRaw[id] = raw;
                s_ToId[raw] = id;
                if (id >= s_Next) s_Next = id + 1;
            }
        }
#else
        public static int Of(Object o) => o ? o.GetInstanceID() : 0;

        public static Object Find(int id)
        {
            // obsolete since Unity 6.3 (EntityIdToObject), but still works and the package supports 2021.3+
#pragma warning disable 0618
            return id == 0 ? null : EditorUtility.InstanceIDToObject(id);
#pragma warning restore 0618
        }
#endif
    }
}
