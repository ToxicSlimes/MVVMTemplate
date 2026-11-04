using Core.Abstractions.MVVM.Models.Inventory;
using Core.Abstractions.MVVM.Models.Lobby;
using Core.Application.Handlers.Lobby;
using Core.Application.Handlers.Player;
using Core.Network.Abstractions.Services;
using Core.Abstractions.Factories;
using Core.Abstractions.StateMachines.States;
using System.Collections.Generic;
using Core.Abstractions.Services;
using Core.Abstractions.AssetManagement;
using Core.Abstractions.Config;
using Core.DependencyInjection.Bindings;
using Core.DependencyInjection;
using Core.DependencyInjection.Extensions;
using Core.Abstractions.MVVM.Animation;
using Core.Abstractions.MVVM.Canvas;
using Core.Abstractions.MVVM.Config;
using Core.Abstractions.MVVM.Factory;
using Core.Abstractions.MVVM.Services;
using Core.Abstractions.Services.Ads;
using Core.Abstractions.Services.Platform;
using Core.Infrastructure.Configs.Services;
using Core.Infrastructure.AssetManagement;
using Core.Infrastructure.Services.Ads;
using Core.Infrastructure.Services.Audio;
using Core.Infrastructure.Services.Localization;
using Core.Infrastructure.Services.Network;
using Core.Presentation.MVVM;
using Core.Presentation.MVVM.Animation;
using Core.Presentation.MVVM.Canvas;
using Core.Presentation.MVVM.Config;
using Core.Presentation.MVVM.Services;
using Core.Infrastructure.Services.Logging;
using Core.Presentation.MVVM.Models.Inventory;
using Core.Infrastructure.Services.Platform;
using Core.Infrastructure.Services.Config;
using Core.Infrastructure.Services.Cache;
using Core.Domain.Models.Cache;
using Core.Abstractions.Popup;
using Core.Abstractions.Services.Input;
using Core.DependencyInjection.Generated;
using Core.Mediator.Abstractions;
using Core.Mediator.Publishing;
using Core.Mediator.Registration.Extensions;
using Core.Infrastructure.Services.Input;
using Core.Network.FusionAdapter.DependencyInjection;
using Core.Infrastructure.Popup;
using Core.Abstractions.UI;
using Core.Presentation.UI.Navigation;
using Core.Presentation.UI.DesignSystem;
using Core.Presentation.UI.DragDrop;
using Core.Presentation.UI.Services;
using Cysharp.Threading.Tasks;
using Reflex.Core;
using Services;
using Core.Abstractions.UI.DragDrop;
using Core.Presentation.MVVM.Hosts;
using UnityEngine;
using ILogger = Core.Abstractions.Services.ILogger;

public static class InfrastructureBindings
{
  public static ContainerBuilder AddCore(this ContainerBuilder builder, bool networkEnabled = true)
  {
    var contributors = CoreRegistrationContributors.Discover();

    builder.AddMorpehEcs();
    builder.AddCoreStates();
    builder.AddModels();
    builder.AddCoreServices();
    builder.AddInputServices();
    builder.AddPopupServices();
    builder.AddInfrastructure();
    builder.AddSaveServices();
    builder.AddNetwork(networkEnabled);
    builder.AddMediatorServices(contributors);
    builder.AddSingleton(container => new PauseInputPublisher(
      container.Resolve<IInputService>(),
      container.Resolve<IMediator>(),
      container.Resolve<INetworkRunnerController>()));
    builder.AddViewModels();
    CoreRegistrationContributors.ApplyBindings(builder, contributors);
    builder.AddElementServices();

    return builder;
  }

  private static ContainerBuilder AddNetwork(this ContainerBuilder builder, bool enabled)
  {
    if (!enabled)
    {
      builder.AddSingleton<INetworkRunnerController>(_ => new NullNetworkRunnerController());
      return builder;
    }

    builder.AddFusionNetwork();
    builder.AddSingleton(container => new PlayerAvatarSpawner(
      container.Resolve<INetworkRunnerController>(),
      container.Resolve<INetworkSpawner>(),
      container.Resolve<INetworkSpawnPoints>()));

    return builder;
  }

  private static ContainerBuilder AddMediatorServices(
    this ContainerBuilder builder,
    IReadOnlyList<ICoreRegistrationContributor> contributors)
  {
    builder.AddMediator(mediator =>
    {
      mediator.UseNotificationPublisher<SequentialPublisher>();
      mediator
        .RegisterViewHandlers()
        .RegisterAllHandlers()
        .RegisterElementHandlers()
        .RegisterToolkitElementHandlers();

      CoreRegistrationContributors.ApplyMediator(mediator, contributors);
    });

    return builder;
  }

  private static ContainerBuilder AddInputServices(this ContainerBuilder builder)
  {
    builder.AddSingleton<IInputService>(_ => new InputService());
    return builder;
  }

  private static ContainerBuilder AddPopupServices(this ContainerBuilder builder)
  {
    builder.AddSingleton<IPopupPoolFactory>(_ => new PopupPoolFactory());

    builder.AddSingleton<IPopupService>(container => new PopupService(
      container.Resolve<ICanvasProvider>(),
      container.Resolve<IPopupPoolFactory>(),
      container.Resolve<IInputService>()
    ));

    return builder;
  }

  private static ContainerBuilder AddModels(this ContainerBuilder builder)
  {
    builder.AddSingleton(typeof(InventoryModel), typeof(IInventoryModel));
    builder.AddSingleton(container => (ILobbyModelWriter)container.Resolve<ILobbyModel>());

    return builder;
  }

  private static ContainerBuilder AddCoreStates(this ContainerBuilder builder)
  {
    builder.AddAllImplementationsOf<IGameState>();
    return builder;
  }

  private static ContainerBuilder AddCoreServices(this ContainerBuilder builder)
  {
    builder.AddSingleton(typeof(PrefsService), typeof(IPrefsService));
    builder.AddSingleton(typeof(LoadSceneService), typeof(ILoadSceneService));
    builder.AddSingleton(typeof(DataPreloadService), typeof(IDataPreloadService));
    builder.AddSingleton<IAdsService>(_ => new NullAdsService());
    builder.AddSingleton<IClipboardService>(_ => new ClipboardService());
    builder.AddSingleton(_ => ResolveEditorOverride<IFileDialogService>(new FileDialogService(), "Core.Editor.Services.Platform.EditorFileDialogService"));
    builder.AddSingleton<IFileSystemService>(_ => new FileSystemService());
    builder.AddSingleton(_ => ResolveEditorOverride<IDialogService>(new DialogService(), "Core.Editor.Services.Platform.EditorDialogService"));
    var cacheConfig = new CacheConfig
    {
      DefaultTtl = System.TimeSpan.FromMinutes(30),
      SchemaVersion = 1,
      MaxAgeDays = 30
    };
    builder.AddSingleton<ICacheService>(container =>
      new DiskCacheService(container.Resolve<ILogger>(), cacheConfig));

    builder.AddSingleton<IAudioService>(container =>
    {
      try
      {
        var configProvider = container.Resolve<IConfigProvider>();
        var audioConfig = configProvider.GetConfig<AudioConfig>();

        if (!AudioConfigAccess.TryGetRuntimeAssets(
              audioConfig,
              out var audioMixer,
              out var soundConfig,
              out var mixerGroupConfig,
              out var audioSourcePrefab))
        {
          Debug.LogWarning("[DI] AudioConfig assets are missing or broken, using NullAudioService");
          return new NullAudioService();
        }

        var go = new GameObject("AudioService");
        Object.DontDestroyOnLoad(go);
        var service = go.AddComponent<AudioService>();

        service.Initialize(
          container.Resolve<IPrefsService>(),
          audioMixer,
          soundConfig,
          mixerGroupConfig,
          audioSourcePrefab
        );

        return service;
      }
      catch (System.Exception ex)
      {
        Debug.LogWarning($"[DI] Failed to initialize AudioService: {ex.Message}");
        return new NullAudioService();
      }
    });

    builder.AddSingleton<ILocalizationService>(container =>
    {
      try
      {
        var configProvider = container.Resolve<IConfigProvider>();
        LocalizationConfig localizationConfig = null;

        try
        {
          localizationConfig = configProvider.GetConfig<LocalizationConfig>();
        }
        catch (System.Exception)
        {
          localizationConfig = null;
        }

        return new LocalizationService(container.Resolve<IPrefsService>(), localizationConfig);
      }
      catch (System.Exception ex)
      {
        Debug.LogWarning($"[DI] Failed to initialize LocalizationService: {ex.Message}");
        return new NullLocalizationService();
      }
    });
    
    return builder;
  }

  private static T ResolveEditorOverride<T>(T fallback, string editorTypeName)
  {
#if UNITY_EDITOR
    var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
    for (var i = 0; i < assemblies.Length; i++)
    {
      var type = assemblies[i].GetType(editorTypeName, false);
      if (type == null)
      {
        continue;
      }

      if (!typeof(T).IsAssignableFrom(type))
      {
        continue;
      }

      var instance = System.Activator.CreateInstance(type);
      if (instance is T typed)
      {
        return typed;
      }
    }
#endif
    return fallback;
  }

  private static ContainerBuilder AddViewModels(this ContainerBuilder builder)
  {
    builder.RegisterViewModels();

    builder.AddSingleton<IEmbeddedPanelHost>(container => new EmbeddedPanelService(
      container,
      container.Resolve<IViewAnimator>(),
      container.Resolve<IMediator>(),
      container.Resolve<ILogger>()
    ));

    return builder;
  }

  private static ContainerBuilder AddInfrastructure(this ContainerBuilder builder)
  {
    builder.AddSingleton<ILogFormatter>(_ => new ColorLogFormatter());
    builder.AddSingleton<ILogInterceptor>(container => new UnityLogInterceptor(container.Resolve<ILogFormatter>()));
    builder.AddSingleton<ILogger>(container => new ColorLogger(container.Resolve<ILogFormatter>(), container.Resolve<ILogInterceptor>()));
    builder.AddSingleton<IAssetProvider>(_ => new AssetProvider());
    builder.AddSingleton<IConfigProvider>(c =>
    {
      var configProvider = new ConfigProvider(c.Resolve<IAssetProvider>(), c.Resolve<ILogger>());
      configProvider.PreloadAllConfigsAsync().Forget();
      return configProvider;
    });
    builder.AddSingleton<INetworkSessionOptionsProvider>(container =>
      new NetworkSessionOptionsProvider(container.Resolve<IConfigProvider>()));
    builder.AddSingleton<ICanvasProvider>(_ => new CanvasProvider());
    builder.AddSingleton<IViewAnimator>(_ => new ViewAnimator());
    builder.AddSingleton<IViewConfig>(container =>
    {
      var configProvider = container.Resolve<IConfigProvider>();
      var config = configProvider.GetConfig<ViewConfig>();
      return config == null ? throw new System.Exception("ViewConfig not found in ConfigProvider") : config;
    });
    builder.AddSingleton<IViewFactory>(container => new ViewFactory(
      container,
      container.Resolve<IAssetProvider>(),
      container.Resolve<IViewConfig>(),
      container.Resolve<ICanvasProvider>()
    ));
    builder.AddSingleton<IUIElementFactory>(container => new UIElementFactory(container));
    var dragDropService = new DragDropService();
    builder.AddSingleton<IDragDropService>(_ => dragDropService);
    builder.AddSingleton(_ => dragDropService);
    builder.AddSingleton<IUIControlService>(container =>
    {
      var configProvider = container.Resolve<IConfigProvider>();
      var designSystem = configProvider.GetConfig<DesignSystemSettings>();
      return new UIControlService(
        container.Resolve<IAudioService>(),
        container.Resolve<IMediator>(),
        designSystem);
    });
    builder.AddSingleton<IViewService>(container => new ViewService(
      container.Resolve<IViewFactory>(),
      container.Resolve<IViewAnimator>()
    ));
    builder.AddSingleton<IGameStateMachineFactory>(container =>
    {
      var states = container.Resolve<IEnumerable<IGameState>>();
      return new GameStateMachineFactory(states);
    });
    builder.AddSingleton(container => container.Resolve<IGameStateMachineFactory>().Create());
    builder.AddSingleton<IUINavigationService>(container =>
      new UINavigationService(container.Resolve<IInputService>()));

    return builder;
  }
}
