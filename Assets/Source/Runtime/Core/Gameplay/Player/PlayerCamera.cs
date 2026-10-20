using System;
using Scellecs.Morpeh;
using UnityEngine;

namespace Core.Gameplay.Player
{
    [Serializable]
    public struct PlayerCamera : IComponent
    {
        public Transform Value;
    }
}
