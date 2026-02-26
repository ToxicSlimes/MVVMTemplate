# NetworkSandboxView

## Назначение

Тестовый экран для работы с Workflow API. Позволяет загружать, просматривать и визуализировать workflows.

## Файлы

| Тип | Путь |
|-----|------|
| View | `Views/NetworkSandbox/NetworkSandboxView.cs` |
| ViewModel | `ViewModels/NetworkSandbox/NetworkSandboxViewModel.cs` |
| Model | `INetworkSandboxModel` (Abstractions) |

---

## ViewModel Properties

| Property | Тип | Описание |
|----------|-----|----------|
| `WorkflowId` | `ReactiveProperty<string>` | ID workflow для загрузки |
| `ResponseText` | `ReactiveProperty<string>` | Текстовый ответ API |
| `StatusText` | `ReactiveProperty<string>` | Статус операции |
| `IsLoading` | `ReactiveProperty<bool>` | Индикатор загрузки |
| `CurrentWorkflow` | `ReactiveProperty<WorkflowModel>` | Загруженный workflow |

---

## ViewModel Commands

| Command | Параметр | Описание |
|---------|----------|----------|
| `GetWorkflowCommand` | `Unit` | Загрузить workflow по ID |
| `GetWorkflowsCommand` | `Unit` | Получить список workflows (public) |
| `GetMyWorkflowsCommand` | `Unit` | Получить мои workflows |
| `GetEntrypointsCommand` | `Unit` | Получить все entrypoints |
| `GetWorkflowEntrypointsCommand` | `Unit` | Получить entrypoints workflow |
| `GetHelperCommand` | `Unit` | Получить workflow helper (node types) |
| `ClearResponseCommand` | `Unit` | Очистить response |
| `OpenPerformanceTestCommand` | `Unit` | Открыть PerformanceTestView |

---

## UI Элементы

### Input

| Поле | Тип | Описание |
|------|-----|----------|
| `_workflowIdInput` | `TMP_InputField` | Поле для Workflow ID |

### Buttons

| Поле | Тип | Описание |
|------|-----|----------|
| `_getWorkflowButton` | `Button` | Get Workflow |
| `_getWorkflowsButton` | `Button` | Get Workflows (list) |
| `_getMyWorkflowsButton` | `Button` | Get My Workflows |
| `_getEntrypointsButton` | `Button` | Get Entrypoints |
| `_getWorkflowEntrypointsButton` | `Button` | Get Workflow Entrypoints |
| `_getHelperButton` | `Button` | Get Helper |
| `_clearButton` | `Button` | Clear Response |
| `_openPerformanceTestButton` | `Button` | Open Performance Test |

### Output

| Поле | Тип | Описание |
|------|-----|----------|
| `_responseText` | `TMP_Text` | Текст ответа |
| `_statusText` | `TMP_Text` | Статус |
| `_loadingIndicator` | `GameObject` | Spinner |

### Graph Visualization

| Поле | Тип | Описание |
|------|-----|----------|
| `_graphRenderer` | `WorkflowGraphRenderer` | Рендерер графа |
| `_graphContainer` | `GameObject` | Контейнер графа |

---

## Биндинги

```csharp
// Workflow ID input (two-way)
_workflowIdInput.OnValueChangedAsObservable()
    .Subscribe(this, (value, self) => self.ViewModel.WorkflowId.Value = value)
    .AddTo(ref disposables);

// Button interactable during loading
ViewModel.IsLoading
    .Select(x => !x)
    .Subscribe(this, (canExecute, self) => 
        self._getWorkflowButton.interactable = canExecute)
    .AddTo(ref disposables);

// Graph rendering on workflow load
ViewModel.CurrentWorkflow
    .Subscribe(this, (workflow, self) => self.RenderWorkflowGraph(workflow))
    .AddTo(ref disposables);
```

---

## Связанные сервисы

| Сервис | Использование |
|--------|---------------|
| `IWorkflowService` | Все операции с Workflow API |
| `IWorkflowNodeDefinitionService` | Определения нодов |
| `IWorkflowNodeMapper` | Маппинг JSON → RuntimeNodes |
| `IViewService` | Открытие PerformanceTestView |
| `ILogger` | Логирование |

---

## Graph Rendering

При загрузке workflow с `ContextSchema.nodes`:

1. `WorkflowNodeMapper.ToRuntimeNodes(nodesArray)` — парсинг нодов
2. `WorkflowGraphModel.BuildFromNodes(runtimeNodes)` — построение графа
3. `WorkflowGraphRenderer.RenderGraph(graphModel)` — визуализация

---

## Схема UI

```
┌──────────────────────────────────────────────────────────────┐
│ Network Sandbox                                               │
├──────────────────────────────────────────────────────────────┤
│ Workflow ID: [________________________________]               │
├──────────────────────────────────────────────────────────────┤
│ [Get Workflow] [Get Workflows] [Get My Workflows]            │
│ [Get Entrypoints] [Get WF Entrypoints] [Get Helper]          │
│ [Clear] [Open Performance Test]                               │
├──────────────────────────────────────────────────────────────┤
│ Status: [OK] Loaded workflow 'My Workflow'                    │
├──────────────────────────────────────────────────────────────┤
│ ┌──────────────────────────────────────────────────────────┐ │
│ │ Response:                                                 │ │
│ │ Workflow: My Workflow (ID: abc-123)                       │ │
│ │ Visibility: public, Version: 1                            │ │
│ │ NodeCount: 5, ContextId: ctx-456                         │ │
│ │                                                           │ │
│ │ Node graph:                                               │ │
│ │ start [entrypoint]                                        │ │
│ │ check [condition] : then -> action; else -> end;          │ │
│ │ action [http] : next -> end;                              │ │
│ │ end [exit]                                                │ │
│ └──────────────────────────────────────────────────────────┘ │
├──────────────────────────────────────────────────────────────┤
│ ┌──────────────────────────────────────────────────────────┐ │
│ │ [Graph Visualization]                                     │ │
│ │  ┌───────┐    ┌───────┐    ┌───────┐                     │ │
│ │  │ Start │───→│ Check │───→│ Action│───→[End]            │ │
│ │  └───────┘    └───────┘    └───────┘                     │ │
│ └──────────────────────────────────────────────────────────┘ │
└──────────────────────────────────────────────────────────────┘
```

---

## Вспомогательные компоненты

| Компонент | Описание |
|-----------|----------|
| `WorkflowGraphRenderer` | Рендерер графа нодов |
| `WorkflowGraphEdgeView` | Отображение связей |
| `UILineRenderer` | Линии между нодами |
| `WorkflowNodeView` | Отдельный нод |

---

## Особенности

- Парсинг `ContextSchema.nodes` как JArray
- Лимит 100 нодов в текстовом представлении графа
- Все кнопки disabled во время loading
- Dependency Injection через `[Inject]`
