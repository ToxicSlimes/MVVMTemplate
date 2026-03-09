#if UNITY_EDITOR
using global::Core.Infrastructure.UI;
using global::Core.Infrastructure.UI.Effects;
using global::Core.Infrastructure.UI.States;
using UnityEditor;
using UnityEngine;

namespace Core.Editor.PrefabBuilderSystem.Theming
{
    [CreateAssetMenu(fileName = "ControlDefaults", menuName = "MVVM/UI/Control Defaults")]
    public class ControlDefaults : ScriptableObject
    {
        [Header("Visual")]
        public ControlVisualConfig visualConfig;

        [Header("Effects")]
        public UIEffect hoverEffect;
        public UIEffect pressEffect;

        private static ControlDefaults _instance;

        public static ControlDefaults Instance
        {
            get
            {
                if (_instance != null) return _instance;

                var guids = AssetDatabase.FindAssets("t:ControlDefaults");
                if (guids.Length > 0)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    _instance = AssetDatabase.LoadAssetAtPath<ControlDefaults>(path);
                }

                return _instance;
            }
        }

        public static void ApplyTo(BaseControlBehaviour bcb)
        {
            var defaults = Instance;
            if (defaults == null) return;

            var so = new SerializedObject(bcb);
            if (defaults.visualConfig != null)
            {
                so.FindProperty("_visualConfig").objectReferenceValue = defaults.visualConfig;
            }
            if (defaults.hoverEffect != null)
            {
                so.FindProperty("_hoverEffect").objectReferenceValue = defaults.hoverEffect;
            }
            if (defaults.pressEffect != null)
            {
                so.FindProperty("_pressEffect").objectReferenceValue = defaults.pressEffect;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        [MenuItem("Tools/MVVM/Create Default Control Assets")]
        private static void CreateDefaultAssets()
        {
            var basePath = "Assets/Source/00. Configs/Controls";
            if (!AssetDatabase.IsValidFolder(basePath))
            {
                System.IO.Directory.CreateDirectory(basePath);
                AssetDatabase.Refresh();
            }

            var vcPath = $"{basePath}/DefaultVisualConfig.asset";
            var vcAsset = AssetDatabase.LoadAssetAtPath<ControlVisualConfig>(vcPath);
            if (vcAsset == null)
            {
                vcAsset = CreateInstance<ControlVisualConfig>();
                AssetDatabase.CreateAsset(vcAsset, vcPath);
            }

            var hoverPath = $"{basePath}/HoverEffect.asset";
            var hoverAsset = AssetDatabase.LoadAssetAtPath<UIEffect>(hoverPath);
            if (hoverAsset == null)
            {
                hoverAsset = CreateInstance<UIEffect>();
                hoverAsset.Type = UIEffectType.Scale;
                hoverAsset.Duration = 0.15f;
                hoverAsset.Strength = 0.5f;
                AssetDatabase.CreateAsset(hoverAsset, hoverPath);
            }

            var pressPath = $"{basePath}/PressEffect.asset";
            var pressAsset = AssetDatabase.LoadAssetAtPath<UIEffect>(pressPath);
            if (pressAsset == null)
            {
                pressAsset = CreateInstance<UIEffect>();
                pressAsset.Type = UIEffectType.Scale;
                pressAsset.Duration = 0.1f;
                pressAsset.Strength = 1f;
                AssetDatabase.CreateAsset(pressAsset, pressPath);
            }

            var defaultsPath = $"{basePath}/ControlDefaults.asset";
            var defaultsAsset = AssetDatabase.LoadAssetAtPath<ControlDefaults>(defaultsPath);
            if (defaultsAsset == null)
            {
                defaultsAsset = CreateInstance<ControlDefaults>();
                AssetDatabase.CreateAsset(defaultsAsset, defaultsPath);
            }

            var dso = new SerializedObject(defaultsAsset);
            dso.FindProperty("visualConfig").objectReferenceValue = vcAsset;
            dso.FindProperty("hoverEffect").objectReferenceValue = hoverAsset;
            dso.FindProperty("pressEffect").objectReferenceValue = pressAsset;
            dso.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[ControlDefaults] Created default control assets at {basePath}");
        }
    }
}
#endif
