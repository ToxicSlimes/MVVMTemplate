using Core.Abstractions.Config;
using UnityEngine;

namespace Core.Gameplay.Player
{
    public sealed class PlayerConfig : ConfigBase
    {
        [field: SerializeField] public float WalkSpeed { get; private set; } = 2.5f;
        [field: SerializeField] public float LookDegreesPerScreen { get; private set; } = 36f;
        [field: SerializeField] public float PitchLimit { get; private set; } = 80f;
        [field: SerializeField] public float EyeHeight { get; private set; } = 1.6f;
        [field: SerializeField] public float CrouchEyeHeight { get; private set; } = 0.9f;
        [field: SerializeField, Min(0.01f)] public float CrouchSeconds { get; private set; } = 0.25f;
    }
}
