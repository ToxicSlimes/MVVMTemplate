using System;
using Core.Abstractions.Config;
using Core.Network.Abstractions.Types;
using UnityEngine;

namespace Core.Infrastructure.Configs.Services
{
    [CreateAssetMenu(fileName = "NetworkConfig", menuName = "Configs/Services/Network Config")]
    public class NetworkConfig : ServiceConfigBase
    {
        [Header("Session")]
        [SerializeField] private NetworkMode defaultMode = NetworkMode.Shared;
        [SerializeField] private int defaultPlayerCount = 4;
        [SerializeField] private bool sessionVisible = true;
        [SerializeField] private bool sessionOpen = true;

        [Header("Session Scene")]
        [SerializeField] private string defaultScenePath = string.Empty;

        [Header("Prefabs")]
        [SerializeField] private NetworkPrefabEntry[] prefabs = Array.Empty<NetworkPrefabEntry>();

        public NetworkMode DefaultMode => defaultMode;
        public int DefaultPlayerCount => defaultPlayerCount;
        public bool SessionVisible => sessionVisible;
        public bool SessionOpen => sessionOpen;
        public string DefaultScenePath => defaultScenePath;

        public NetworkStartRequest BuildStartRequest(string sessionName = null) =>
            new(defaultMode, sessionName, defaultPlayerCount, defaultScenePath, sessionVisible, sessionOpen);

        public bool TryGetPrefab(string id, out NetworkPrefabKey prefab)
        {
            for (var i = 0; i < prefabs.Length; i++)
            {
                if (!string.Equals(prefabs[i].Id, id, StringComparison.Ordinal))
                {
                    continue;
                }

                prefab = new NetworkPrefabKey(prefabs[i].PrefabName);
                return prefab.IsValid;
            }

            prefab = default;
            return false;
        }
    }
}
