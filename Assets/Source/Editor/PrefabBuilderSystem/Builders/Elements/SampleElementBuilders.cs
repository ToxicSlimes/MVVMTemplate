#if UNITY_EDITOR
using Core.Editor.PrefabBuilderSystem.Core;
using Core.Editor.PrefabBuilderSystem.Elements;
using Core.MVVM.Elements.Samples;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Core.Editor.PrefabBuilderSystem.Builders.Elements
{
    /// <summary>
    /// Builds sample BindableElement prefabs for the template.
    /// Menu: Tools → UI Prefab Builder → Build Sample Elements
    /// </summary>
    public static class SampleElementBuilders
    {
        private const string OutputDir = "Assets/Prefabs/Elements/Samples";

        [MenuItem("Tools/UI Prefab Builder/Build Sample Elements")]
        public static void BuildAll()
        {
            BuildTextCard();
            BuildStatBar();
            BuildActionListItem();
            Debug.Log("<color=green>✓ All sample element prefabs created</color>");
        }

        private static void BuildTextCard()
        {
            var root = CreateElementRoot("TextCard", 300f, 70f);

            var hlg = root.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(10, 10, 8, 8);
            hlg.spacing = 10f;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;

            // Icon placeholder
            var iconGo = new GameObject("Icon");
            iconGo.transform.SetParent(root.transform, false);
            iconGo.AddComponent<RectTransform>();
            var iconLe = iconGo.AddComponent<LayoutElement>();
            iconLe.preferredWidth = 48f;
            iconLe.preferredHeight = 48f;
            var iconImg = iconGo.AddComponent<Image>();
            iconImg.color = new Color(0.3f, 0.3f, 0.4f);

            // Text container
            var textContainer = new GameObject("TextContainer");
            textContainer.transform.SetParent(root.transform, false);
            textContainer.AddComponent<RectTransform>();
            var vlg = textContainer.AddComponent<VerticalLayoutGroup>();
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;
            var tcLe = textContainer.AddComponent<LayoutElement>();
            tcLe.flexibleWidth = 1f;

            var titleTmp = CreateText(textContainer.transform, "Title", "Card Title", 16f, FontStyles.Bold);
            var descTmp = CreateText(textContainer.transform, "Description", "Description text", 12f, FontStyles.Normal, new Color(0.7f, 0.7f, 0.7f));

            // Add component + wire
            var element = root.AddComponent<TextCardElement>();
            var so = new SerializedObject(element);
            SerializationHelper.Wire(so, "_titleText", titleTmp);
            SerializationHelper.Wire(so, "_descriptionText", descTmp);
            SerializationHelper.Wire(so, "_iconImage", iconImg);
            so.ApplyModifiedPropertiesWithoutUndo();

            SavePrefab(root, $"{OutputDir}/TextCard.prefab");
        }

        private static void BuildStatBar()
        {
            var root = CreateElementRoot("StatBar", 300f, 40f);

            var hlg = root.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(10, 10, 5, 5);
            hlg.spacing = 8f;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            var labelTmp = CreateText(root.transform, "Label", "Stat", 14f, FontStyles.Normal);
            var labelLe = labelTmp.gameObject.AddComponent<LayoutElement>();
            labelLe.preferredWidth = 60f;

            // Bar background
            var barBg = new GameObject("BarBackground");
            barBg.transform.SetParent(root.transform, false);
            barBg.AddComponent<RectTransform>();
            barBg.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.2f);
            var barLe = barBg.AddComponent<LayoutElement>();
            barLe.flexibleWidth = 1f;
            barLe.preferredHeight = 18f;

            var fill = new GameObject("Fill");
            fill.transform.SetParent(barBg.transform, false);
            var fillRt = fill.AddComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = Vector2.zero;
            fillRt.offsetMax = Vector2.zero;
            var fillImg = fill.AddComponent<Image>();
            fillImg.color = new Color(0.2f, 0.8f, 0.3f);
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;

            var valueTmp = CreateText(root.transform, "Value", "0/100", 13f, FontStyles.Normal);
            valueTmp.alignment = TextAlignmentOptions.MidlineRight;
            var valueLe = valueTmp.gameObject.AddComponent<LayoutElement>();
            valueLe.preferredWidth = 60f;

            var element = root.AddComponent<StatBarElement>();
            var so = new SerializedObject(element);
            SerializationHelper.Wire(so, "_labelText", labelTmp);
            SerializationHelper.Wire(so, "_valueText", valueTmp);
            SerializationHelper.Wire(so, "_fillImage", fillImg);
            so.ApplyModifiedPropertiesWithoutUndo();

            SavePrefab(root, $"{OutputDir}/StatBar.prefab");
        }

        private static void BuildActionListItem()
        {
            var root = CreateElementRoot("ActionListItem", 300f, 50f);

            var hlg = root.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(10, 10, 5, 5);
            hlg.spacing = 10f;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            // Icon
            var iconGo = new GameObject("Icon");
            iconGo.transform.SetParent(root.transform, false);
            iconGo.AddComponent<RectTransform>();
            var iconLe = iconGo.AddComponent<LayoutElement>();
            iconLe.preferredWidth = 36f;
            iconLe.preferredHeight = 36f;
            var iconImg = iconGo.AddComponent<Image>();
            iconImg.color = new Color(0.3f, 0.3f, 0.4f);

            // Text container
            var textContainer = new GameObject("TextContainer");
            textContainer.transform.SetParent(root.transform, false);
            textContainer.AddComponent<RectTransform>();
            var vlg = textContainer.AddComponent<VerticalLayoutGroup>();
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;
            var tcLe = textContainer.AddComponent<LayoutElement>();
            tcLe.flexibleWidth = 1f;

            var titleTmp = CreateText(textContainer.transform, "Title", "Item Title", 14f, FontStyles.Bold);
            var subtitleTmp = CreateText(textContainer.transform, "Subtitle", "Subtitle", 11f, FontStyles.Normal, new Color(0.6f, 0.6f, 0.6f));

            // Action button
            var btnGo = new GameObject("ActionButton");
            btnGo.transform.SetParent(root.transform, false);
            btnGo.AddComponent<RectTransform>();
            var btnLe = btnGo.AddComponent<LayoutElement>();
            btnLe.preferredWidth = 80f;
            btnGo.AddComponent<Image>().color = new Color(0.3f, 0.5f, 0.9f);
            var btn = btnGo.AddComponent<Button>();

            var btnLabelGo = new GameObject("Label");
            btnLabelGo.transform.SetParent(btnGo.transform, false);
            var btnLabelRt = btnLabelGo.AddComponent<RectTransform>();
            btnLabelRt.anchorMin = Vector2.zero;
            btnLabelRt.anchorMax = Vector2.one;
            btnLabelRt.offsetMin = Vector2.zero;
            btnLabelRt.offsetMax = Vector2.zero;
            var btnLabel = btnLabelGo.AddComponent<TextMeshProUGUI>();
            btnLabel.text = "Action";
            btnLabel.fontSize = 12f;
            btnLabel.alignment = TextAlignmentOptions.Center;

            var element = root.AddComponent<ActionListItemElement>();
            var so = new SerializedObject(element);
            SerializationHelper.Wire(so, "_titleText", titleTmp);
            SerializationHelper.Wire(so, "_subtitleText", subtitleTmp);
            SerializationHelper.Wire(so, "_iconImage", iconImg);
            SerializationHelper.Wire(so, "_actionButton", btn);
            SerializationHelper.Wire(so, "_actionLabel", btnLabel);
            so.ApplyModifiedPropertiesWithoutUndo();

            SavePrefab(root, $"{OutputDir}/ActionListItem.prefab");
        }

        private static GameObject CreateElementRoot(string name, float width, float height)
        {
            var go = new GameObject(name);
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(width, height);
            rt.localScale = Vector3.one;

            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = height;

            var bg = go.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.25f, 0.9f);

            return go;
        }

        private static TextMeshProUGUI CreateText(Transform parent, string name, string text, float fontSize, FontStyles style, Color? color = null)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            if (color.HasValue) tmp.color = color.Value;
            return tmp;
        }

        private static void SavePrefab(GameObject root, string path)
        {
            var dir = System.IO.Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);

            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            Debug.Log($"<color=cyan>Element prefab:</color> {path}");
        }
    }
}
#endif

