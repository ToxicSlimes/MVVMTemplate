using System.Collections.Generic;
using EndlessAdventure.Editor.PrefabBuilderSystem.Conventions;
using UnityEngine;
using UnityEngine.UI;

namespace EndlessAdventure.Editor.PrefabBuilderSystem.Core
{
    public class PrefabBuilderContext
    {
        public GameObject Root { get; set; }
        public Canvas Canvas { get; set; }
        public CanvasScaler CanvasScaler { get; set; }
        public CanvasGroup CanvasGroup { get; set; }
        public GraphicRaycaster Raycaster { get; set; }

        private readonly Dictionary<string, Component> _components = new();
        private readonly Dictionary<string, GameObject> _gameObjects = new();
        private readonly HashSet<string> _registeredNames = new();

        public void Register<T>(string key, T component) where T : Component
        {
            ValidateKey(key);
            _components[key] = component;
            _gameObjects[key] = component.gameObject;
            _registeredNames.Add(key);
        }

        public void RegisterGameObject(string key, GameObject go)
        {
            ValidateKey(key);
            _gameObjects[key] = go;
            _registeredNames.Add(key);
        }

        private void ValidateKey(string key)
        {
            var oldLogWarnings = NamingConvention.LogWarnings;
            NamingConvention.LogWarnings = false;
            
            var result = NamingConvention.Validate(key, _registeredNames);
            
            NamingConvention.LogWarnings = oldLogWarnings;
            
            if (result.HasError && NamingConvention.StrictMode)
            {
                throw new System.ArgumentException($"[NamingConvention] {result.Message}");
            }
        }

        public T Get<T>(string key) where T : Component
        {
            if (_components.TryGetValue(key, out var c))
                return c as T;
            return null;
        }

        public GameObject GetGameObject(string key)
        {
            return _gameObjects.TryGetValue(key, out var go) ? go : null;
        }

        public bool TryGet<T>(string key, out T component) where T : Component
        {
            component = Get<T>(key);
            return component != null;
        }

        public IEnumerable<string> GetAllKeys() => _components.Keys;

        public void Clear()
        {
            _components.Clear();
            _gameObjects.Clear();
            _registeredNames.Clear();
        }

        public HashSet<string> GetRegisteredNames() => new(_registeredNames);

        public List<ValidationResult> ValidateAllNames()
        {
            return NamingConvention.ValidateAll(_registeredNames);
        }
    }
}
