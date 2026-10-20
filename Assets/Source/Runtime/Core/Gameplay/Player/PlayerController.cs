using System;
using Scellecs.Morpeh;
using UnityEngine;

namespace Core.Gameplay.Player
{
    [Serializable]
    public struct PlayerController : IComponent
    {
        public CharacterController Value;
    }
}
