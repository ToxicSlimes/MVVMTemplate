#if UNITY_EDITOR
using Core.Abstractions.Popup;
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
    /// Builds sample BindableElement prefabs using base element prefabs from Assets/Prefabs/Elements/.
    /// 
    /// Each sample element is composed from existing base prefabs (Text, Button, ProgressBar, etc.)
    /// rather than creating GameObjects from scratch. This demonstrates the intended workflow:
    /// 
    /// 1. Generate base elements via Base Element Generator
    /// 2. Compose BindableElements from those base prefabs
    /// 3. Wire serialized fields (including BindableElement base: _clickButton, _selectionIndicator, _popupTrigger)
    /// </summary>
    public static class SampleElementBuilders
    {
        private const string OutputDir = "Assets/Prefabs/Elements/Samples";
        private const string TooltipPrefabPath = "Assets/Prefabs/Popups/TooltipPopup.prefab";

        [MenuItem("Tools/UI Prefab Builder/Build Sample Elements")]
        public static void BuildAll()
        {
            BuildTextCard();
            BuildStatBar();
            BuildActionListItem();
            AssetDatabase.Refresh();
            Debug.Log("<color=green>✓ All sample element prefabs created from base prefabs</color>");
        }

        public static void BuildTextCard()
        {
            // Root: Card prefab as base (has background, padding, layout)
            var cardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ElementPaths.Containers.Card);
            GameObject root;

            if (cardPrefab != null)
            {
                root = (GameObject)PrefabUtility.InstantiatePrefab(cardPrefab);
                root.name = "TextCard";
                PrefabUtility.UnpackPrefabInstance(root, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

                // Card has VerticalLayoutGroup — switch to Horizontal for icon+text
                var existingVlg = root.GetComponent<VerticalLayoutGroup>();
                if (existingVlg != null) Object.DestroyImmediate(existingVlg);

                var hlg = root.AddComponent<HorizontalLayoutGroup>();
                hlg.padding = new RectOffset(10, 10, 8, 8);
                hlg.spacing = 10f;
                hlg.childAlignment = TextAnchor.MiddleLeft;
                hlg.childControlWidth = true;
                hlg.childControlHeight = true;
                hlg.childForceExpandWidth = false;
            }
            else
            {
                root = CreateFallbackRoot("TextCard", 300f, 70f);
                var hlg = root.AddComponent<HorizontalLayoutGroup>();
                hlg.padding = new RectOffset(10, 10, 8, 8);
                hlg.spacing = 10f;
                hlg.childAlignment = TextAnchor.MiddleLeft;
                hlg.childControlWidth = true;
                hlg.childControlHeight = true;
                hlg.childForceExpandWidth = false;
                Debug.LogWarning($"[SampleElements] Card prefab not found at {ElementPaths.Containers.Card}. Using fallback.");
            }

            // Ensure sizing
            var rootRt = root.GetComponent<RectTransform>();
            rootRt.sizeDelta = new Vector2(300f, 70f);
            EnsureLayoutElement(root, 70f);

            // Icon — from Image prefab
            var iconGo = SpawnOrCreate(ElementPaths.Atoms.Image, root.transform, "Icon");
            var iconLe = EnsureLayoutElement(iconGo, null);
            iconLe.preferredWidth = 48f;
            iconLe.preferredHeight = 48f;
            var iconImg = iconGo.GetComponent<Image>();

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

            // Title — from Text prefab
            var titleGo = SpawnOrCreate(ElementPaths.Atoms.Text, textContainer.transform, "Title");
            var titleTmp = titleGo.GetComponent<TextMeshProUGUI>();
            titleTmp.text = "Card Title";
            titleTmp.fontSize = 16f;
            titleTmp.fontStyle = FontStyles.Bold;

            // Description — from Text prefab
            var descGo = SpawnOrCreate(ElementPaths.Atoms.Text, textContainer.transform, "Description");
            var descTmp = descGo.GetComponent<TextMeshProUGUI>();
            descTmp.text = "Description text";
            descTmp.fontSize = 12f;
            descTmp.color = new Color(0.7f, 0.7f, 0.7f);

            // Click button on root
            var clickButton = root.GetComponent<Button>() ?? root.AddComponent<Button>();
            clickButton.transition = Selectable.Transition.ColorTint;

            // Selection indicator + popup trigger
            var selectionGo = CreateSelectionIndicator(root.transform);
            var popupTrigger = AddPopupTrigger(root);

            // Wire BindableElement
            var element = root.AddComponent<TextCardElement>();
            var so = new SerializedObject(element);
            SerializationHelper.Wire(so, "_clickButton", clickButton);
            SerializationHelper.Wire(so, "_selectionIndicator", selectionGo);
            SerializationHelper.Wire(so, "_popupTrigger", popupTrigger);
            SerializationHelper.Wire(so, "_titleText", titleTmp);
            SerializationHelper.Wire(so, "_descriptionText", descTmp);
            SerializationHelper.Wire(so, "_iconImage", iconImg);
            so.ApplyModifiedPropertiesWithoutUndo();

            SavePrefab(root, ElementPaths.Samples.TextCard);
        }

        public static void BuildStatBar()
        {
            // Root: from HorizontalLayout prefab
            var hlPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ElementPaths.Containers.HorizontalLayout);
            GameObject root;

            if (hlPrefab != null)
            {
                root = (GameObject)PrefabUtility.InstantiatePrefab(hlPrefab);
                root.name = "StatBar";
                PrefabUtility.UnpackPrefabInstance(root, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

                var hlg = root.GetComponent<HorizontalLayoutGroup>();
                if (hlg != null)
                {
                    hlg.padding = new RectOffset(10, 10, 5, 5);
                    hlg.spacing = 8f;
                    hlg.childAlignment = TextAnchor.MiddleLeft;
                    hlg.childControlWidth = true;
                    hlg.childControlHeight = true;
                }
            }
            else
            {
                root = CreateFallbackRoot("StatBar", 300f, 40f);
                var hlg = root.AddComponent<HorizontalLayoutGroup>();
                hlg.padding = new RectOffset(10, 10, 5, 5);
                hlg.spacing = 8f;
                hlg.childAlignment = TextAnchor.MiddleLeft;
                hlg.childControlWidth = true;
                hlg.childControlHeight = true;
                Debug.LogWarning($"[SampleElements] HorizontalLayout prefab not found. Using fallback.");
            }

            var rootRt = root.GetComponent<RectTransform>();
            rootRt.sizeDelta = new Vector2(300f, 40f);
            EnsureLayoutElement(root, 40f);

            // Background
            var bg = root.GetComponent<Image>() ?? root.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.25f, 0.9f);

            // Label — from Text prefab
            var labelGo = SpawnOrCreate(ElementPaths.Atoms.Text, root.transform, "Label");
            var labelTmp = labelGo.GetComponent<TextMeshProUGUI>();
            labelTmp.text = "Stat";
            labelTmp.fontSize = 14f;
            var labelLe = EnsureLayoutElement(labelGo, null);
            labelLe.preferredWidth = 60f;

            // Bar — from ProgressBar prefab
            var barGo = SpawnOrCreate(ElementPaths.Interactive.ProgressBar, root.transform, "Bar");
            var barLe = EnsureLayoutElement(barGo, null);
            barLe.flexibleWidth = 1f;
            barLe.preferredHeight = 18f;

            // Find fill image inside ProgressBar
            var fillImg = FindFillImage(barGo);
            if (fillImg == null)
            {
                // Fallback: create simple fill manually
                var fill = new GameObject("Fill");
                fill.transform.SetParent(barGo.transform, false);
                var fillRt = fill.AddComponent<RectTransform>();
                fillRt.anchorMin = Vector2.zero;
                fillRt.anchorMax = Vector2.one;
                fillRt.offsetMin = Vector2.zero;
                fillRt.offsetMax = Vector2.zero;
                fillImg = fill.AddComponent<Image>();
                fillImg.color = new Color(0.2f, 0.8f, 0.3f);
                fillImg.type = Image.Type.Filled;
                fillImg.fillMethod = Image.FillMethod.Horizontal;
            }

            // Value text — from Text prefab
            var valueGo = SpawnOrCreate(ElementPaths.Atoms.Text, root.transform, "Value");
            var valueTmp = valueGo.GetComponent<TextMeshProUGUI>();
            valueTmp.text = "0/100";
            valueTmp.fontSize = 13f;
            valueTmp.alignment = TextAlignmentOptions.MidlineRight;
            var valueLe = EnsureLayoutElement(valueGo, null);
            valueLe.preferredWidth = 60f;

            var clickButton = root.AddComponent<Button>();
            clickButton.transition = Selectable.Transition.ColorTint;
            var selectionGo = CreateSelectionIndicator(root.transform);
            var popupTrigger = AddPopupTrigger(root);

            var element = root.AddComponent<StatBarElement>();
            var so = new SerializedObject(element);
            SerializationHelper.Wire(so, "_clickButton", clickButton);
            SerializationHelper.Wire(so, "_selectionIndicator", selectionGo);
            SerializationHelper.Wire(so, "_popupTrigger", popupTrigger);
            SerializationHelper.Wire(so, "_labelText", labelTmp);
            SerializationHelper.Wire(so, "_valueText", valueTmp);
            SerializationHelper.Wire(so, "_fillImage", fillImg);
            so.ApplyModifiedPropertiesWithoutUndo();

            SavePrefab(root, ElementPaths.Samples.StatBar);
        }

        public static void BuildActionListItem()
        {
            // Root: from HorizontalLayout prefab
            var hlPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ElementPaths.Containers.HorizontalLayout);
            GameObject root;

            if (hlPrefab != null)
            {
                root = (GameObject)PrefabUtility.InstantiatePrefab(hlPrefab);
                root.name = "ActionListItem";
                PrefabUtility.UnpackPrefabInstance(root, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

                var hlg = root.GetComponent<HorizontalLayoutGroup>();
                if (hlg != null)
                {
                    hlg.padding = new RectOffset(10, 10, 5, 5);
                    hlg.spacing = 10f;
                    hlg.childAlignment = TextAnchor.MiddleLeft;
                    hlg.childControlWidth = true;
                    hlg.childControlHeight = true;
                }
            }
            else
            {
                root = CreateFallbackRoot("ActionListItem", 300f, 50f);
                var hlg = root.AddComponent<HorizontalLayoutGroup>();
                hlg.padding = new RectOffset(10, 10, 5, 5);
                hlg.spacing = 10f;
                hlg.childAlignment = TextAnchor.MiddleLeft;
                hlg.childControlWidth = true;
                hlg.childControlHeight = true;
                Debug.LogWarning($"[SampleElements] HorizontalLayout prefab not found. Using fallback.");
            }

            var rootRt = root.GetComponent<RectTransform>();
            rootRt.sizeDelta = new Vector2(300f, 50f);
            EnsureLayoutElement(root, 50f);

            var bg = root.GetComponent<Image>() ?? root.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.25f, 0.9f);

            // Root click button (for selection/IClickableElement)
            var clickButton = root.AddComponent<Button>();
            clickButton.transition = Selectable.Transition.ColorTint;

            // Icon — from Image prefab
            var iconGo = SpawnOrCreate(ElementPaths.Atoms.Image, root.transform, "Icon");
            var iconLe = EnsureLayoutElement(iconGo, null);
            iconLe.preferredWidth = 36f;
            iconLe.preferredHeight = 36f;
            var iconImg = iconGo.GetComponent<Image>();

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

            // Title — from Text prefab
            var titleGo = SpawnOrCreate(ElementPaths.Atoms.Text, textContainer.transform, "Title");
            var titleTmp = titleGo.GetComponent<TextMeshProUGUI>();
            titleTmp.text = "Item Title";
            titleTmp.fontSize = 14f;
            titleTmp.fontStyle = FontStyles.Bold;

            // Subtitle — from Text prefab
            var subtitleGo = SpawnOrCreate(ElementPaths.Atoms.Text, textContainer.transform, "Subtitle");
            var subtitleTmp = subtitleGo.GetComponent<TextMeshProUGUI>();
            subtitleTmp.text = "Subtitle";
            subtitleTmp.fontSize = 11f;
            subtitleTmp.color = new Color(0.6f, 0.6f, 0.6f);

            // Action button — from Button prefab
            var actionBtnGo = SpawnOrCreate(ElementPaths.Interactive.Button, root.transform, "ActionButton");
            var actionBtn = actionBtnGo.GetComponent<Button>();
            var actionBtnLe = EnsureLayoutElement(actionBtnGo, null);
            actionBtnLe.preferredWidth = 80f;
            var btnLabel = actionBtnGo.GetComponentInChildren<TextMeshProUGUI>();
            if (btnLabel != null)
            {
                btnLabel.text = "Action";
                btnLabel.fontSize = 12f;
            }

            var selectionGo = CreateSelectionIndicator(root.transform);
            var popupTrigger = AddPopupTrigger(root);

            var element = root.AddComponent<ActionListItemElement>();
            var so = new SerializedObject(element);
            SerializationHelper.Wire(so, "_clickButton", clickButton);
            SerializationHelper.Wire(so, "_selectionIndicator", selectionGo);
            SerializationHelper.Wire(so, "_popupTrigger", popupTrigger);
            SerializationHelper.Wire(so, "_titleText", titleTmp);
            SerializationHelper.Wire(so, "_subtitleText", subtitleTmp);
            SerializationHelper.Wire(so, "_iconImage", iconImg);
            SerializationHelper.Wire(so, "_actionButton", actionBtn);
            SerializationHelper.Wire(so, "_actionLabel", btnLabel);
            so.ApplyModifiedPropertiesWithoutUndo();

            SavePrefab(root, ElementPaths.Samples.ActionListItem);
        }

        // === Helpers ===

        /// <summary>
        /// Spawns from prefab if it exists, otherwise creates a basic fallback GameObject.
        /// </summary>
        private static GameObject SpawnOrCreate(string prefabPath, Transform parent, string name)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            GameObject go;

            if (prefab != null)
            {
                go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                go.transform.SetParent(parent, false);
                go.name = name;
                PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            }
            else
            {
                go = new GameObject(name);
                go.transform.SetParent(parent, false);
                go.AddComponent<RectTransform>();
                Debug.LogWarning($"[SampleElements] Prefab not found: {prefabPath}. Created empty GO '{name}'.");
            }

            return go;
        }

        private static GameObject CreateFallbackRoot(string name, float width, float height)
        {
            var go = new GameObject(name);
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(width, height);
            rt.localScale = Vector3.one;
            go.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.25f, 0.9f);
            return go;
        }

        private static LayoutElement EnsureLayoutElement(GameObject go, float? preferredHeight)
        {
            var le = go.GetComponent<LayoutElement>();
            if (le == null) le = go.AddComponent<LayoutElement>();
            if (preferredHeight.HasValue) le.preferredHeight = preferredHeight.Value;
            return le;
        }

        private static Image FindFillImage(GameObject progressBar)
        {
            // ProgressBar prefab structure: root > Background > Fill
            var fill = progressBar.transform.Find("Fill");
            if (fill != null) return fill.GetComponent<Image>();

            // Try deeper
            foreach (Transform child in progressBar.transform)
            {
                var f = child.Find("Fill");
                if (f != null) return f.GetComponent<Image>();
            }

            return null;
        }

        private static GameObject CreateSelectionIndicator(Transform parent)
        {
            var go = new GameObject("SelectionIndicator");
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(-2f, -2f);
            rt.offsetMax = new Vector2(2f, 2f);

            var img = go.AddComponent<Image>();
            img.color = new Color(0.4f, 0.7f, 1f, 0.8f);
            img.raycastTarget = false;

            go.SetActive(false);
            return go;
        }

        private static PopupTrigger AddPopupTrigger(GameObject root)
        {
            var trigger = root.AddComponent<PopupTrigger>();
            var so = new SerializedObject(trigger);

            var tooltipPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TooltipPrefabPath);
            if (tooltipPrefab != null)
            {
                var prefabProp = so.FindProperty("_popupPrefab");
                if (prefabProp != null)
                    prefabProp.objectReferenceValue = tooltipPrefab;
            }

            var triggerProp = so.FindProperty("_triggerType");
            if (triggerProp != null)
                triggerProp.enumValueIndex = (int)PopupTriggerType.Hover;

            so.ApplyModifiedPropertiesWithoutUndo();
            return trigger;
        }

        private static void SavePrefab(GameObject root, string path)
        {
            var dir = System.IO.Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);

            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            Debug.Log($"<color=cyan>Element prefab (from base prefabs):</color> {path}");
        }
    }
}
#endif
