using TriInspector;
using UnityEngine;

namespace Core.Abstractions.Config
{
    /// <summary>
    /// Базовый класс для view/UI конфигов (prefabs, стили, анимации).
    /// </summary>
    [DeclareBoxGroup("prefabs", Title = "🎨 Prefabs")]
    [DeclareBoxGroup("styles", Title = "🖌️ Styles")]
    [DeclareBoxGroup("animations", Title = "✨ Animations")]
    [DeclareBoxGroup("sounds", Title = "🔊 Sounds")]
    public abstract class ViewConfigBase : ConfigBase
    {
        protected override string ConfigTypeName => $"🖼️ {base.ConfigTypeName}";

#if UNITY_EDITOR
        [Button(ButtonSizes.Large), PropertyOrder(50)]
        protected virtual void FindMissingReferences()
        {
            var fields = GetType().GetFields(
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Public);
            
            int missing = 0;
            foreach (var field in fields)
            {
                if (typeof(Object).IsAssignableFrom(field.FieldType))
                {
                    var value = field.GetValue(this) as Object;
                    if (value == null)
                    {
                        Debug.LogWarning($"[{GetType().Name}] Missing reference: {field.Name}");
                        missing++;
                    }
                }
            }
            
            if (missing == 0)
            {
                Debug.Log($"[{GetType().Name}] ✅ All references assigned!");
            }
            else
            {
                Debug.LogWarning($"[{GetType().Name}] ⚠️ {missing} missing references found");
            }
        }
#endif
    }
}
