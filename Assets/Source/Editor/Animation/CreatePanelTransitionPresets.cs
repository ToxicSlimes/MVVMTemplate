using Core.Abstractions.MVVM.Views;
using Core.Infrastructure.MVVM.Animation;
using LitMotion;
using UnityEditor;
using UnityEngine;

namespace Core.Editor.Animation
{
    public static class CreatePanelTransitionPresets
    {
        private const string FolderPath = "Assets/Source/00. Configs/Animation";

        [MenuItem("Tools/MVVM/Create Panel Transition Presets")]
        public static void CreateAll()
        {
            EnsureFolder();

            CreatePreset("Preset_Fade", new PresetData
            {

                ShowTransition = ViewTransitionStyle.Fade,
                ShowDuration = 0.25f,
                ShowEase = Ease.OutQuad,
                HideTransition = ViewTransitionStyle.Fade,
                HideDuration = 0.2f,
                HideEase = Ease.InQuad
            });

            CreatePreset("Preset_FadeScale", new PresetData
            {

                ShowTransition = ViewTransitionStyle.Fade | ViewTransitionStyle.Scale,
                ShowDuration = 0.3f,
                ShowEase = Ease.OutQuad,
                HideTransition = ViewTransitionStyle.Fade | ViewTransitionStyle.Scale,
                HideDuration = 0.2f,
                HideEase = Ease.InQuad
            });

            CreatePreset("Preset_FadeScaleBounce", new PresetData
            {

                ShowTransition = ViewTransitionStyle.Fade | ViewTransitionStyle.Scale | ViewTransitionStyle.Bounce,
                ShowDuration = 0.35f,
                ShowEase = Ease.OutBack,
                HideTransition = ViewTransitionStyle.Fade | ViewTransitionStyle.Scale,
                HideDuration = 0.2f,
                HideEase = Ease.InBack
            });

            CreatePreset("Preset_SlideUp", new PresetData
            {

                ShowTransition = ViewTransitionStyle.Fade | ViewTransitionStyle.SlideUp,
                ShowDuration = 0.3f,
                ShowEase = Ease.OutCubic,
                HideTransition = ViewTransitionStyle.Fade | ViewTransitionStyle.SlideDown,
                HideDuration = 0.25f,
                HideEase = Ease.InCubic
            });

            CreatePreset("Preset_SlideDown", new PresetData
            {

                ShowTransition = ViewTransitionStyle.Fade | ViewTransitionStyle.SlideDown,
                ShowDuration = 0.3f,
                ShowEase = Ease.OutCubic,
                HideTransition = ViewTransitionStyle.Fade | ViewTransitionStyle.SlideUp,
                HideDuration = 0.25f,
                HideEase = Ease.InCubic
            });

            CreatePreset("Preset_SlideLeft", new PresetData
            {

                ShowTransition = ViewTransitionStyle.Fade | ViewTransitionStyle.SlideLeft,
                ShowDuration = 0.3f,
                ShowEase = Ease.OutCubic,
                HideTransition = ViewTransitionStyle.Fade | ViewTransitionStyle.SlideRight,
                HideDuration = 0.25f,
                HideEase = Ease.InCubic
            });

            CreatePreset("Preset_SlideRight", new PresetData
            {

                ShowTransition = ViewTransitionStyle.Fade | ViewTransitionStyle.SlideRight,
                ShowDuration = 0.3f,
                ShowEase = Ease.OutCubic,
                HideTransition = ViewTransitionStyle.Fade | ViewTransitionStyle.SlideLeft,
                HideDuration = 0.25f,
                HideEase = Ease.InCubic
            });

            CreatePreset("Preset_PopupModal", new PresetData
            {

                ShowTransition = ViewTransitionStyle.Fade | ViewTransitionStyle.Scale | ViewTransitionStyle.Bounce,
                ShowDuration = 0.4f,
                ShowEase = Ease.OutBack,
                HideTransition = ViewTransitionStyle.Fade | ViewTransitionStyle.Scale,
                HideDuration = 0.15f,
                HideEase = Ease.InQuad
            });

            CreatePreset("Preset_ToastTop", new PresetData
            {

                ShowTransition = ViewTransitionStyle.Fade | ViewTransitionStyle.SlideDown,
                ShowDuration = 0.25f,
                ShowEase = Ease.OutQuart,
                HideTransition = ViewTransitionStyle.Fade | ViewTransitionStyle.SlideUp,
                HideDuration = 0.2f,
                HideEase = Ease.InQuart,
                SlideOffset = 0.5f
            });

            CreatePreset("Preset_ToastBottom", new PresetData
            {

                ShowTransition = ViewTransitionStyle.Fade | ViewTransitionStyle.SlideUp,
                ShowDuration = 0.25f,
                ShowEase = Ease.OutQuart,
                HideTransition = ViewTransitionStyle.Fade | ViewTransitionStyle.SlideDown,
                HideDuration = 0.2f,
                HideEase = Ease.InQuart,
                SlideOffset = 0.5f
            });

            CreatePreset("Preset_Instant", new PresetData
            {

                ShowTransition = ViewTransitionStyle.None,
                HideTransition = ViewTransitionStyle.None
            });

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Panel Transition Presets",
                $"Presets created/updated in:\n{FolderPath}\n\n" +
                "- Preset_Fade\n" +
                "- Preset_FadeScale\n" +
                "- Preset_FadeScaleBounce\n" +
                "- Preset_SlideUp / Down / Left / Right\n" +
                "- Preset_PopupModal\n" +
                "- Preset_ToastTop / ToastBottom\n" +
                "- Preset_Instant",
                "OK"
            );

            var folder = AssetDatabase.LoadAssetAtPath<Object>(FolderPath);
            Selection.activeObject = folder;
            EditorGUIUtility.PingObject(folder);
        }

        private static void CreatePreset(string name, PresetData data)
        {
            var assetPath = $"{FolderPath}/{name}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<PanelTransitionPreset>(assetPath);
            var preset = existing != null ? existing : ScriptableObject.CreateInstance<PanelTransitionPreset>();

            var so = new SerializedObject(preset);
            so.FindProperty("_showTransition").intValue = (int)data.ShowTransition;
            so.FindProperty("_showDuration").floatValue = data.ShowDuration;
            so.FindProperty("_showDelay").floatValue = data.ShowDelay;
            so.FindProperty("_showEase").intValue = (int)data.ShowEase;
            so.FindProperty("_hideTransition").intValue = (int)data.HideTransition;
            so.FindProperty("_hideDuration").floatValue = data.HideDuration;
            so.FindProperty("_hideDelay").floatValue = data.HideDelay;
            so.FindProperty("_hideEase").intValue = (int)data.HideEase;
            so.FindProperty("_slideOffset").floatValue = data.SlideOffset;
            so.ApplyModifiedPropertiesWithoutUndo();

            if (existing == null)
                AssetDatabase.CreateAsset(preset, assetPath);
            else
                EditorUtility.SetDirty(preset);

            Debug.Log($"[PanelTransitionPresets] {(existing != null ? "Updated" : "Created")} {assetPath}");
        }

        private static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Source/00. Configs/Animation"))
                AssetDatabase.CreateFolder("Assets/Source/00. Configs", "Animation");
        }

        private static PresetData DefaultPresetData() => new PresetData
        {
            ShowTransition = ViewTransitionStyle.Fade,
            ShowDuration = 0.25f,
            ShowEase = Ease.OutQuad,
            HideTransition = ViewTransitionStyle.Fade,
            HideDuration = 0.2f,
            HideEase = Ease.InQuad,
            SlideOffset = 1f
        };

        private struct PresetData
        {
            public ViewTransitionStyle ShowTransition;
            public float ShowDuration;
            public float ShowDelay;
            public Ease ShowEase;
            public ViewTransitionStyle HideTransition;
            public float HideDuration;
            public float HideDelay;
            public Ease HideEase;
            public float SlideOffset;
        }
    }
}
