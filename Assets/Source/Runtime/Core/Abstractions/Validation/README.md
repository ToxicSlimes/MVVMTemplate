# Validation Abstractions

Атрибуты для валидации в Unity Inspector.

## Типы

| Тип | Описание |
|-----|----------|
| `ValidateMissingRefsAttribute` | Валидация отсутствующих ссылок на поле |

## Использование

```csharp
using Core.Abstractions.Validation;
using UnityEngine;

public class MyComponent : MonoBehaviour
{
    [ValidateMissingRefs("Player reference is required!")]
    [SerializeField] private Transform _player;
    
    [ValidateMissingRefs(showWarningIcon: false)]
    [SerializeField] private AudioClip _optionalSound;
}
```

## Параметры ValidateMissingRefsAttribute

| Параметр | Тип | По умолчанию | Описание |
|----------|-----|--------------|----------|
| `errorMessage` | string | "Missing reference detected!" | Сообщение об ошибке |
| `showWarningIcon` | bool | true | Показывать иконку предупреждения |

## Реализация

Drawer находится в `Editor/` папке проекта.
