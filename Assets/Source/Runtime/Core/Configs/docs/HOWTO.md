# Configs: HOWTO — Как добавить новый конфиг

## Шаг 1: Создай класс

```csharp
using Core.Abstractions.Config;
using TriInspector;
using UnityEngine;

namespace Core.Configs.Services
{
    [CreateAssetMenu(fileName = "MyFeatureConfig", menuName = "EndlessAdventure/My Feature Config")]
    public class MyFeatureConfig : ServiceConfigBase
    {
        #region Settings

        [Title("⚙️ Main Settings")]
        [Range(1, 100)]
        [SerializeField] private int _maxItems = 10;

        [Required]
        [SerializeField] private GameObject _prefab;

        #endregion

        #region Properties

        public int MaxItems => _maxItems;
        public GameObject Prefab => _prefab;

        #endregion

        #region Validation

        protected override void ValidateConfig()
        {
            base.ValidateConfig();

            if (_prefab == null)
                Debug.LogWarning($"[{nameof(MyFeatureConfig)}] Prefab is not assigned!");
        }

        #endregion
    }
}
```

**Выбор базового класса:**
- `ConfigBase` — для данных (ColorPalette, TypographySettings)
- `ServiceConfigBase` — для настроек сервисов (Network, Audio, ProceduralUI)

## Шаг 2: Создай ассет

1. Unity → `Assets → Create → EndlessAdventure → My Feature Config`
2. Положи в `Assets/Source/00. Configs/`
3. Заполни поля в Inspector

## Шаг 3: Подключи в DI

В `InfrastructureBindings.cs`:
```csharp
builder.AddSingleton<IMyFeatureService>(container =>
{
    var configProvider = container.Resolve<IConfigProvider>();
    var config = configProvider.GetConfig<MyFeatureConfig>();
    
    if (config == null)
    {
        Debug.LogWarning("[DI] MyFeatureConfig not found");
        return null;
    }
    
    return new MyFeatureService(config);
});
```

Или expose конфиг напрямую:
```csharp
builder.AddSingleton(container =>
{
    var configProvider = container.Resolve<IConfigProvider>();
    return configProvider.GetConfig<MyFeatureConfig>()
        ?? throw new System.Exception("MyFeatureConfig not found");
});
```

## Шаг 4: Для Design System конфигов

Если конфиг — часть дизайн-системы, добавь ссылку в `DesignSystemSettings`:
```csharp
[Title("🎨 Design System Components")]
[Required]
public MyDesignConfig myDesign;
```

И запроси через Mediator:
```csharp
var config = await _mediator.Send(new GetConfigQuery<DesignSystemSettings>());
```

---

*Обновлено: 2026-02-11*
