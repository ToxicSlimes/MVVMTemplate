using System;
using UnityEngine;

namespace Core.Configs.Services
{
    [Serializable]
    public class NetworkPrefabEntry
    {
        [Tooltip("Логический идентификатор, по которому игровой код запрашивает префаб")]
        [SerializeField] private string id = string.Empty;

        [Tooltip("Имя префаба, зарегистрированного в NetworkProjectConfig")]
        [SerializeField] private string prefabName = string.Empty;

        public string Id => id;
        public string PrefabName => prefabName;
    }
}
