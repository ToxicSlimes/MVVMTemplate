# TESTING GUIDE — Abstractions

Как тестировать код, зависящий от интерфейсов из Abstractions. Mock patterns с NSubstitute.

---

## Принцип

Abstractions = интерфейсы → идеально для мокирования. Тестируем **потребителей** интерфейсов, а не сами интерфейсы.

---

## Настройка

```
NuGet: NSubstitute 5.x
NuGet: NSubstitute.Analyzers.CSharp (опционально, для предупреждений)
Unity: com.unity.test-framework
```

---

## Паттерн 1: Mock сервиса

### IChatService

```csharp
[Test]
public async Task ChatViewModel_LoadHistory_SetsMessages()
{
    // Arrange
    var chatService = Substitute.For<IChatService>();
    var fakeChat = new ChatModel
    {
        Id = "chat-1",
        Messages = new List<ChatMessage>
        {
            new() { Role = "user", Content = "Hello" },
            new() { Role = "assistant", Content = "Hi!" }
        }
    };
    
    chatService.GetChatHistoryAsync("chat-1", Arg.Any<CancellationToken>())
        .Returns(UniTask.FromResult(fakeChat));
    
    var model = Substitute.For<IChatModel>();
    var vm = new ChatViewModel(chatService, model);
    
    // Act
    await vm.LoadChatAsync("chat-1");
    
    // Assert
    model.Received(1).ChatMessages = Arg.Is<List<ChatMessage>>(
        msgs => msgs.Count == 2);
}
```

### IAuthService

```csharp
[Test]
public async Task LoginViewModel_SignIn_Success()
{
    var authService = Substitute.For<IAuthService>();
    authService.SignInEmailAsync("test@test.com", "pass123", true, Arg.Any<CancellationToken>())
        .Returns(UniTask.FromResult(true));
    
    var model = Substitute.For<ILoginModel>();
    var vm = new LoginViewModel(authService, model);
    
    await vm.SignInAsync("test@test.com", "pass123", rememberMe: true);
    
    model.Received(1).IsAuthorized = true;
    model.Received(1).IsLoading = false;
}
```

---

## Паттерн 2: Mock Model (event-based)

Для моделей с `event Action<T>`:

```csharp
[Test]
public void View_ReactsToModelChanges()
{
    var model = Substitute.For<ILoginModel>();
    
    // Поднять событие:
    model.IsLoadingChanged += Raise.Event<Action<bool>>(true);
    
    // Или через property setter verification:
    model.IsLoading = true;
    model.Received(1).IsLoading = true;
}
```

---

## Паттерн 3: Mock IInputService (Observable)

```csharp
[Test]
public void PlayerController_OnAttack_Fires()
{
    var inputService = Substitute.For<IInputService>();
    var attackSubject = new Subject<Unit>();
    
    inputService.OnAttack.Returns(attackSubject);
    
    var controller = new PlayerController(inputService);
    bool attacked = false;
    controller.OnAttackPerformed += () => attacked = true;
    
    // Act
    attackSubject.OnNext(Unit.Default);
    
    // Assert
    Assert.IsTrue(attacked);
}
```

---

## Паттерн 4: Mock IConfigProvider

```csharp
[Test]
public async Task Service_LoadsConfig()
{
    var configProvider = Substitute.For<IConfigProvider>();
    var fakeConfig = ScriptableObject.CreateInstance<MyServiceConfig>();
    
    configProvider.GetConfig<MyServiceConfig>().Returns(fakeConfig);
    configProvider.IsConfigLoaded<MyServiceConfig>().Returns(true);
    
    var service = new MyService(configProvider);
    
    // Config доступен синхронно
    Assert.IsNotNull(service.Config);
}
```

---

## Паттерн 5: Mock IViewService

```csharp
[Test]
public async Task Navigator_ShowsChat()
{
    var viewService = Substitute.For<IViewService>();
    var fakeView = Substitute.For<IView>();
    
    viewService.ShowAsync<ChatViewModel>(Arg.Any<ViewShowParams>())
        .Returns(UniTask.FromResult<IView>(fakeView));
    
    var navigator = new ScreenNavigator(viewService);
    await navigator.NavigateToChat();
    
    await viewService.Received(1).ShowAsync<ChatViewModel>(
        Arg.Any<ViewShowParams>());
}
```

---

## Паттерн 6: Mock IAssetProvider

```csharp
[Test]
public async Task Factory_InstantiatesPrefab()
{
    var assetProvider = Substitute.For<IAssetProvider>();
    var fakePrefab = new GameObject("FakeElement");
    
    assetProvider.InstantiateAsync<MyElement>("address", Arg.Any<Transform>())
        .Returns(UniTask.FromResult(fakePrefab.AddComponent<MyElement>()));
    
    var factory = new ElementFactory(assetProvider);
    var element = await factory.CreateAsync<MyElement>("address");
    
    Assert.IsNotNull(element);
}
```

---

## Паттерн 7: Verify CancellationToken передаётся

```csharp
[Test]
public async Task ViewModel_PassesCancellationToken()
{
    var service = Substitute.For<IChatService>();
    service.CreateChatAsync(Arg.Any<CreateChatRequestDto>(), Arg.Any<CancellationToken>())
        .Returns(UniTask.FromResult(new ChatModel()));
    
    var vm = new ChatViewModel(service, Substitute.For<IChatModel>());
    await vm.CreateChatAsync();
    
    // Verify token был не default:
    await service.Received(1).CreateChatAsync(
        Arg.Any<CreateChatRequestDto>(),
        Arg.Is<CancellationToken>(ct => ct != default));
}
```

---

## Паттерн 8: Mock ProceduralUI Builders

```csharp
[Test]
public void Builder_CreatesButton()
{
    var builder = Substitute.For<IProceduralUiBuilder>();
    var buttonBuilder = Substitute.For<IButtonBuilder>();
    
    builder.Button("Click me").Returns(buttonBuilder);
    buttonBuilder.Variant(UiButtonVariant.Primary).Returns(buttonBuilder);
    buttonBuilder.Build().Returns(new UiComponentModel());
    
    var result = builder.Button("Click me")
        .Variant(UiButtonVariant.Primary)
        .Build();
    
    Assert.IsNotNull(result);
}
```

---

## Чеклист тестирования

- [ ] Мокай **интерфейсы**, не конкретные классы
- [ ] Всегда проверяй `CancellationToken` передачу
- [ ] Для `UniTask` возвратов используй `UniTask.FromResult()`
- [ ] Для Observable — используй `Subject<T>` для эмуляции событий
- [ ] Для event Action<T> — `Raise.Event<Action<T>>(value)`
- [ ] ScriptableObject конфиги создавай через `ScriptableObject.CreateInstance<T>()`
- [ ] Dispose мокнутые объекты в TearDown

---

*Обновлено: 2026-02-11*
