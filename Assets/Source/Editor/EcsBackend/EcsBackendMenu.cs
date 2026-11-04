using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace MVVM.EcsBackend
{
    public static class EcsBackendMenu
    {
        private static readonly string[] Targets = { "Standalone", "Android", "WebGL" };
        private static NamedBuildTarget Target(string name)
        {
            return NamedBuildTarget.FromBuildTargetGroup((BuildTargetGroup)Enum.Parse(typeof(BuildTargetGroup), name));
        }
        private static string Read(string target) => PlayerSettings.GetScriptingDefineSymbols(Target(target));
        private static void Write(string target, string defines) => PlayerSettings.SetScriptingDefineSymbols(Target(target), defines);

        [MenuItem("Tools/MVVM/ECS Backend/MORPEH")]
        private static void Morpeh() => Select(Backend.Morpeh);
        [MenuItem("Tools/MVVM/ECS Backend/STATIC_ECS")]
        private static void StaticEcs() => Select(Backend.StaticEcs);
        [MenuItem("Tools/MVVM/ECS Backend/Validate")]
        private static void ValidateMenu()
        {
            var lines = Targets.Select(t => t + ": " + (BackendDefines.Validate(Read(t)) ?? "valid")).ToArray();
            var backends = Targets.Select(t => Read(t).Split(';').Any(x => x.Trim() == "STATIC_ECS")).Distinct().Count();
            bool invalid = Targets.Any(t => BackendDefines.Validate(Read(t)) != null) || backends != 1;
            var message = string.Join("\n", lines) + (backends != 1 ? "\nTargets use different backends." : "") + "\nMORPEH_DISABLE_AUTOINITIALIZATION is user-owned; verify package auto-initialization separately.";
            EditorUtility.DisplayDialog("ECS Backend validation", message, "OK");
            if (invalid) Debug.LogError("ECS Backend validation failed: " + message); else Debug.Log("ECS Backend validation: " + message);
        }
        private static void Select(Backend backend)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
            {
                EditorUtility.DisplayDialog("ECS Backend", "Stop Play Mode and wait for compilation before switching.", "OK");
                return;
            }
            if (!EditorUtility.DisplayDialog("ECS Backend", "Set " + backend + " for Standalone, Android and WebGL? Existing user-owned defines are preserved; Morpeh auto-initialization is disabled for StaticEcs.", "Apply", "Cancel")) return;
            EditorApplication.LockReloadAssemblies();
            try
            {
                BackendDefines.Apply(Targets, backend, Read, Write);
                Debug.Log("ECS Backend selected: " + backend + ". Verify Morpeh package auto-initialization separately.");
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                EditorUtility.DisplayDialog("ECS Backend selection failed", e.Message, "OK");
            }
            finally { EditorApplication.UnlockReloadAssemblies(); }
        }
    }
}
