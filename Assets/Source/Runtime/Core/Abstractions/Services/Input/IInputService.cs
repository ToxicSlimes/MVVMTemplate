using System;
using R3;
using UnityEngine;

namespace Core.Abstractions.Services.Input
{
    public interface IInputService : IDisposable
    {
        Observable<Unit> OnCancel { get; }
        Observable<Unit> OnSubmit { get; }
        Observable<Vector2> OnNavigate { get; }
        Observable<Vector2> OnPoint { get; }
        Observable<Unit> OnClick { get; }
        Observable<Unit> OnRightClick { get; }
        Observable<Vector2> OnScroll { get; }

        Observable<Vector2> OnMove { get; }
        Observable<Vector2> OnLook { get; }
        Observable<Unit> OnAttack { get; }
        Observable<Unit> OnInteract { get; }
        Observable<Unit> OnJump { get; }
        Observable<Unit> OnSprint { get; }
        Observable<Unit> OnCrouch { get; }
        Observable<Unit> OnNext { get; }
        Observable<Unit> OnPrevious { get; }

        void EnablePlayerActions();
        void DisablePlayerActions();
        void EnableUIActions();
        void DisableUIActions();

        Vector2 MoveVector { get; }
        Vector2 LookVector { get; }
        Vector2 MouseScreenPosition { get; }
        bool IsAttackPressed { get; }
        bool IsInteractPressed { get; }
        bool IsMenuPressed { get; }
        bool IsJumpPressed { get; }
        bool IsSprintPressed { get; }
    }
}
