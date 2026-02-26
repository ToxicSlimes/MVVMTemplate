using Core.Abstractions.Services.Input;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Core.Infrastructure.Services.Input
{
    public sealed class InputService : IInputService
    {
        private readonly InputSystem_Actions _actions;
        private bool _disposed;

        private readonly Subject<Unit> _onCancel = new();
        private readonly Subject<Unit> _onSubmit = new();
        private readonly Subject<Vector2> _onNavigate = new();
        private readonly Subject<Vector2> _onPoint = new();
        private readonly Subject<Unit> _onClick = new();
        private readonly Subject<Unit> _onRightClick = new();
        private readonly Subject<Vector2> _onScroll = new();

        private readonly Subject<Vector2> _onMove = new();
        private readonly Subject<Vector2> _onLook = new();
        private readonly Subject<Unit> _onAttack = new();
        private readonly Subject<Unit> _onInteract = new();
        private readonly Subject<Unit> _onJump = new();
        private readonly Subject<Unit> _onSprint = new();
        private readonly Subject<Unit> _onCrouch = new();
        private readonly Subject<Unit> _onNext = new();
        private readonly Subject<Unit> _onPrevious = new();

        public Observable<Unit> OnCancel => _onCancel;
        public Observable<Unit> OnSubmit => _onSubmit;
        public Observable<Vector2> OnNavigate => _onNavigate;
        public Observable<Vector2> OnPoint => _onPoint;
        public Observable<Unit> OnClick => _onClick;
        public Observable<Unit> OnRightClick => _onRightClick;
        public Observable<Vector2> OnScroll => _onScroll;

        public Observable<Vector2> OnMove => _onMove;
        public Observable<Vector2> OnLook => _onLook;
        public Observable<Unit> OnAttack => _onAttack;
        public Observable<Unit> OnInteract => _onInteract;
        public Observable<Unit> OnJump => _onJump;
        public Observable<Unit> OnSprint => _onSprint;
        public Observable<Unit> OnCrouch => _onCrouch;
        public Observable<Unit> OnNext => _onNext;
        public Observable<Unit> OnPrevious => _onPrevious;

        public Vector2 MoveVector => _actions.Player.Move.ReadValue<Vector2>();
        public Vector2 LookVector => _actions.Player.Look.ReadValue<Vector2>();
        public Vector2 MouseScreenPosition => UnityEngine.InputSystem.Mouse.current?.position.ReadValue() ?? Vector2.zero;
        public bool IsAttackPressed => _actions.Player.Attack.IsPressed();
        public bool IsJumpPressed => _actions.Player.Jump.IsPressed();
        public bool IsSprintPressed => _actions.Player.Sprint.IsPressed();

        public InputService()
        {
            _actions = new InputSystem_Actions();
            SubscribeToActions();
            EnableUIActions();
            EnablePlayerActions();
        }

        private void SubscribeToActions()
        {
            _actions.UI.Cancel.performed += OnCancelPerformed;
            _actions.UI.Submit.performed += OnSubmitPerformed;
            _actions.UI.Navigate.performed += OnNavigatePerformed;
            _actions.UI.Point.performed += OnPointPerformed;
            _actions.UI.Click.performed += OnClickPerformed;
            _actions.UI.RightClick.performed += OnRightClickPerformed;
            _actions.UI.ScrollWheel.performed += OnScrollPerformed;

            _actions.Player.Move.performed += OnMovePerformed;
            _actions.Player.Move.canceled += OnMoveCanceled;
            _actions.Player.Look.performed += OnLookPerformed;
            _actions.Player.Look.canceled += OnLookCanceled;
            _actions.Player.Attack.performed += OnAttackPerformed;
            _actions.Player.Interact.performed += OnInteractPerformed;
            _actions.Player.Jump.performed += OnJumpPerformed;
            _actions.Player.Sprint.performed += OnSprintPerformed;
            _actions.Player.Crouch.performed += OnCrouchPerformed;
            _actions.Player.Next.performed += OnNextPerformed;
            _actions.Player.Previous.performed += OnPreviousPerformed;
        }

        private void UnsubscribeFromActions()
        {
            _actions.UI.Cancel.performed -= OnCancelPerformed;
            _actions.UI.Submit.performed -= OnSubmitPerformed;
            _actions.UI.Navigate.performed -= OnNavigatePerformed;
            _actions.UI.Point.performed -= OnPointPerformed;
            _actions.UI.Click.performed -= OnClickPerformed;
            _actions.UI.RightClick.performed -= OnRightClickPerformed;
            _actions.UI.ScrollWheel.performed -= OnScrollPerformed;

            _actions.Player.Move.performed -= OnMovePerformed;
            _actions.Player.Move.canceled -= OnMoveCanceled;
            _actions.Player.Look.performed -= OnLookPerformed;
            _actions.Player.Look.canceled -= OnLookCanceled;
            _actions.Player.Attack.performed -= OnAttackPerformed;
            _actions.Player.Interact.performed -= OnInteractPerformed;
            _actions.Player.Jump.performed -= OnJumpPerformed;
            _actions.Player.Sprint.performed -= OnSprintPerformed;
            _actions.Player.Crouch.performed -= OnCrouchPerformed;
            _actions.Player.Next.performed -= OnNextPerformed;
            _actions.Player.Previous.performed -= OnPreviousPerformed;
        }

        private void OnCancelPerformed(InputAction.CallbackContext ctx) => _onCancel.OnNext(Unit.Default);
        private void OnSubmitPerformed(InputAction.CallbackContext ctx) => _onSubmit.OnNext(Unit.Default);
        private void OnNavigatePerformed(InputAction.CallbackContext ctx) => _onNavigate.OnNext(ctx.ReadValue<Vector2>());
        private void OnPointPerformed(InputAction.CallbackContext ctx) => _onPoint.OnNext(ctx.ReadValue<Vector2>());
        private void OnClickPerformed(InputAction.CallbackContext ctx) => _onClick.OnNext(Unit.Default);
        private void OnRightClickPerformed(InputAction.CallbackContext ctx) => _onRightClick.OnNext(Unit.Default);
        private void OnScrollPerformed(InputAction.CallbackContext ctx) => _onScroll.OnNext(ctx.ReadValue<Vector2>());

        private void OnMovePerformed(InputAction.CallbackContext ctx) => _onMove.OnNext(ctx.ReadValue<Vector2>());
        private void OnMoveCanceled(InputAction.CallbackContext ctx) => _onMove.OnNext(Vector2.zero);
        private void OnLookPerformed(InputAction.CallbackContext ctx) => _onLook.OnNext(ctx.ReadValue<Vector2>());
        private void OnLookCanceled(InputAction.CallbackContext ctx) => _onLook.OnNext(Vector2.zero);
        private void OnAttackPerformed(InputAction.CallbackContext ctx) => _onAttack.OnNext(Unit.Default);
        private void OnInteractPerformed(InputAction.CallbackContext ctx) => _onInteract.OnNext(Unit.Default);
        private void OnJumpPerformed(InputAction.CallbackContext ctx) => _onJump.OnNext(Unit.Default);
        private void OnSprintPerformed(InputAction.CallbackContext ctx) => _onSprint.OnNext(Unit.Default);
        private void OnCrouchPerformed(InputAction.CallbackContext ctx) => _onCrouch.OnNext(Unit.Default);
        private void OnNextPerformed(InputAction.CallbackContext ctx) => _onNext.OnNext(Unit.Default);
        private void OnPreviousPerformed(InputAction.CallbackContext ctx) => _onPrevious.OnNext(Unit.Default);

        public void EnablePlayerActions() => _actions.Player.Enable();
        public void DisablePlayerActions() => _actions.Player.Disable();
        public void EnableUIActions() => _actions.UI.Enable();
        public void DisableUIActions() => _actions.UI.Disable();

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            UnsubscribeFromActions();

            _onCancel.Dispose();
            _onSubmit.Dispose();
            _onNavigate.Dispose();
            _onPoint.Dispose();
            _onClick.Dispose();
            _onRightClick.Dispose();
            _onScroll.Dispose();

            _onMove.Dispose();
            _onLook.Dispose();
            _onAttack.Dispose();
            _onInteract.Dispose();
            _onJump.Dispose();
            _onSprint.Dispose();
            _onCrouch.Dispose();
            _onNext.Dispose();
            _onPrevious.Dispose();

            _actions.Dispose();
        }
    }
}
