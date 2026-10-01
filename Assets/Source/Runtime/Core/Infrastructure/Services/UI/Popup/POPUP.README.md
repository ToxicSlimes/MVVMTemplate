# 🎈 Popup System Documentation

Легковесная система попапов и тултипов для Unity UI.

---

## 📋 Overview

**Popup System** — отдельная от MVVM система для маленьких hover-попапов и тултипов.

**Ключевые особенности:**
- **Пул** — переиспользование попапов (ObjectPool)
- **LitMotion Animations** — zero-allocation анимации через [LitMotion](https://github.com/AnnulusGames/LitMotion)
- **Вложенность** — попап может показать другой попап
- **Screen Clamping** — автоматическое позиционирование в пределах экрана
- **Generic Data** — типизированные данные без boxing

---

## 📦 Dependencies

### LitMotion (Required)
High-performance tween library. Install via Package Manager:
```
https://github.com/AnnulusGames/LitMotion.git?path=src/LitMotion/Assets/LitMotion
```

**Why LitMotion:**
- Zero allocation (Burst-optimized)
- 2-20x faster than DOTween
- Native R3/UniTask support
- Punch, Shake, Sequences

---

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────────────────────┐
│                          POPUP SYSTEM                                    │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │                    IPopupService                                 │   │
│  │  - Show<TPopup, TData>(target, data, params)                    │   │
│  │  - Hide(popup) / HideAll() / HideTopPopup()                     │   │
│  │  - RegisterPrefab<TPopup>(prefab)                               │   │
│  │  - OnCancel → HideTopPopup (via IInputService)                  │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                              │                                          │
│  ┌───────────────────────────┼──────────────────────────────────────┐  │
│  │                           ▼                                       │  │
│  │  ┌─────────────────────────────────────────────────────────────┐ │  │
│  │  │              PopupBase<TData> : MonoBehaviour               │ │  │
│  │  │  - SetData(TData data)                                      │ │  │
│  │  │  - SetTarget(RectTransform target)                          │ │  │
│  │  │  - Show() / Hide()                                           │ │  │
│  │  │  - Animate() → PopupAnimator (LitMotion wrapper)            │ │  │
│  │  └─────────────────────────────────────────────────────────────┘ │  │
│  │                                                                   │  │
│  │  ┌─────────────────────────────────────────────────────────────┐ │  │
│  │  │              PopupAnimator (LitMotion Adapter)               │ │  │
│  │  │  - FadeIn/FadeOut(duration, ease)                           │ │  │
│  │  │  - Scale(from, to, duration, ease)                          │ │  │
│  │  │  - Punch/Shake(strength, duration)                          │ │  │
│  │  │  - Play() → UniTask                                          │ │  │
│  │  └─────────────────────────────────────────────────────────────┘ │  │
│  │                                                                   │  │
│  │  ┌─────────────────────────────────────────────────────────────┐ │  │
│  │  │              ScreenClampUtility (static)                     │ │  │
│  │  │  - ClampToScreen(popup, canvas, padding)                    │ │  │
│  │  │  - CalculatePosition(target, popupSize, position)           │ │  │
│  │  │  - Auto-flip if doesn't fit                                  │ │  │
│  │  └─────────────────────────────────────────────────────────────┘ │  │
│  │                                                                   │  │
│  └──────────────────────────────────────────────────────────────────┘  │
│                                                                         │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │              PopupTrigger : MonoBehaviour                        │   │
│  │  - TriggerType: Hover / Click / Manual                          │   │
│  │  - Position, Offset, ShowDelay, HideDelay                       │   │
│  │  - IPointerEnter/Exit/ClickHandler, ITooltipTarget              │   │
│  │  - SetDataProvider<T>() / SetTooltipData(title, desc)           │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 📁 File Structure

```
Assets/Source/Runtime/Core/
├── Abstractions/Popup/
│   ├── IPopupService.cs        # Интерфейс сервиса
│   ├── IPopup.cs               # Интерфейс попапа
│   ├── IPopupPoolFactory.cs    # Фабрика пулов
│   ├── PopupPosition.cs        # Enum позиций
│   ├── PopupShowParams.cs      # Параметры показа
│   ├── PopupTrigger.cs         # Триггер показа (hover/click)
│   └── PopupTriggerType.cs     # Enum типов триггера
│
├── Infrastructure/Popup/
│   ├── POPUP.README.md         # Эта документация
│   ├── PopupService.cs         # Реализация сервиса
│   ├── PopupBase.cs            # Базовый класс попапа
│   ├── PopupAnimator.cs        # LitMotion adapter
│   ├── PopupPoolFactory.cs     # Реализация IPopupPoolFactory
│   ├── PopupPool.cs            # Пул попапов
│   └── ScreenClampUtility.cs   # Утилита позиционирования
│
└── MVVM/Popups/                # Конкретные реализации
    └── Tooltip/
        ├── TooltipPopup.cs
        └── TooltipData.cs
```

---

## 🔧 API Reference

### PopupAnimator (LitMotion Adapter)

```csharp
// Fluent API wrapping LitMotion
popup.Animate()
    .FadeIn(0.15f, Ease.OutQuad)           // CanvasGroup.alpha 0→1
    .Scale(0.9f, 1f, 0.15f, Ease.OutBack)  // localScale animation
    .Punch(Vector3.up * 10f, 0.3f)          // LitMotion.Punch
    .Shake(Vector3.one * 5f, 0.2f)          // LitMotion.Shake
    .Delay(0.1f)                            // Add delay
    .OnComplete(() => Debug.Log("Done!"))
    .Play();                                // Returns UniTask
```

### PopupBase

```csharp
public abstract class PopupBase<TData> : PopupBase
{
    public TData Data { get; }
    
    protected abstract void OnDataSet(TData data);
    protected virtual void OnDataCleared() { }
    
    // Override for custom animations
    protected virtual PopupAnimator GetShowAnimation()
    {
        return Animate()
            .FadeIn(0.15f)
            .Scale(0.9f, 1f, 0.15f, Ease.OutBack);
    }
}
```

---

## 📖 Usage Examples

### Basic Tooltip

```csharp
// 1. Create tooltip data (implements IPopupData)
public class TooltipData : IPopupData
{
    public string Title { get; }
    public string Description { get; }
    public TooltipData(string title, string description = null);
}

// 2. Create tooltip popup
public class TooltipPopup : PopupBase<TooltipData>
{
    [SerializeField] private TMP_Text _titleText;
    
    protected override void OnDataSet(TooltipData data)
    {
        _titleText.text = data.Title;
    }
    
    // Custom LitMotion animation
    protected override PopupAnimator GetShowAnimation()
    {
        return Animate()
            .FadeIn(0.2f, Ease.OutQuad)
            .Scale(0.85f, 1f, 0.2f, Ease.OutBack);
    }
}

// 3. Register and show
_popupService.RegisterPrefab<TooltipPopup>(tooltipPrefab);

var data = new TooltipData("Sword of Fire", "A blazing blade");
await _popupService.ShowAsync<TooltipPopup, TooltipData>(
    targetButton.GetComponent<RectTransform>(),
    data,
    PopupShowParams.Top(10f)
);
```

### Via PopupTrigger (Inspector)

```csharp
// Add PopupTrigger component to any UI element
// Configure in Inspector:
// - Popup Prefab: TooltipPrefab
// - Trigger Type: Hover
// - Position: Top
// - Show Delay: 0.3s
```

### Nested Popups

```csharp
public class ItemInfoPopup : PopupBase<ItemData>
{
    [SerializeField] private PopupTrigger _statsTrigger;  // PopupTrigger inside!
    
    protected override void OnDataSet(ItemData data)
    {
        _statsTrigger.SetDataProvider(() => data.Stats);
    }
}
```

---

## ⚠️ Best Practices

### DO ✅

```csharp
// Use IPopupData for typed data
public class TooltipData : IPopupData { ... }

// Use LitMotion Ease enum
.Scale(0.9f, 1f, 0.15f, Ease.OutBack)

// Clear data in OnReturnToPool
public override void OnReturnToPool()
{
    _titleText.text = "";
}
```

### DON'T ❌

```csharp
// Don't create popups via Instantiate
var popup = Instantiate(prefab);  // ❌ Use PopupService!

// Don't forget IPopupData interface
public class TooltipData { ... }  // ❌ Must implement IPopupData!

// Don't manually position without clamping
popup.position = target.position;  // ❌ May go off-screen!
```

---

## 🔧 Installation

1. **Install LitMotion:**
   ```
   Window → Package Manager → + → Add package from git URL:
   https://github.com/AnnulusGames/LitMotion.git?path=src/LitMotion/Assets/LitMotion
   ```

2. **Register PopupService in DI** (already done in InfrastructureBindings)

3. **Create Overlay Canvas** and register with CanvasProvider

4. **Create popup prefabs** with PopupBase<T> component
