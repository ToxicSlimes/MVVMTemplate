#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EndlessAdventure.Editor.PrefabBuilderSystem.Templates
{
    [CreateAssetMenu(fileName = "NewUITemplate", menuName = "PrefabBuilder/UI Template")]
    public class UITemplate : ScriptableObject
    {
        [Tooltip("Template display name")]
        public string DisplayName;

        [Tooltip("Template description")]
        [TextArea(2, 4)]
        public string Description;

        [Tooltip("Category for grouping in UI")]
        public string Category = "General";

        [Tooltip("Icon for display in Hub")]
        public Sprite Icon;

        [Tooltip("Root node of the template")]
        public UITemplateNode Root;

        [Tooltip("Tags for searching")]
        public List<string> Tags = new();

        [Header("Prefab Settings")]
        [Tooltip("Default output path for generated prefab")]
        public string DefaultOutputPath = "Assets/Prefabs/UI/";

        [Tooltip("Add Canvas component to root")]
        public bool AddCanvas;

        [Tooltip("Add CanvasScaler if Canvas is added")]
        public bool AddCanvasScaler = true;

        public void Initialize(string displayName, string description, string category = "General")
        {
            DisplayName = displayName;
            Description = description;
            Category = category;
            Root = new UITemplateNode("Root", "Root", UIElementType.Container) { Stretch = true };
        }

        public UITemplateNode FindNode(string id)
        {
            return FindNodeRecursive(Root, id);
        }

        private UITemplateNode FindNodeRecursive(UITemplateNode node, string id)
        {
            if (node == null) return null;
            if (node.Id == id) return node;

            foreach (var child in node.Children)
            {
                var found = FindNodeRecursive(child, id);
                if (found != null) return found;
            }

            return null;
        }

        public List<UITemplateNode> GetAllNodes()
        {
            var nodes = new List<UITemplateNode>();
            CollectNodes(Root, nodes);
            return nodes;
        }

        private void CollectNodes(UITemplateNode node, List<UITemplateNode> nodes)
        {
            if (node == null) return;
            nodes.Add(node);
            foreach (var child in node.Children)
            {
                CollectNodes(child, nodes);
            }
        }

        public int NodeCount => GetAllNodes().Count;

        private void OnValidate()
        {
            if (Root == null)
            {
                Root = new UITemplateNode("Root", "Root", UIElementType.Container) { Stretch = true };
            }
        }
    }
}
#endif
