#if UNITY_EDITOR
using Core.Network.FusionAdapter.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Core.Editor.NetworkPrefabTools
{
    public static class GameSceneBuilder
    {
        public const string ScenePath = "Assets/Source/Assets/Scenes/GameplayScene.unity";

        private const string GroundName = "Ground";
        private const string SpawnRootName = "SpawnPoints";
        private const int SpawnCount = 4;
        private const float SpawnRadius = 4f;
        private const float GroundScale = 2f;

        [MenuItem("Tools/Network/Build Gameplay Scene", false, 2)]
        public static void BuildMenu()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            RemoveDuplicateEventSystem();
            ClearTemplateRoots();
            BuildGround();
            BuildSpawnPoints();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log($"<color=green>Gameplay scene rebuilt: {ScenePath}</color>");
        }

        private static void RemoveDuplicateEventSystem()
        {
            var existing = Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();

            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }
        }

        private static void ClearTemplateRoots()
        {
            DestroyByName(GroundName);
            DestroyByName(SpawnRootName);
        }

        private static void DestroyByName(string name)
        {
            var existing = GameObject.Find(name);

            if (existing != null)
            {
                Object.DestroyImmediate(existing);
            }
        }

        private static void BuildGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = GroundName;
            ground.transform.localScale = new Vector3(GroundScale, 1f, GroundScale);
            ground.isStatic = true;
        }

        private static void BuildSpawnPoints()
        {
            var root = new GameObject(SpawnRootName);
            var points = new Transform[SpawnCount];

            for (var i = 0; i < SpawnCount; i++)
            {
                var angle = i * Mathf.PI * 2f / SpawnCount;
                var position = new Vector3(Mathf.Cos(angle) * SpawnRadius, 0f, Mathf.Sin(angle) * SpawnRadius);
                var point = new GameObject($"Spawn_{i}");

                point.transform.SetParent(root.transform, false);
                point.transform.position = position;
                point.transform.rotation = Quaternion.LookRotation(-position.normalized, Vector3.up);
                points[i] = point.transform;
            }

            var group = root.AddComponent<PlayerSpawnPoints>();
            var serialized = new SerializedObject(group);
            var array = serialized.FindProperty("_points");

            array.arraySize = points.Length;

            for (var i = 0; i < points.Length; i++)
            {
                array.GetArrayElementAtIndex(i).objectReferenceValue = points[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
