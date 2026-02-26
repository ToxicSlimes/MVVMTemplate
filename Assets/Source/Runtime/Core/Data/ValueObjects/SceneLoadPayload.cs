using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

public readonly struct SceneLoadPayload
{
    public SceneLoadPayload(int sceneIndex, SceneLoadType loadType, LoadSceneMode mode = LoadSceneMode.Single, Action sceneLoaded = null, Func<CancellationToken, UniTask> activationAwaiter = null)
    {
        SceneIndex = sceneIndex;
        SceneName = null;
        LoadType = loadType;
        Mode = mode;
        SceneLoaded = sceneLoaded;
        ActivationAwaiter = activationAwaiter;
    }

    public SceneLoadPayload(string sceneName, SceneLoadType loadType, LoadSceneMode mode = LoadSceneMode.Single, Action sceneLoaded = null, Func<CancellationToken, UniTask> activationAwaiter = null)
    {
        SceneIndex = -1;
        SceneName = sceneName;
        LoadType = loadType;
        Mode = mode;
        SceneLoaded = sceneLoaded;
        ActivationAwaiter = activationAwaiter;
    }



    public SceneLoadType LoadType { get; }
    public LoadSceneMode Mode { get; }
    public Action SceneLoaded { get; }
    public int SceneIndex { get; }
    public string SceneName { get; }
    public Func<CancellationToken, UniTask> ActivationAwaiter { get; }
}
