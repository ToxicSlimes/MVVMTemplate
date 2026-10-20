using Core.Abstractions.Config;
using Core.Gameplay.Input;
using Scellecs.Morpeh;
using UnityEngine;

namespace Core.Gameplay.Player
{
    public sealed class PlayerMovementService
    {
        private readonly PlayerConfig _config;
        private readonly InputRepository _inputs;
        private readonly PlayerRepository _players;

        public PlayerMovementService(IConfigProvider configs, InputRepository inputs, PlayerRepository players)
        {
            _config = configs.GetConfig<PlayerConfig>();
            _inputs = inputs;
            _players = players;
        }

        public void Move(Entity playerInput, Entity player, float deltaTime)
        {
            var move = _inputs.PlayerInputs.For(playerInput).Move;
            var controller = _players.Players.For(player).Controller;
            var transform = controller.transform;

            var fallSpeed = controller.isGrounded ? 0f : controller.velocity.y;
            fallSpeed += Physics.gravity.y * deltaTime;

            var walk = (transform.right * move.x + transform.forward * move.y) * _config.WalkSpeed;
            controller.Move((walk + Vector3.up * fallSpeed) * deltaTime);
        }
    }
}
