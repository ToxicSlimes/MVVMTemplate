using UnityEngine;

public static class ProjectScopeBootstrapper
{
    private const string ProjectScopeResourcePath = "ProjectScope";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureProjectScope()
    {
        if (Object.FindFirstObjectByType<ProjectInstaller>() != null)
            return;

        var prefab = Resources.Load<GameObject>(ProjectScopeResourcePath);
        if (prefab == null)
        {
            Debug.LogError($"[ProjectScopeBootstrapper] Не найден prefab в Resources/{ProjectScopeResourcePath}");
            return;
        }

        Object.Instantiate(prefab);
    }
}
