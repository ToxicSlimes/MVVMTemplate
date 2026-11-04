using System;
using System.Collections.Generic;
using System.Reflection;
using MVVM.EcsBackend;
using NUnit.Framework;
using UnityEditor;

namespace MVVM.EcsBackend.Tests
{
    public sealed class EcsBackendSelectorTests
    {
        [Test]
        public void Select_RoundTrip_RemovesOnlySelectorOwnedDisable()
        {
            var selected = BackendDefines.Select("KEEP;MORPEH", Backend.StaticEcs);
            Assert.That(selected, Is.EqualTo("KEEP;MORPEH_DISABLE_AUTOINITIALIZATION;MVVM_ECS_OWNS_MORPEH_DISABLE;STATIC_ECS"));
            Assert.That(BackendDefines.Select(selected, Backend.Morpeh), Is.EqualTo("KEEP;MORPEH"));
        }

        [Test]
        public void Select_RoundTrip_PreservesUserOwnedDisable()
        {
            var selected = BackendDefines.Select("KEEP;MORPEH;MORPEH_DISABLE_AUTOINITIALIZATION", Backend.StaticEcs);
            Assert.That(BackendDefines.Select(selected, Backend.Morpeh), Is.EqualTo("KEEP;MORPEH_DISABLE_AUTOINITIALIZATION;MORPEH"));
        }

        [TestCase(null, false)]
        [TestCase("MORPEH;STATIC_ECS", false)]
        [TestCase("STATIC_ECS", false)]
        [TestCase(" STATIC_ECS ; MORPEH_DISABLE_AUTOINITIALIZATION ", true)]
        [TestCase("MORPEH", true)]
        public void Validate_EnforcesBackendAndDisable(string defines, bool valid)
        {
            Assert.That(BackendDefines.Validate(defines) == null, Is.EqualTo(valid));
        }

        [Test]
        public void Apply_WriteFailure_RestoresAlreadyChangedTargets()
        {
            var values = new Dictionary<string, string> { ["A"] = "MORPEH;KEEP", ["B"] = "MORPEH" };
            Assert.Throws<InvalidOperationException>(() => BackendDefines.Apply(new[] { "A", "B" }, Backend.StaticEcs,
                target => values[target], (target, value) => { if (target == "B") throw new Exception("failure"); values[target] = value; }));
            Assert.That(values["A"], Is.EqualTo("MORPEH;KEEP"));
            Assert.That(values["B"], Is.EqualTo("MORPEH"));
        }

        [Test]
        public void Apply_ConcurrentChange_IsNotOverwrittenByRollback()
        {
            var values = new Dictionary<string, string> { ["A"] = "MORPEH", ["B"] = "MORPEH" };
            var error = Assert.Throws<InvalidOperationException>(() => BackendDefines.Apply(new[] { "A", "B" }, Backend.StaticEcs,
                target => values[target], (target, value) => { values[target] = value; if (target == "B") { values["A"] = "EXTERNAL"; throw new Exception("failure"); } }));
            Assert.That(error.Message, Does.Contain("manual repair required"));
            Assert.That(values["A"], Is.EqualTo("EXTERNAL"));
            Assert.That(values["B"], Is.EqualTo("MORPEH"));
        }

        // Synchronous test: no yield, SaveAssets or Refresh while defines differ.
        // Reload remains locked until restoration is complete, including on assertion failure.
        [Test]
        public void PlayerSettings_MenuBoundary_SwitchesAllTargetsAndRestoresOriginals()
        {
            Assert.That(EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling, Is.False,
                "Run only in idle Edit Mode.");
            const BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic;
            var readMethod = typeof(EcsBackendMenu).GetMethod("Read", flags);
            var writeMethod = typeof(EcsBackendMenu).GetMethod("Write", flags);
            Assert.That(readMethod, Is.Not.Null);
            Assert.That(writeMethod, Is.Not.Null);
            var read = (Func<string, string>)Delegate.CreateDelegate(typeof(Func<string, string>), readMethod);
            var write = (Action<string, string>)Delegate.CreateDelegate(typeof(Action<string, string>), writeMethod);
            var targets = new[] { "Standalone", "Android", "WebGL" };
            var before = new Dictionary<string, string>();
            var lastWritten = new Dictionary<string, string>();
            var issues = new List<string>();
            EditorApplication.LockReloadAssemblies();
            try
            {
                foreach (var target in targets) before.Add(target, read(target));
                Action<string, string> trackedWrite = (target, value) => { lastWritten[target] = value; write(target, value); };
                foreach (var backend in new[] { Backend.StaticEcs, Backend.Morpeh })
                {
                    var expected = new Dictionary<string, string>();
                    foreach (var target in targets) expected.Add(target, BackendDefines.Select(read(target), backend));
                    BackendDefines.Apply(targets, backend, read, trackedWrite);
                    foreach (var target in targets)
                    {
                        Assert.That(read(target), Is.EqualTo(expected[target]), target);
                        Assert.That(BackendDefines.Validate(read(target)), Is.Null, target);
                    }
                }
            }
            finally
            {
                foreach (var target in targets)
                {
                    if (!before.ContainsKey(target)) continue;
                    try
                    {
                        var current = read(target);
                        if (current == before[target]) continue;
                        if (!lastWritten.TryGetValue(target, out var written) || current != written)
                        {
                            issues.Add(target + " changed externally; not restored");
                            continue;
                        }
                        write(target, before[target]);
                        if (read(target) != before[target]) issues.Add(target + " restore verification failed");
                    }
                    catch (Exception e) { issues.Add(target + " restore failed: " + e.Message); }
                }
                EditorApplication.UnlockReloadAssemblies();
                if (issues.Count != 0) throw new InvalidOperationException("Manual repair required: " + string.Join(", ", issues));
            }
        }
    }
}
