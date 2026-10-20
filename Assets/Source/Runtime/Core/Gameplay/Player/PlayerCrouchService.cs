using Core.Abstractions.Config;
using Scellecs.Morpeh;
using UnityEngine;

namespace Core.Gameplay.Player
{
    public sealed class PlayerCrouchService
    {
        private readonly PlayerConfig _config;
        private readonly PlayerRepository _players;

        public PlayerCrouchService(IConfigProvider configs, PlayerRepository players)
        {
            _config = configs.GetConfig<PlayerConfig>();
            _players = players;
        }

        public void Apply(Entity request, Entity player) =>
            _players.Crouching.Set(player, new Crouching { Value = _players.CrouchRequests.Get(request).Value });

        public void MoveCamera(Entity player, float deltaTime)
        {
            var crouching = _players.Crouching.Has(player) && _players.Crouching.Get(player).Value;
            var camera = _players.Players.For(player).Camera;
            var position = camera.localPosition;
            var speed = (_config.EyeHeight - _config.CrouchEyeHeight) / _config.CrouchSeconds;
            position.y = Mathf.MoveTowards(position.y, crouching ? _config.CrouchEyeHeight : _config.EyeHeight, speed * deltaTime);
            camera.localPosition = position;
        }
    }
}
