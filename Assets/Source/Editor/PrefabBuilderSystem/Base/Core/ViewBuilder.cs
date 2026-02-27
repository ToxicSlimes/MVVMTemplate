#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Core.Editor.PrefabBuilderSystem.Elements;
using Core.Editor.PrefabBuilderSystem.Validation;

namespace Core.Editor.PrefabBuilderSystem.Core
{
    public abstract class ViewBuilder
    {
        public PrefabBuilderContext Context { get; private set; }

        protected abstract string PrefabPath { get; }
        protected virtual Vector2 ReferenceResolution => new(1920, 1080);
        protected virtual float MatchWidthOrHeight => 0.5f;
        protected virtual bool ValidateBeforeSave => true;
        protected virtual bool ShowValidationDialog => true;

        public ValidationReport LastValidationReport { get; private set; }

        public void Build()
        {
            EnsureDirectoryExists(PrefabPath);

            Context = new PrefabBuilderContext
            {
                Root = new GameObject(Path.GetFileNameWithoutExtension(PrefabPath))
            };

            SetupCanvas(Context.Root);
            BuildStructure();

            if (ValidateBeforeSave && UIValidationSystem.Instance.ValidateBeforeBuild)
            {
                LastValidationReport = UIValidationSystem.Instance.Validate(Context.Root, Context);

                if (!UIValidationSystem.Instance.CanBuild(LastValidationReport))
                {
                    LastValidationReport.LogToConsole();
                    Debug.LogError($"<color=red>✗ Build failed validation: {LastValidationReport.GetSummary()}</color>");
                    Object.DestroyImmediate(Context.Root);
                    return;
                }

                if (LastValidationReport.HasWarnings)
                {
                    LastValidationReport.LogToConsole();
                    
                    if (ShowValidationDialog)
                    {
                        var continueAnyway = EditorUtility.DisplayDialog(
                            "Validation Warnings",
                            $"Found {LastValidationReport.WarningCount} warnings:\n\n{LastValidationReport.GetSummary()}\n\nContinue with build?",
                            "Continue",
                            "Cancel");

                        if (!continueAnyway)
                        {
                            Object.DestroyImmediate(Context.Root);
                            return;
                        }
                    }
                }
            }

            PrefabUtility.SaveAsPrefabAsset(Context.Root, PrefabPath);
            Object.DestroyImmediate(Context.Root);

            Debug.Log($"<color=green>✓ Prefab created: {PrefabPath}</color>");

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null)
                return;
            
            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);
        }

        public ValidationReport ValidateOnly()
        {
            Context = new PrefabBuilderContext
            {
                Root = new GameObject(Path.GetFileNameWithoutExtension(PrefabPath) + "_Validation")
            };

            SetupCanvas(Context.Root);
            BuildStructure();

            LastValidationReport = UIValidationSystem.Instance.Validate(Context.Root, Context);

            Object.DestroyImmediate(Context.Root);
            return LastValidationReport;
        }

        protected abstract void BuildStructure();

        protected virtual void SetupCanvas(GameObject go)
        {
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            Context.Canvas = canvas;

            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.matchWidthOrHeight = MatchWidthOrHeight;
            Context.CanvasScaler = scaler;

            var raycaster = go.AddComponent<GraphicRaycaster>();
            Context.Raycaster = raycaster;

            var canvasGroup = go.AddComponent<CanvasGroup>();
            Context.CanvasGroup = canvasGroup;

            // Ensure RectTransform is properly initialized
            var rt = go.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.localScale = Vector3.one;
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                rt.pivot = new Vector2(0.5f, 0.5f);
            }
        }

        protected RectTransform Root => Context.Root.GetComponent<RectTransform>() 
            ?? Context.Root.AddComponent<RectTransform>();

        protected static void EnsureDirectoryExists(string path)
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        protected static bool TryLoadPrefab(string path, out GameObject prefab, string displayName = null)
        {
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab != null)
                return true;
            
            Debug.LogError($"Prefab '{displayName ?? Path.GetFileNameWithoutExtension(path)}' not found at: {path}");
            return false;

        }

        protected static GameObject SpawnPrefab(GameObject prefab, Transform parent, string name)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.name = name;
            return instance;
        }

        protected static void SaveElementPrefab(GameObject root, string path)
        {
            EnsureDirectoryExists(path);
            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            Debug.Log($"<color=cyan>Element Prefab Created:</color> {path}");
        }
    }

    public abstract class ViewBuilder<TView> : ViewBuilder where TView : MonoBehaviour
    {
        protected virtual void WireFields(TView view)
        {
        }

        protected override void BuildStructure()
        {
            BuildViewStructure();
            
            var view = Context.Root.AddComponent<TView>();
            WireFields(view);
        }

        protected abstract void BuildViewStructure();
    }
}
#endif
