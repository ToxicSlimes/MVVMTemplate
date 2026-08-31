#if UNITY_EDITOR
using System.Collections.Generic;
using Core.Network.Abstractions.Types;
using Core.Network.FusionAdapter.Player;
using Core.Network.FusionAdapter.Session;
using Fusion;
using Fusion.Addons.FSM;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core.Editor.NetworkPrefabTools
{
    public static class GameSessionPrefabGenerator
    {
        public const string PrefabPath = "Assets/Prefabs/Network/" + RootName + ".prefab";
        public const string PlayerPrefabPath = "Assets/Prefabs/Network/" + PlayerRootName + ".prefab";
        public const string AvatarPrefabPath = "Assets/Prefabs/Network/" + AvatarRootName + ".prefab";
        public const string BotAvatarPrefabPath = "Assets/Prefabs/Network/" + BotAvatarRootName + ".prefab";

        private const string RootName = NetworkPrefabKeys.GameSessionName;
        private const string PlayerRootName = NetworkPrefabKeys.NetworkPlayerName;
        private const string AvatarRootName = NetworkPrefabKeys.PlayerAvatarName;
        private const string BotAvatarRootName = NetworkPrefabKeys.BotAvatarName;
        private const string FolderParent = "Assets/Prefabs";
        private const string FolderName = "Network";

        [MenuItem("Tools/Network/Build Network Prefabs", false, 1)]
        public static void BuildMenu()
        {
            Build();
            BuildPlayer();
            BuildAvatar();
            BuildBotAvatar();

            Debug.Log(
                $"<color=green>✓ Network prefabs</color>\n{PrefabPath}\n{PlayerPrefabPath}\n{AvatarPrefabPath}\n{BotAvatarPrefabPath}\n" +
                "If spawn cannot find a prefab — Fusion → Rebuild Prefab Table.");
        }

        public static GameObject Build()
        {
            return Save(RootName, PrefabPath, root =>
            {
                root.AddComponent<NetworkObject>();
                root.AddComponent<StateMachineController>();
                root.AddComponent<GameSessionMachine>();
            });
        }

        public static GameObject BuildPlayer()
        {
            return Save(PlayerRootName, PlayerPrefabPath, root =>
            {
                root.AddComponent<NetworkObject>();
                root.AddComponent<NetworkPlayer>();
            });
        }

        public static GameObject BuildAvatar()
        {
            return Save(AvatarRootName, AvatarPrefabPath, root =>
            {
                root.AddComponent<NetworkObject>();
                root.AddComponent<NetworkedAvatarController>();
            });
        }

        public static GameObject BuildBotAvatar()
        {
            return Save(BotAvatarRootName, BotAvatarPrefabPath, root =>
            {
                root.AddComponent<NetworkObject>();
                root.AddComponent<BotAvatarController>();
            });
        }

        private static GameObject Save(string rootName, string path, System.Action<GameObject> compose)
        {
            EnsureDirectory();

            var scene = SceneManager.GetActiveScene();
            var known = new HashSet<GameObject>(scene.GetRootGameObjects());

            var root = new GameObject(rootName);
            compose(root);

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            RemoveStrayRoots(scene, known);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            AssetDatabase.SaveAssets();

            Debug.Log($"<color=green>✓ Network prefab created: {path}</color>");

            return prefab;
        }

        private static void RemoveStrayRoots(Scene scene, HashSet<GameObject> known)
        {
            foreach (var candidate in scene.GetRootGameObjects())
            {
                if (known.Contains(candidate))
                {
                    continue;
                }

                Object.DestroyImmediate(candidate);
            }
        }

        private static void EnsureDirectory()
        {
            if (!AssetDatabase.IsValidFolder($"{FolderParent}/{FolderName}"))
            {
                AssetDatabase.CreateFolder(FolderParent, FolderName);
            }
        }
    }
}
#endif
