# Input Service Abstractions

Контракт для Unity Input System с R3 Reactive Extensions.

## Типы

| Тип | Описание |
|-----|----------|
| `IInputService` | Wrapper над Unity Input System с Observable events |

## IInputService API

```csharp
public interface IInputService : IDisposable
{
    // === UI Events ===
    Observable<Unit> OnCancel { get; }
    Observable<Unit> OnSubmit { get; }
    Observable<Vector2> OnNavigate { get; }
    Observable<Vector2> OnPoint { get; }
    Observable<Unit> OnClick { get; }
    Observable<Unit> OnRightClick { get; }
    Observable<Vector2> OnScroll { get; }

    // === Player Events ===
    Observable<Vector2> OnMove { get; }
    Observable<Vector2> OnLook { get; }
    Observable<Unit> OnAttack { get; }
    Observable<Unit> OnInteract { get; }
    Observable<Unit> OnJump { get; }
    Observable<Unit> OnSprint { get; }
    Observable<Unit> OnCrouch { get; }
    Observable<Unit> OnNext { get; }
    Observable<Unit> OnPrevious { get; }

    // === Action Map Control ===
    void EnablePlayerActions();
    void DisablePlayerActions();
    void EnableUIActions();
    void DisableUIActions();

    // === Polled State ===
    Vector2 MoveVector { get; }
    Vector2 LookVector { get; }
    Vector2 MouseScreenPosition { get; }
    bool IsAttackPressed { get; }
    bool IsJumpPressed { get; }
    bool IsSprintPressed { get; }
}
```

## Использование

### Подписка на события (reactive)

```csharp
public class PlayerController : MonoBehaviour
{
    [Inject] private readonly IInputService _input;
    private DisposableBag _disposables;

    private void Start()
    {
        _input.OnAttack
            .Subscribe(_ => Attack())
            .AddTo(ref _disposables);

        _input.OnMove
            .Subscribe(dir => Move(dir))
            .AddTo(ref _disposables);
    }

    private void OnDestroy() => _disposables.Dispose();
}
```

### Polled state (в Update)

```csharp
private void Update()
{
    var moveDir = _input.MoveVector;
    transform.Translate(moveDir * _speed * Time.deltaTime);

    if (_input.IsSprintPressed)
        _speed = _sprintSpeed;
}
```

### Переключение Action Maps

```csharp
// В меню
_input.DisablePlayerActions();
_input.EnableUIActions();

// В игре
_input.DisableUIActions();
_input.EnablePlayerActions();
```

## Реализация

`Infrastructure/Services/Input/InputService.cs`
