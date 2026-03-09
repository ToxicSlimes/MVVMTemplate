#if UNITY_EDITOR
namespace Core.Editor.PrefabBuilderSystem.Elements
{
    public static class ElementPaths
    {
        private const string Base = "Assets/Prefabs/Elements";

        public static class Atoms
        {
            private const string Dir = Base + "/Atoms";
            public const string Text = Dir + "/Text.prefab";
            public const string Image = Dir + "/Image.prefab";
            public const string RawImage = Dir + "/RawImage.prefab";
            public const string Icon = Dir + "/Icon.prefab";
            public const string Spacer = Dir + "/Spacer.prefab";
            public const string Divider = Dir + "/Divider.prefab";
        }

        public static class Interactive
        {
            private const string Dir = Base + "/Interactive";
            public const string Button = Dir + "/Button.prefab";
            public const string Toggle = Dir + "/Toggle.prefab";
            public const string Slider = Dir + "/Slider.prefab";
            public const string InputField = Dir + "/InputField.prefab";
            public const string Dropdown = Dir + "/Dropdown.prefab";
            public const string Scrollbar = Dir + "/Scrollbar.prefab";
            public const string ProgressBar = Dir + "/ProgressBar.prefab";
            public const string TabButton = Dir + "/TabButton.prefab";
            public const string RadioButton = Dir + "/RadioButton.prefab";
            public const string Stepper = Dir + "/Stepper.prefab";
        }

        public static class Containers
        {
            private const string Dir = Base + "/Containers";
            public const string HorizontalLayout = Dir + "/HorizontalLayout.prefab";
            public const string VerticalLayout = Dir + "/VerticalLayout.prefab";
            public const string GridLayout = Dir + "/GridLayout.prefab";
            public const string ScrollView = Dir + "/ScrollView.prefab";
            public const string Card = Dir + "/Card.prefab";
            public const string Panel = Dir + "/Panel.prefab";
            public const string Modal = Dir + "/Modal.prefab";
            public const string TabBar = Dir + "/TabBar.prefab";
            public const string Accordion = Dir + "/Accordion.prefab";
        }

        public static class Composites
        {
            private const string Dir = Base + "/Composites";
            public const string SliderRow = Dir + "/SliderRow.prefab";
            public const string ToggleRow = Dir + "/ToggleRow.prefab";
            public const string DropdownRow = Dir + "/DropdownRow.prefab";
            public const string InputRow = Dir + "/InputRow.prefab";
            public const string ProgressRow = Dir + "/ProgressRow.prefab";
            public const string ButtonGroup = Dir + "/ButtonGroup.prefab";
            public const string TabGroup = Dir + "/TabGroup.prefab";
            
            public static class Settings
            {
                private const string SettingsDir = Base + "/Settings";
                public const string SliderRow = SettingsDir + "/SliderRow.prefab";
                public const string ToggleRow = SettingsDir + "/ToggleRow.prefab";
                public const string DropdownRow = SettingsDir + "/DropdownRow.prefab";
                public const string Divider = SettingsDir + "/Divider.prefab";
                public const string TabButton = SettingsDir + "/TabButton.prefab";
            }
        }

        public static class Chat
        {
            private const string Dir = Base + "/Chat";
            public const string ChatBubble = Dir + "/ChatBubble.prefab";
        }

        public static class MainView
        {
            private const string Dir = Base + "/MainView";
            public const string HeroSlot = Dir + "/HeroSlot.prefab";
            public const string HeroSlotDetailed = Dir + "/HeroSlotDetailed.prefab";
            public const string HeroesGridPanel = Dir + "/HeroesGridPanel.prefab";
            public const string RelationshipSlot = Dir + "/RelationshipSlot.prefab";
            public const string AbilityCard = Dir + "/AbilityCard.prefab";
            public const string AttributeItem = Dir + "/AttributeItem.prefab";
            public const string HeroDetailPanel = Dir + "/HeroDetailPanel.prefab";
            public const string StatRow = Dir + "/StatRow.prefab";
            public const string ResourceItem = Dir + "/ResourceItem.prefab";
            public const string MenuButton = Dir + "/MenuButton.prefab";
            public const string EquipmentSlot = Dir + "/EquipmentSlot.prefab";
            public const string ServiceSlot = Dir + "/ServiceSlot.prefab";
            public const string HealingPanel = Dir + "/HealingPanel.prefab";
            public const string TrainingPanel = Dir + "/TrainingPanel.prefab";
        }

        public static class Samples
        {
            private const string Dir = Base + "/Samples";
            public const string TextCard = Dir + "/TextCard.prefab";
            public const string StatBar = Dir + "/StatBar.prefab";
            public const string ActionListItem = Dir + "/ActionListItem.prefab";
            public const string InventoryItem = Dir + "/InventoryItem.prefab";
        }

        public static class GameUI
        {
            private const string Dir = Base + "/GameUI";
            public const string HealthBar = Dir + "/HealthBar.prefab";
            public const string ResourceCounter = Dir + "/ResourceCounter.prefab";
            public const string Avatar = Dir + "/Avatar.prefab";
            public const string ItemSlot = Dir + "/ItemSlot.prefab";
            public const string StatDisplay = Dir + "/StatDisplay.prefab";
            public const string CooldownOverlay = Dir + "/CooldownOverlay.prefab";
            public const string Badge = Dir + "/Badge.prefab";
            public const string Tooltip = Dir + "/Tooltip.prefab";
            public const string RarityFrame = Dir + "/RarityFrame.prefab";
            public const string StarRating = Dir + "/StarRating.prefab";
        }
    }
}
#endif
