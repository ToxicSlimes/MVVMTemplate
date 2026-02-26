using Core.Abstractions.Services;
using Core.Abstractions.StateMachines.States;
using Core.Abstractions.StateMachines;
using Cysharp.Threading.Tasks;

public class LoadSceneState : GameStateBase<LoadSceneState>, IPayloadState<SceneLoadPayload>
{
    private readonly ILoadSceneService _loadSceneService;

    public LoadSceneState(ILoadSceneService loadSceneService)
    {
        _loadSceneService = loadSceneService;
    }

    public void Enter(SceneLoadPayload payload)
    {
        LoadAsync(payload).Forget();
    }

    private async UniTaskVoid LoadAsync(SceneLoadPayload payload)
    {
        await _loadSceneService.LoadAsync(payload);
    }

    public override void Exit()
    {
    }
}
