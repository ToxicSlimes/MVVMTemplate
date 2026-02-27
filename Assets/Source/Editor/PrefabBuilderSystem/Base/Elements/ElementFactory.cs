#if UNITY_EDITOR
using Core.Editor.PrefabBuilderSystem.Elements.Composites;
using Core.Editor.PrefabBuilderSystem.Elements.GameUI;

namespace Core.Editor.PrefabBuilderSystem.Elements
{
    public static class UI
    {
        // === ATOMS ===
        public static TextBuilder Text(string name) => new(name);
        public static ImageBuilder Image(string name) => new(name);
        public static RawImageBuilder RawImage(string name) => new(name);
        public static IconBuilder Icon(string name) => new(name);
        public static SpacerBuilder Spacer(string name = "Spacer") => new(name);
        public static DividerBuilder Divider(string name = "Divider") => new(name);

        // === INTERACTIVE ===
        public static ButtonBuilder Button(string name) => new(name);
        public static ToggleBuilder Toggle(string name) => new(name);
        public static SliderBuilder Slider(string name) => new(name);
        public static InputFieldBuilder InputField(string name) => new(name);
        public static DropdownBuilder Dropdown(string name) => new(name);
        public static ScrollbarBuilder Scrollbar(string name) => new(name);
        public static ProgressBarBuilder ProgressBar(string name) => new(name);
        public static TabButtonBuilder TabButton(string name) => new(name);
        public static RadioButtonBuilder RadioButton(string name) => new(name);
        public static StepperBuilder Stepper(string name) => new(name);

        // === CONTAINERS ===
        public static HorizontalLayoutBuilder HorizontalLayout(string name) => new(name);
        public static VerticalLayoutBuilder VerticalLayout(string name) => new(name);
        public static GridBuilder Grid(string name) => new(name);
        public static ScrollViewBuilder ScrollView(string name) => new(name);
        public static CardBuilder Card(string name) => new(name);
        public static PanelBuilder Panel(string name) => new(name);
        public static ModalBuilder Modal(string name) => new(name);
        public static TabBarBuilder TabBar(string name) => new(name);
        public static AccordionBuilder Accordion(string name) => new(name);

        // === COMPOSITES (Rows) ===
        public static SliderRowBuilder SliderRow(string name) => new(name);
        public static ToggleRowBuilder ToggleRow(string name) => new(name);
        public static DropdownRowBuilder DropdownRow(string name) => new(name);

        // === GAME UI ===
        public static HealthBarBuilder HealthBar(string name) => new(name);
        public static BadgeBuilder Badge(string name) => new(name);
        public static AvatarBuilder Avatar(string name) => new(name);
    }
}
#endif
