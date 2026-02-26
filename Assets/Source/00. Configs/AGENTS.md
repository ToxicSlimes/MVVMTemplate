# AGENTS.md - Configs

## 📋 ScriptableObject Assets

Конфигурационные ScriptableObject assets для ViewMappings.

## 📂 Structure

```
00. Configs/
└── ViewsMapping/               # ViewMapping assets
    ├── SettingsViewMapping.asset
    ├── ChatViewMapping.asset
    ├── NetworkSandboxViewMapping.asset
    └── {FeatureName}ViewMapping.asset
```

## 🔧 ViewMapping Asset

Каждый ViewMapping asset содержит:
- **ViewModel Type** — тип ViewModel (автоматически)
- **View Prefab** — ссылка на Addressable префаб View

## 📝 Creating ViewMapping

1. **Генерация кода:**
   - Tools → MVVM → View Mapping Generator
   - Выбери ViewModel → Generate

2. **Создание asset:**
   - После компиляции asset создастся автоматически
   - Или: Create → ViewMapping → {FeatureName}

3. **Назначение префаба:**
   - Открой созданный asset
   - Перетащи View префаб в поле `ViewPrefab`
   - Убедись что префаб — **Addressable**

## ⚠️ Rules

1. **Имена соответствуют** — `{Name}ViewMapping.asset` → `{Name}View` prefab
2. **Префаб = Addressable** — иначе ViewFactory не загрузит
3. **Не дублируй** — один asset на ViewModel
4. **ViewConfig** — все mappings должны быть в `Resources/ViewConfig.asset`

## 🔗 Related

- **Код маппингов:** `CodeGen/Views/`
- **Generator:** `Editor/ViewMappingGenerator/`
- **ViewConfig:** `Resources/ViewConfig.asset`
- **Префабы:** `Prefabs/Views/`
