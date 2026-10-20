using System;
using Scellecs.Morpeh;

namespace Core.Gameplay.Player
{
    [Serializable]
    public struct SetCrouchRequest : IComponent
    {
        public bool Value;
    }
}
