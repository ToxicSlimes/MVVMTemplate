using TriInspector;

namespace Core.Abstractions.Config
{
    /// <summary>
    /// Базовый класс для data конфигов (статические данные, таблицы, справочники).
    /// </summary>
    [DeclareBoxGroup("data", Title = "📊 Data")]
    [DeclareBoxGroup("references", Title = "🔗 References")]
    [DeclareBoxGroup("validation", Title = "✅ Validation")]
    public abstract class DataConfigBase : ConfigBase
    {
        protected override string ConfigTypeName => $"📦 {base.ConfigTypeName}";

#if UNITY_EDITOR
        [Button(ButtonSizes.Large), Group("validation"), PropertyOrder(50)]
        protected virtual void ReloadData()
        {
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEngine.Debug.Log($"[{GetType().Name}] Data reloaded");
        }

        [Button(ButtonSizes.Large), Group("validation"), PropertyOrder(51)]
        protected virtual void ValidateAll()
        {
            ValidateConfig();
            UnityEngine.Debug.Log($"[{GetType().Name}] Validation complete");
        }
#endif
    }
}
