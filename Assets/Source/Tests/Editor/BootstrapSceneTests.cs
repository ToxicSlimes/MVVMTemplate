using System.Linq;
using NUnit.Framework;
using Reflex.Core;
using UnityEditor;
using UnityEngine;

namespace Core.Tests
{
    public sealed class BootstrapSceneTests
    {
        private const string BootstrapSceneGuid = "944f598b69f41441a903e7491a704675";

        [Test]
        public void BootstrapScene_DoesNotDependOnProjectScopePrefab()
        {
            var scenePath = AssetDatabase.GUIDToAssetPath(BootstrapSceneGuid);
            Assert.That(AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath), Is.Not.Null,
                "BootstrapScene must be present and imported.");

            var scopePrefabs = AssetDatabase.GetDependencies(scenePath, true)
                .Where(path => path.EndsWith(".prefab", System.StringComparison.OrdinalIgnoreCase))
                .Where(path =>
                {
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    Assert.That(prefab, Is.Not.Null, $"Could not load prefab dependency: {path}");
                    return prefab.GetComponentsInChildren<ProjectScope>(true).Length > 0;
                })
                .ToArray();

            Assert.That(scopePrefabs, Is.Empty,
                "Reflex creates the project scope; BootstrapScene must not reference another ProjectScope prefab.");
        }
    }
}
