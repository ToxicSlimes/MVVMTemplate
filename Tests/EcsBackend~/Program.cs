using System;
using System.Collections.Generic;
using MVVM.EcsBackend;

class Program
{
    static int count;
    static void Check(bool condition, string name) { if (!condition) throw new Exception(name); count++; Console.WriteLine("PASS " + name); }
    static void Main()
    {
        var selected = BackendDefines.Select("OTHER;MORPEH", Backend.StaticEcs);
        Check(selected.Contains("MORPEH_DISABLE_AUTOINITIALIZATION"), "static disables Morpeh auto-init");
        Check(BackendDefines.Select(selected, Backend.Morpeh) == "OTHER;MORPEH", "restore owned disable flag");
        var userOwned = BackendDefines.Select("OTHER;MORPEH;MORPEH_DISABLE_AUTOINITIALIZATION", Backend.StaticEcs);
        Check(BackendDefines.Select(userOwned, Backend.Morpeh) == "OTHER;MORPEH_DISABLE_AUTOINITIALIZATION;MORPEH", "preserve user-owned disable flag");
        Check(userOwned == "OTHER;MORPEH_DISABLE_AUTOINITIALIZATION;STATIC_ECS", "select preserves user defines");
        Check(BackendDefines.Select("STATIC_ECS;X;STATIC_ECS", Backend.Morpeh) == "X;MORPEH", "switch removes duplicate backend flags");
        Check(BackendDefines.Validate("X; MORPEH ") == null && BackendDefines.Validate("STATIC_ECS;MORPEH_DISABLE_AUTOINITIALIZATION") == null, "valid configurations");
        Check(BackendDefines.Validate("X") != null && BackendDefines.Validate("MORPEH;STATIC_ECS") != null, "invalid configurations");
        var d = new Dictionary<string, string> { ["A"] = "MORPEH;KEEP", ["B"] = "MORPEH;OTHER" };
        BackendDefines.Apply(new[] { "A", "B" }, Backend.StaticEcs, t => d[t], (t, v) => d[t] = v);
        Check(d["A"] == BackendDefines.Select("MORPEH;KEEP", Backend.StaticEcs) && d["B"] == BackendDefines.Select("MORPEH;OTHER", Backend.StaticEcs), "all targets switched");
        d["A"] = "MORPEH;KEEP"; d["B"] = "MORPEH;OTHER";
        try { BackendDefines.Apply(new[] { "A", "B" }, Backend.StaticEcs, t => d[t], (t, v) => { if (t == "B") throw new Exception("write failure"); d[t] = v; }); throw new Exception("failure expected"); } catch (InvalidOperationException) { }
        Check(d["A"] == "MORPEH;KEEP" && d["B"] == "MORPEH;OTHER", "rollback after write failure");
        d["A"] = "MORPEH;KEEP"; d["B"] = "MORPEH;OTHER";
        try { BackendDefines.Apply(new[] { "A", "B" }, Backend.StaticEcs, t => d[t], (t, v) => { d[t] = v; if (t == "B") { d["A"] = "EXTERNAL"; throw new Exception("failure"); } }); throw new Exception("failure expected"); } catch (InvalidOperationException e) { Check(e.Message.Contains("manual repair required"), "concurrent edit reported"); }
        Check(d["A"] == "EXTERNAL" && d["B"] == "MORPEH;OTHER", "rollback preserves concurrent edit");
        Console.WriteLine(count + " tests passed");
    }
}
