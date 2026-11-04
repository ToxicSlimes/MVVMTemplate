using System;
using System.Collections.Generic;
using System.Linq;

namespace MVVM.EcsBackend
{
    public enum Backend { Morpeh, StaticEcs }
    public static class BackendDefines
    {
        public static string Select(string defines, Backend backend)
        {
            var parts = (defines ?? "").Split(';').Select(x => x.Trim()).Where(x => x.Length != 0).ToList();
            const string disable = "MORPEH_DISABLE_AUTOINITIALIZATION";
            const string owned = "MVVM_ECS_OWNS_MORPEH_DISABLE";
            bool ownsDisable = parts.Contains(owned);
            parts.RemoveAll(x => x == "MORPEH" || x == "STATIC_ECS");
            if (backend == Backend.StaticEcs)
            {
                if (!parts.Contains(disable)) { parts.Add(disable); parts.Add(owned); }
                parts.Add("STATIC_ECS");
            }
            else
            {
                if (ownsDisable) parts.RemoveAll(x => x == disable || x == owned);
                parts.Add("MORPEH");
            }
            return string.Join(";", parts);
        }
        public static string Validate(string defines)
        {
            var parts = (defines ?? "").Split(';').Select(x => x.Trim()).ToArray();
            bool m = parts.Contains("MORPEH"), s = parts.Contains("STATIC_ECS");
            if (m == s) return m ? "both MORPEH and STATIC_ECS are present" : "neither MORPEH nor STATIC_ECS is present";
            if (s && !parts.Contains("MORPEH_DISABLE_AUTOINITIALIZATION")) return "STATIC_ECS requires MORPEH_DISABLE_AUTOINITIALIZATION";
            return null;
        }
        public static void Apply(IReadOnlyList<string> targets, Backend backend, Func<string, string> read, Action<string, string> write)
        {
            var before = new Dictionary<string, string>();
            var after = new Dictionary<string, string>();
            foreach (var target in targets) { before.Add(target, read(target)); after.Add(target, Select(before[target], backend)); }
            var attempted = new List<string>();
            try
            {
                foreach (var target in targets)
                {
                    if (read(target) != before[target]) throw new InvalidOperationException(target + " changed while selecting backend");
                    if (before[target] == after[target]) continue;
                    attempted.Add(target);
                    write(target, after[target]);
                    if (read(target) != after[target]) throw new InvalidOperationException(target + " did not retain requested defines");
                }
                foreach (var target in targets)
                    if (read(target) != after[target]) throw new InvalidOperationException(target + " changed during selection");
            }
            catch (Exception failure)
            {
                var issues = new List<string>();
                for (int i = attempted.Count - 1; i >= 0; i--)
                {
                    var target = attempted[i];
                    try
                    {
                        var current = read(target);
                        if (current == before[target]) continue;
                        if (current != after[target]) { issues.Add(target + " changed externally; not restored"); continue; }
                        write(target, before[target]);
                        if (read(target) != before[target]) issues.Add(target + " restore verification failed");
                    }
                    catch (Exception e) { issues.Add(target + " restore failed: " + e.Message); }
                }
                throw new InvalidOperationException("Backend selection failed: " + failure.Message + (issues.Count == 0 ? "; attempted changes restored" : "; manual repair required: " + string.Join(", ", issues)), failure);
            }
        }
    }
}
