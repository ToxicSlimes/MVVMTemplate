#if UNITY_EDITOR
using UnityEngine;

namespace Core.Editor.PrefabBuilderSystem.Generator
{
    [CreateAssetMenu(fileName = "BaseElementSettings", menuName = "PrefabBuilder/Base Element Settings")]
    public class BaseElementSettings : ScriptableObject
    {
        [Header("=== ATOMS ===")]
        public TextSettings text = new();
        public ImageSettings image = new();
        public IconSettings icon = new();
        public SpacerSettings spacer = new();
        public DividerSettings divider = new();

        [Header("=== INTERACTIVE ===")]
        public ButtonSettings button = new();
        public ToggleSettings toggle = new();
        public SliderSettings slider = new();
        public InputFieldSettings inputField = new();
        public DropdownSettings dropdown = new();
        public ScrollbarSettings scrollbar = new();
        public ProgressBarSettings progressBar = new();
        public TabButtonSettings tabButton = new();
        public RadioButtonSettings radioButton = new();
        public StepperSettings stepper = new();

        [Header("=== CONTAINERS ===")]
        public LayoutSettings horizontalLayout = new();
        public LayoutSettings verticalLayout = new();
        public GridSettings grid = new();
        public ScrollViewSettings scrollView = new();
        public CardSettings card = new();
        public PanelSettings panel = new();
        public ModalSettings modal = new();
        public TabBarSettings tabBar = new();
        public AccordionSettings accordion = new();

        [Header("=== GAME UI ===")]
        public HealthBarSettings healthBar = new();
        public BadgeSettings badge = new();
        public AvatarSettings avatar = new();

        // === ATOMS ===

        [System.Serializable]
        public class TextSettings
        {
            public float preferredHeight = 24f;
            public float fontSize = 14f;
        }

        [System.Serializable]
        public class ImageSettings
        {
            public float width = 100f;
            public float height = 100f;
        }

        [System.Serializable]
        public class IconSettings
        {
            public float size = 24f;
        }

        [System.Serializable]
        public class SpacerSettings
        {
            public float height = 16f;
        }

        [System.Serializable]
        public class DividerSettings
        {
            public float thickness = 1f;
        }

        // === INTERACTIVE ===

        [System.Serializable]
        public class ButtonSettings
        {
            public float width = 120f;
            public float height = 40f;
            public float fontSize = 16f;
            public string defaultLabel = "Button";
        }

        [System.Serializable]
        public class ToggleSettings
        {
            public float size = 24f;
            public float checkmarkPadding = 4f;
        }

        [System.Serializable]
        public class SliderSettings
        {
            public float width = 200f;
            public float height = 30f;
            public float handleSize = 20f;
            [Tooltip("Offset для Handle Slide Area")]
            public float handleAreaOffset = 5f;
            [Tooltip("Offset для Background и Fill Area")]
            public float trackOffset = 5f;
            public float defaultValue = 0.5f;
        }

        [System.Serializable]
        public class InputFieldSettings
        {
            public float width = 200f;
            public float height = 40f;
            public float fontSize = 14f;
            public float padding = 12f;
            public string placeholder = "Enter text...";
            [Tooltip("Вертикальный offset для Text Area")]
            public float textAreaVerticalOffset = 8f;
        }

        [System.Serializable]
        public class DropdownSettings
        {
            public float width = 200f;
            public float height = 40f;
            public float fontSize = 14f;
            public float itemHeight = 32f;
            public float templateHeight = 150f;
        }

        [System.Serializable]
        public class ScrollbarSettings
        {
            public float width = 12f;
            public float handlePadding = 2f;
        }

        [System.Serializable]
        public class ProgressBarSettings
        {
            public float height = 20f;
            public float defaultValue = 0.7f;
        }

        [System.Serializable]
        public class TabButtonSettings
        {
            public float minWidth = 80f;
            public float height = 40f;
            public float fontSize = 14f;
            public float indicatorHeight = 3f;
        }

        [System.Serializable]
        public class RadioButtonSettings
        {
            public float radioSize = 20f;
            public float innerRatio = 0.6f;
            public float spacing = 10f;
        }

        [System.Serializable]
        public class StepperSettings
        {
            public float buttonSize = 32f;
            public float valueWidth = 60f;
            public float fontSize = 16f;
        }

        // === CONTAINERS ===

        [System.Serializable]
        public class LayoutSettings
        {
            public float spacing = 8f;
            public int paddingLeft = 0;
            public int paddingRight = 0;
            public int paddingTop = 0;
            public int paddingBottom = 0;
        }

        [System.Serializable]
        public class GridSettings
        {
            public float cellWidth = 100f;
            public float cellHeight = 100f;
            public float spacingX = 8f;
            public float spacingY = 8f;
        }

        [System.Serializable]
        public class ScrollViewSettings
        {
            public float width = 300f;
            public float height = 400f;
            public float scrollSensitivity = 20f;
        }

        [System.Serializable]
        public class CardSettings
        {
            public int padding = 16;
            public float borderWidth = 1f;
        }

        [System.Serializable]
        public class PanelSettings
        {
            public float headerHeight = 40f;
            public int contentPadding = 16;
            public float contentSpacing = 8f;
        }

        [System.Serializable]
        public class ModalSettings
        {
            public float width = 500f;
            public float height = 400f;
            public float headerHeight = 50f;
            public float footerHeight = 60f;
            public float closeButtonSize = 36f;
        }

        [System.Serializable]
        public class TabBarSettings
        {
            public float height = 44f;
            public float spacing = 4f;
            public int padding = 8;
        }

        [System.Serializable]
        public class AccordionSettings
        {
            public float headerHeight = 44f;
            public int contentPadding = 16;
            public float contentSpacing = 8f;
        }

        // === GAME UI ===

        [System.Serializable]
        public class HealthBarSettings
        {
            public float height = 24f;
            public float fontSize = 14f;
            public float defaultValue = 1f;
            public Color fillColor = new(0.9f, 0.22f, 0.21f, 1f);
            public Color backgroundColor = new(0.4f, 0.4f, 0.4f, 1f);
            public Color textColor = Color.white;
        }

        [System.Serializable]
        public class BadgeSettings
        {
            public float size = 36f;
            public float fontSize = 12f;
            public Color backgroundColor = Color.white;
            public Color textColor = new(0.2f, 0.2f, 0.2f, 1f);
            public Color outlineColor = new(0.1f, 0.1f, 0.1f, 1f);
        }

        [System.Serializable]
        public class AvatarSettings
        {
            public float size = 64f;
            public float borderWidth = 2f;
            public Color backgroundColor = new(0.3f, 0.3f, 0.3f, 1f);
            public Color borderColor = new(0.1f, 0.1f, 0.1f, 1f);
        }
    }
}
#endif
