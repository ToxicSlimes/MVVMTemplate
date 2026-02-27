#if UNITY_EDITOR
using UnityEngine;

namespace Core.Editor.PrefabBuilderSystem.Settings
{
    public abstract class BuilderSettingsBase : ScriptableObject
    {
        public abstract void ResetToDefaults();

        public T Clone<T>() where T : BuilderSettingsBase
        {
            return Instantiate(this) as T;
        }
    }
}
#endif
