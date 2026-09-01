#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Core.Editor.CI
{
    public static class UnityCompileCheck
    {
        public static void Run()
        {
            if (EditorUtility.scriptCompilationFailed)
            {
                Debug.LogError("[CI] Script compilation failed.");
                EditorApplication.Exit(1);
                return;
            }

            Debug.Log("[CI] Script compilation succeeded.");
            EditorApplication.Exit(0);
        }
    }
}
#endif
