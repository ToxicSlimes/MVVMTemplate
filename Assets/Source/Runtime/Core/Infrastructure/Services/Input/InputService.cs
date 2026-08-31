using Core.Abstractions.Services.Input;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace Core.Infrastructure.Services.Input
{
    public sealed class InputService : IInputService
    {
        private readonly InputSystem_Actions _actions;
        private bool _disposed;
        private bool _uiModuleAdopted;

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

        /// <summary>
        /// Читается нажатие, а не срабатывание действия: на <c>Interact</c> висит интеракция
        /// Hold, и её <c>performed</c> приходит только после удержания. Симуляции нужен факт
        /// «кнопка нажата в этот тик», из которого сама выводится и нажатие, и удержание.
        /// </summary>
        public bool IsInteractPressed => _actions.Player.Interact.IsPressed();
        public bool IsJumpPressed => _actions.Player.Jump.IsPressed();
        public bool IsSprintPressed => _actions.Player.Sprint.IsPressed();

        /// <summary>
        /// Игра начинается в меню, поэтому включена только карта UI. Держать обе разом нельзя:
        /// WASD со стрелками привязаны и к <c>Player/Move</c>, и к <c>UI/Navigate</c>, а левая
        /// кнопка мыши — к <c>Player/Attack</c> и <c>UI/Click</c>. Одно нажатие делало бы два дела.
        /// </summary>
        public InputService()
        {
            _actions = new InputSystem_Actions();
            SubscribeToActions();
            EnableUIActions();
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

        /// <remarks>
        /// Курсор прячется вместе с включением игрового ввода, а не отдельным вызовом: обзор
        /// поворачивается мышью, и видимый курсор в этот момент упирался бы в край экрана,
        /// обрывая поворот. Оба состояния меняются в одном месте, чтобы не разъезжались.
        /// </remarks>
        public void EnablePlayerActions()
        {
            _actions.Player.Enable();
            SetCursorLocked(true);
        }

        public void DisablePlayerActions() => _actions.Player.Disable();

        public void EnableUIActions()
        {
            _actions.UI.Enable();
            SetUIModuleEnabled(true);
            SetCursorLocked(false);
        }

        public void DisableUIActions()
        {
            SetUIModuleEnabled(false);
            _actions.UI.Disable();
        }

        /// <summary>
        /// Нажатие клавиши выхода, работающее в любом состоянии ввода.
        /// </summary>
        /// <remarks>
        /// Читается прямо с клавиатуры, а не действием из карты: <c>Cancel</c> живёт в карте UI,
        /// а в бою она выключена — меню паузы оттуда было бы не открыть. Заводить то же самое
        /// вторым действием в игровой карте значило бы держать две привязки к одной клавише и
        /// ловить её дважды, когда обе карты активны.
        /// </remarks>
        public bool IsMenuPressed => Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;

        private static void SetCursorLocked(bool isLocked)
        {
            Cursor.lockState = isLocked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !isLocked;
        }

        /// <summary>
        /// Приводит модуль ввода EventSystem к общему с сервисом ассету действий и открывает
        /// либо закрывает ему доступ к вводу.
        /// </summary>
        /// <remarks>
        /// По умолчанию модуль ссылается на пакетный <c>DefaultInputActions</c> — отдельный
        /// экземпляр, из-за чего в игре жили две независимые копии действий: выключение карты
        /// здесь до модуля не доходило, а WASD продолжал двигать выделение по кнопкам экрана.
        /// Отдаём модулю наш ассет — ссылки он перевяжет сам, сопоставив имена карты и действий,
        /// а они совпадают со всеми десятью, которые он ищет.
        /// Гасим при этом сам модуль, а не карту: включением и выключением своих действий он
        /// распоряжается сам, со счётчиком ссылок, и вмешательство извне этот учёт ломает.
        /// EventSystem создаётся позже сервиса, поэтому привязка ленивая.
        /// </remarks>
        private void SetUIModuleEnabled(bool isEnabled)
        {
            var eventSystem = EventSystem.current;

            if (eventSystem == null)
            {
                return;
            }

            var module = eventSystem.currentInputModule as InputSystemUIInputModule;

            if (module == null)
            {
                return;
            }

            AdoptUIModule(module);
            module.enabled = isEnabled;
        }

        /// <summary>
        /// Смена ассета действия заново не включает, поэтому работающий модуль передёргиваем:
        /// его собственные OnDisable и OnEnable сделают это правильно.
        /// </summary>
        private void AdoptUIModule(InputSystemUIInputModule module)
        {
            if (_uiModuleAdopted)
            {
                return;
            }

            _uiModuleAdopted = true;
            module.actionsAsset = _actions.asset;

            if (!module.enabled)
            {
                return;
            }

            module.enabled = false;
            module.enabled = true;
        }

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
