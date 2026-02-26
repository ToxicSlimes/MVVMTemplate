using System;
using System.Collections.Generic;
using Core.Abstractions.Attributes;
using Core.Abstractions.Config;
using Core.Data.Domain.Models.Chat.Extensions;
using TriInspector;
using UnityEngine;

namespace Core.Configs.Services
{
    /// <summary>
    /// ScriptableObject configuration for Procedural UI.
    /// Contains references to component prefabs and default settings.
    /// </summary>
    [CreateAssetMenu(fileName = "ProceduralUiConfig", menuName = "EndlessAdventure/Procedural UI Config")]
    public class ProceduralUiConfig : ServiceConfigBase
    {
#if UNITY_EDITOR
        [ShowInInspector, ReadOnly, HideInPlayMode]
        [PropertyOrder(-100)]
        [LabelText("")]
        private string MissingRefsWarning
        {
            get
            {
                if (_componentPrefabs == null || _componentPrefabs.Length == 0)
                    return "";

                int missingCount = 0;
                foreach (var entry in _componentPrefabs)
                {
                    if (entry.Prefab == null) missingCount++;
                }

                if (missingCount > 0)
                {
                    return $"⚠️ MISSING REFERENCES: {missingCount} prefab(s) need attention!";
                }

                return "✅ All prefabs assigned!";
            }
        }
#endif

        #region Prefabs
        
        [Title("🎨 Component Prefabs")]
        [ListDrawerSettings(AlwaysExpanded = true)]
        [SerializeField] 
        private ComponentPrefabEntry[] _componentPrefabs;

        #endregion

        #region Pool Settings
        
        [Title("🏊 Pool Settings")]
        [Range(1, 20)]
        [SerializeField] 
        private int _defaultPoolSize = 5;

        [Range(5, 100)]
        [SerializeField] 
        private int _maxPoolSize = 20;

        #endregion

        #region Image Loading
        
        [Title("🖼️ Image Loading")]
        [Range(10, 200)]
        [SerializeField] 
        private int _imageCacheSize = 50;

        [Range(1f, 30f)]
        [SerializeField] 
        private float _imageLoadTimeout = 10f;

        #endregion

        #region Animation
        
        [Title("✨ Animation")]
        [Range(0.1f, 20f)]
        [SerializeField] 
        private float _defaultFadeSpeed = 5f;

        [Range(0.1f, 10f)]
        [SerializeField] 
        private float _defaultAnimationSpeed = 2f;

        #endregion

        #region Properties

        public int DefaultPoolSize => _defaultPoolSize;
        public int MaxPoolSize => _maxPoolSize;
        public int ImageCacheSize => _imageCacheSize;
        public float ImageLoadTimeout => _imageLoadTimeout;
        public float DefaultFadeSpeed => _defaultFadeSpeed;
        public float DefaultAnimationSpeed => _defaultAnimationSpeed;

        #endregion

        #region Methods

        public GameObject GetPrefab(UiComponentType type)
        {
            if (_componentPrefabs == null) return null;

            foreach (var entry in _componentPrefabs)
            {
                if (entry.Type == type)
                {
                    return entry.Prefab;
                }
            }

            return null;
        }

        public IReadOnlyList<ComponentPrefabEntry> GetAllPrefabs()
        {
            return _componentPrefabs ?? Array.Empty<ComponentPrefabEntry>();
        }

        #endregion

        #region Validation

        protected override void ValidateConfig()
        {
            base.ValidateConfig();

            if (_defaultPoolSize > _maxPoolSize)
            {
                Debug.LogWarning($"[{nameof(ProceduralUiConfig)}] Default pool size ({_defaultPoolSize}) > Max pool size ({_maxPoolSize})!");
            }
        }

        #endregion

        #region Editor

#if UNITY_EDITOR
        [Title("🔧 Actions")]
        [Button(ButtonSizes.Large)]
        private void AutoFindPrefabs()
        {
            var guids = UnityEditor.AssetDatabase.FindAssets("t:Prefab Procedural", new[] { "Assets/Prefabs/ProceduralUI" });
            var foundEntries = new List<ComponentPrefabEntry>();

            var typeNames = new Dictionary<UiComponentType, string>
            {
                { UiComponentType.Button, "ProceduralButton" },
                { UiComponentType.Progress, "ProceduralProgress" },
                { UiComponentType.Display, "ProceduralDisplay" },
                { UiComponentType.Divider, "ProceduralDivider" },
                { UiComponentType.Card, "ProceduralCard" },
                { UiComponentType.Alert, "ProceduralAlert" },
                { UiComponentType.Select, "ProceduralSelect" },
                { UiComponentType.Checkbox, "ProceduralCheckbox" },
                { UiComponentType.Input, "ProceduralInput" },
                { UiComponentType.Image, "ProceduralImage" },
                { UiComponentType.Modal, "ProceduralModal" },
                { UiComponentType.List, "ProceduralList" }
            };

            foreach (var guid in guids)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                foreach (var kvp in typeNames)
                {
                    // Use exact match to avoid ProceduralListItem matching ProceduralList
                    if (prefab.name == kvp.Value)
                    {
                        foundEntries.Add(new ComponentPrefabEntry { Type = kvp.Key, Prefab = prefab });
                        Debug.Log($"[ProceduralUiConfig] Found {kvp.Key}: {prefab.name}");
                    }
                }
            }

            _componentPrefabs = foundEntries.ToArray();
            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log($"[ProceduralUiConfig] Found {foundEntries.Count} prefabs");
        }

        [Button(ButtonSizes.Large)]
        private void BuildAllPrefabs()
        {
            UnityEditor.EditorApplication.ExecuteMenuItem("Tools/Procedural UI/Build All Prefabs");
            UnityEditor.EditorApplication.delayCall += AutoFindPrefabs;
        }

        [ShowInInspector, ReadOnly]
        private string PrefabsStatus
        {
            get
            {
                if (_componentPrefabs == null || _componentPrefabs.Length == 0)
                    return "❌ No prefabs assigned";
                
                int assigned = 0;
                foreach (var entry in _componentPrefabs)
                {
                    if (entry.Prefab != null) assigned++;
                }
                
                return assigned == _componentPrefabs.Length 
                    ? $"✅ {assigned}/{_componentPrefabs.Length} prefabs assigned" 
                    : $"⚠️ {assigned}/{_componentPrefabs.Length} prefabs assigned";
            }
        }
#endif

        #endregion

        [Serializable]
        [ConfigEntryStyle(ColorPreset.Blue, MissingRefFieldName = "Prefab")]
        public class ComponentPrefabEntry
        {
            public UiComponentType Type;

            [Required]
            public GameObject Prefab;
        }
    }
}
