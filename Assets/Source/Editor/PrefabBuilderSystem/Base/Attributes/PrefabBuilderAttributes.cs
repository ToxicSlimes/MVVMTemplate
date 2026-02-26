#if UNITY_EDITOR
using System;

namespace EndlessAdventure.Editor.PrefabBuilderSystem.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class HubEntryAttribute : Attribute
    {
        public string Name { get; }
        public string Description { get; }
        public string Category { get; }
        public int Order { get; set; } = 100;

        public HubEntryAttribute(string name, string description, string category = "General")
        {
            Name = name;
            Description = description;
            Category = category;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class PrefabPathAttribute : Attribute
    {
        public string Path { get; }
        public PrefabPathAttribute(string path) => Path = path;
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class BuilderWindowAttribute : Attribute
    {
        public Type WindowType { get; }
        public BuilderWindowAttribute(Type windowType) => WindowType = windowType;
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class SettingAttribute : Attribute
    {
        public string Label { get; }
        public string Category { get; set; }
        public string Tooltip { get; set; }
        public float Min { get; set; } = float.MinValue;
        public float Max { get; set; } = float.MaxValue;

        public SettingAttribute(string label = null) => Label = label;
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class SettingsCategoryAttribute : Attribute
    {
        public string Name { get; }
        public int Order { get; set; }
        public bool Foldout { get; set; } = true;

        public SettingsCategoryAttribute(string name) => Name = name;
    }
}
#endif
