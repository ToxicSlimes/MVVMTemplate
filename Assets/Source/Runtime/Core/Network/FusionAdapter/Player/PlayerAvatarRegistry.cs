using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace Core.Network.FusionAdapter.Player
{
    public sealed class PlayerAvatarRegistry
    {
        private readonly Dictionary<PlayerRef, Transform> _byOwner = new();

        public void Add(PlayerRef owner, Transform avatar)
        {
            if (avatar == null)
            {
                return;
            }

            _byOwner[owner] = avatar;
        }

        public void Remove(PlayerRef owner) => _byOwner.Remove(owner);

        public bool TryGet(PlayerRef owner, out Transform avatar) => _byOwner.TryGetValue(owner, out avatar);
    }
}
