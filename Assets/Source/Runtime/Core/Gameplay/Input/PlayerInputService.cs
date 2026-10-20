using Core.Abstractions.Services.Input;
using Scellecs.Morpeh;
using UnityEngine;

namespace Core.Gameplay.Input
{
    public sealed class PlayerInputService
    {
        private readonly IInputService _input;
        private readonly InputRepository _inputs;

        public PlayerInputService(IInputService input, InputRepository inputs)
        {
            _input = input;
            _inputs = inputs;
        }

        public void Create() => _inputs.CreatePlayerInput();

        public void Remove(Entity playerInput) => _inputs.Remove(playerInput);

        public void Read(Entity playerInput)
        {
            var view = _inputs.PlayerInputs.For(playerInput);
            view.Move = _input.MoveVector;
            view.Look = _input.LookVector;
            view.Attack = _input.IsAttackPressed || view.AutoFire;
        }

        public void AddTouch(Entity playerInput, Entity touchLook)
        {
            var view = _inputs.PlayerInputs.For(playerInput);
            var touch = _inputs.TouchLooks.For(touchLook);
            view.Look += touch.Delta;
            view.Attack |= touch.Held;
            touch.Delta = Vector2.zero;
        }
    }
}
