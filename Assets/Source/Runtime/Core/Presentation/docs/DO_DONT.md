# Presentation — DO / DON'T

*Обновлено: 2026-02-11*

---

## ✅ DO

### States
- **Наследуй от `GameStateBase<TSelf>`** — generic self-type для type-safe state machine
- **Реализуй `IEnterState`** для простых state'ов или **`IPayloadState<T>`** для state'ов с параметрами
- **Используй `IViewService.ShowAsync<TViewModel>()`** для показа View при входе в state
- **Загрузку сцен делегируй в `LoadSceneState`** через `SceneLoadPayload` с callback:
  ```csharp
  var payload = new SceneLoadPayload("MyScene", SceneLoadType.Regular,
      LoadSceneMode.Single, ShowMyView);
  StateMachine.Enter<LoadSceneState, SceneLoadPayload>(payload);
  ```
- **`Exit()` должен быть override** — очищай ресурсы state'а

### Navigation Handlers
- **Наследуй от `NavigateHandlerBase<TTarget, TState>`** — одна строка на handler
- **`TTarget : struct, INavigationTarget`** — маркер-структура для типобезопасной навигации
- **Один handler = один переход** — не комбинируй логику

### State Machine
- **Переходы только через `StateMachine.Enter<TState>()`** — не вызывай state'ы напрямую
- **`SetStateMachine()`** вызывается автоматически — не вызывай вручную

---

## ❌ DON'T

- **НЕ пиши UI-логику в State** — State только оркестрирует (загрузка сцены, показ view, переход)
- **НЕ вызывай `Enter()` напрямую** — только через `StateMachine.Enter<TState>()`
- **НЕ создавай циклические переходы** без условий выхода
- **НЕ забывай callback в `SceneLoadPayload`** — без него view не покажется после загрузки сцены
- **НЕ используй `async void`** в State'ах без обработки ошибок — `UniTaskVoid` + `.Forget()` предпочтительнее
- **НЕ ставь State'ы в global namespace** — используй `Core.Presentation.States` (существующий техдолг)
