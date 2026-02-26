# Configs: DO / DON'T

## ✅ DO

### Наследование
- Наследуй от `ConfigBase` для простых конфигов
- Наследуй от `ServiceConfigBase` для сервисных конфигов (network, audio, pool)

### SerializeField + Properties
- Все поля — `[SerializeField] private` с публичным read-only property:
  ```csharp
  [SerializeField] private int _requestTimeout = 500;
  public int RequestTimeout => _requestTimeout;
  ```

### Tri-Inspector атрибуты
- `[Title("🎨 Section")]` для группировки
- `[Required]` для обязательных ссылок
- `[Range(min, max)]` для числовых ограничений
- `[Tooltip("...")]` для описания

### Валидация
- Переопредели `ValidateConfig()` для проверки значений:
  ```csharp
  protected override void ValidateConfig()
  {
      base.ValidateConfig();
      if (_defaultPoolSize > _maxPoolSize)
          Debug.LogWarning("...");
  }
  ```

### CreateAssetMenu
- Всегда добавляй `[CreateAssetMenu]` с понятным путём:
  ```csharp
  [CreateAssetMenu(fileName = "MyConfig", menuName = "EndlessAdventure/My Config")]
  ```

### Композиция
- Используй вложенные SO для сложных конфигов (как `DesignSystemSettings` → `ColorPalette` + `TypographySettings` + `SpacingSettings`)

---

## ❌ DON'T

- **НЕ** храни runtime-состояние в конфигах — они для настроек, не для данных
- **НЕ** делай поля `public` — только через `[SerializeField] private` + property
- **НЕ** хардкодь значения в коде — выноси в конфиг
- **НЕ** забывай дефолтные значения при объявлении полей
- **НЕ** создавай конфиги без `[CreateAssetMenu]` — иначе нельзя создать через Unity меню
- **НЕ** пропускай валидацию для ссылок на другие ассеты (prefabs, SO) — используй `[Required]`
- **НЕ** клади секреты (API keys) в конфиги без пометки — используй `[Tooltip]` и document sensitivity

---

*Обновлено: 2026-02-11*
