using Core.Abstractions.UI;
using UnityEngine;

namespace Core.Presentation.UI.States
{
    [CreateAssetMenu(fileName = "ControlVisualConfig", menuName = "MVVM/UI/Control Visual Config")]
    public class ControlVisualConfig : ScriptableObject
    {
        [Header("Normal")]
        public Color NormalColor = Color.white;
        public float NormalScale = 1f;
        public float NormalAlpha = 1f;

        [Header("Hovered")]
        public Color HoveredColor = new(0.9f, 0.9f, 0.9f, 1f);
        public float HoveredScale = 1.02f;
        public float HoveredAlpha = 1f;
        public float HoverTransitionDuration = 0.15f;

        [Header("Pressed")]
        public Color PressedColor = new(0.8f, 0.8f, 0.8f, 1f);
        public float PressedScale = 0.96f;
        public float PressedAlpha = 1f;
        public float PressTransitionDuration = 0.1f;

        [Header("Selected")]
        public Color SelectedColor = new(0.85f, 0.85f, 1f, 1f);
        public float SelectedScale = 1f;
        public float SelectedAlpha = 1f;

        [Header("Disabled")]
        public Color DisabledColor = new(0.5f, 0.5f, 0.5f, 1f);
        public float DisabledScale = 1f;
        public float DisabledAlpha = 0.5f;

        [Header("Error")]
        public Color ErrorColor = new(1f, 0.3f, 0.3f, 1f);
        public float ErrorScale = 1f;
        public float ErrorAlpha = 1f;
        public float ErrorTransitionDuration = 0.2f;

        public Color GetColor(ControlState state) => state switch
        {
            ControlState.Normal => NormalColor,
            ControlState.Hovered => HoveredColor,
            ControlState.Pressed => PressedColor,
            ControlState.Selected => SelectedColor,
            ControlState.Disabled => DisabledColor,
            ControlState.Error => ErrorColor,
            _ => NormalColor
        };

        public float GetScale(ControlState state) => state switch
        {
            ControlState.Normal => NormalScale,
            ControlState.Hovered => HoveredScale,
            ControlState.Pressed => PressedScale,
            ControlState.Selected => SelectedScale,
            ControlState.Disabled => DisabledScale,
            ControlState.Error => ErrorScale,
            _ => NormalScale
        };

        public float GetAlpha(ControlState state) => state switch
        {
            ControlState.Normal => NormalAlpha,
            ControlState.Hovered => HoveredAlpha,
            ControlState.Pressed => PressedAlpha,
            ControlState.Selected => SelectedAlpha,
            ControlState.Disabled => DisabledAlpha,
            ControlState.Error => ErrorAlpha,
            _ => NormalAlpha
        };

        public float GetTransitionDuration(ControlState state) => state switch
        {
            ControlState.Hovered => HoverTransitionDuration,
            ControlState.Pressed => PressTransitionDuration,
            ControlState.Error => ErrorTransitionDuration,
            _ => 0.15f
        };
    }
}
