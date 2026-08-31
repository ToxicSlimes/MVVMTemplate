#if UNITY_EDITOR
using System;
using System.IO;
using Core.Editor.PrefabBuilderSystem.Elements;
using Core.Editor.PrefabBuilderSystem.Theming;
using Core.Editor.PrefabBuilderSystem.Tools;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Editor.PrefabBuilderSystem.Core
{
    /// <summary>
    /// Base for full-screen view builders. Lays down the background, hands the body to the derived
    /// builder and applies panel animators, so a concrete builder only describes its own content.
    /// </summary>
    public abstract class PanelViewBuilder<TView> : ViewBuilder<TView> where TView : MonoBehaviour
    {
        private const string BackgroundName = "Background";

        private static readonly Color DeepSpaceFallback = new(0.024f, 0.043f, 0.102f, 1f);

        /// <summary>Set to false for overlays and HUDs that must stay transparent.</summary>
        protected virtual bool HasBackground => true;

        /// <summary>
        /// Set to false for popups that live under a foreign canvas and must not carry one of their own.
        /// </summary>
        protected virtual bool UseBasePanelPrefab => true;

        protected virtual Color BackgroundColor => DesignSystemProvider.Colors?.deepSpace ?? DeepSpaceFallback;

        /// <summary>
        /// Starts from the shared base panel prefab, so every saved view becomes its Prefab Variant
        /// and inherits canvas, scaler and background from one place.
        /// </summary>
        protected override GameObject CreateRoot()
        {
            if (!UseBasePanelPrefab)
                return base.CreateRoot();

            var basePrefab = BaseViewPanelGenerator.LoadOrBuild();

            if (basePrefab == null)
            {
                Debug.LogWarning($"[PanelViewBuilder] Base panel prefab missing, {GetType().Name} falls back to a plain root.");
                return base.CreateRoot();
            }

            var root = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab);
            root.name = Path.GetFileNameWithoutExtension(PrefabPath);
            return root;
        }

        protected sealed override void BuildViewStructure()
        {
            ApplyBackground();
            BuildPanelStructure();
            ApplyPanelAnimators();
        }

        protected abstract void BuildPanelStructure();

        protected virtual void ApplyPanelAnimators()
        {
        }

        protected void AddAnimator(GameObject target, int order, string preset = null)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            AddPanelAnimator(target, order, preset);
        }

        /// <summary>Wires object reference fields in a single serialization pass.</summary>
        protected void WireAll(TView view, params (string Field, UnityEngine.Object Value)[] fields)
        {
            if (fields == null || fields.Length == 0)
            {
                return;
            }

            WireAll(view, so =>
            {
                for (var i = 0; i < fields.Length; i++)
                {
                    SerializationHelper.Wire(so, fields[i].Field, fields[i].Value);
                }
            });
        }

        /// <summary>Wiring pass for views that also expose array fields.</summary>
        protected void WireAll(TView view, Action<SerializedObject> wire)
        {
            if (wire == null)
            {
                throw new ArgumentNullException(nameof(wire));
            }

            var serializedView = new SerializedObject(view);
            wire(serializedView);
            serializedView.ApplyModifiedPropertiesWithoutUndo();
        }

        protected static void WireArray(SerializedObject serializedView, string field, params UnityEngine.Object[] values)
        {
            var property = serializedView.FindProperty(field);

            if (property == null)
            {
                Debug.LogWarning($"[PanelViewBuilder] Field '{field}' not found on {serializedView.targetObject.GetType().Name}");
                return;
            }

            property.arraySize = values.Length;

            for (var i = 0; i < values.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
        }

        /// <summary>
        /// Reuses the background inherited from the base prefab, adding one only when the root
        /// was built from scratch. Views without a background hide it instead of deleting it,
        /// because removing an object of a prefab instance restructures the variant.
        /// </summary>
        private void ApplyBackground()
        {
            var inherited = Root.Find(BackgroundName);

            if (!HasBackground)
            {
                if (inherited != null)
                {
                    inherited.gameObject.SetActive(false);
                }

                return;
            }

            if (inherited != null && inherited.TryGetComponent<Image>(out var image))
            {
                inherited.gameObject.SetActive(true);
                image.color = BackgroundColor;
                return;
            }

            new ImageBuilder(BackgroundName)
                .WithColor(BackgroundColor)
                .NoRaycast()
                .WithStretch()
                .Build(Root);
        }
    }
}
#endif
