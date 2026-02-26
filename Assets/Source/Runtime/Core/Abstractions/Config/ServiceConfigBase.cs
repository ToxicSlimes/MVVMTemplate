using TriInspector;

namespace Core.Abstractions.Config
{
    /// <summary>
    /// Базовый класс для конфигов сервисов.
    /// Добавляет стандартные секции для настроек сервисов.
    /// </summary>
    [DeclareBoxGroup("service", Title = "⚙️ Service Settings")]
    [DeclareBoxGroup("pool", Title = "🏊 Pool Settings")]
    [DeclareBoxGroup("cache", Title = "💾 Cache Settings")]
    [DeclareBoxGroup("network", Title = "🌐 Network Settings")]
    [DeclareBoxGroup("debug", Title = "🐛 Debug")]
    public abstract class ServiceConfigBase : ConfigBase
    {
        protected override string ConfigTypeName => $"🔧 {base.ConfigTypeName}";
    }
}
