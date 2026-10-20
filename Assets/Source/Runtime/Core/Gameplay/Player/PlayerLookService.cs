using Core.Gameplay.Input;
using Core.Abstractions.Config;
using Core.Abstractions.MVVM.Models.Settings;
using Scellecs.Morpeh;
using UnityEngine;

namespace Core.Gameplay.Player
{
    public sealed class PlayerLookService
    {
        private readonly ISettingsModel _settings;
        private readonly PlayerConfig _config;
        private readonly InputRepository _inputs;
        private readonly PlayerRepository _players;

        public PlayerLookService(
            ISettingsModel settings, IConfigProvider configs, InputRepository inputs, PlayerRepository players)
        {
            _settings = settings;
            _config = configs.GetConfig<PlayerConfig>();
            _inputs = inputs;
            _players = players;
        }

        public void Look(Entity playerInput, Entity player)
        {
            var controls = _settings.Controls;
            var degrees = _inputs.PlayerInputs.For(playerInput).Look / Screen.height
                * (controls.MouseSensitivity * _config.LookDegreesPerScreen);
            if (controls.InvertYAxis)
            {
                degrees.y = -degrees.y;
            }

            var body = _players.Players.For(player);
            body.Controller.transform.Rotate(0f, degrees.x, 0f);
            var pitch = Mathf.DeltaAngle(0f, body.Camera.localEulerAngles.x) - degrees.y;
            body.Camera.localRotation = Quaternion.Euler(Mathf.Clamp(pitch, -_config.PitchLimit, _config.PitchLimit), 0f, 0f);
        }
    }
}
