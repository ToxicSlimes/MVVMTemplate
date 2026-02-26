using Core.Abstractions.Factories;
using Core.Abstractions.StateMachines.States;
using System.Collections.Generic;
using Core.Abstractions.Services;
using Core.Abstractions.AssetManagement;
using Core.Abstractions.Config;
using Core.DependencyInjection.Bindings;
using Core.DependencyInjection.Extensions;
using Core.Abstractions.MVVM.Animation;
using Core.Abstractions.MVVM.Canvas;
using Core.Abstractions.MVVM.Config;
using Core.Abstractions.MVVM.Factory;
using Core.Abstractions.MVVM.Services;
using Core.Abstractions.Services.Platform;
using Core.Configs.Services;
using Core.Infrastructure.AssetManagement;
using Core.Infrastructure.Services.Audio;
using Core.Infrastructure.MVVM;
using Core.Infrastructure.MVVM.Animation;
using Core.Infrastructure.MVVM.Canvas;
using Core.Infrastructure.MVVM.Config;
using Core.Infrastructure.MVVM.Services;
using Core.Infrastructure.Services.Logging;
using Core.Infrastructure.Services.Platform;
using Core.Infrastructure.Services.Config;
using Core.Infrastructure.Services.Cache;
using Core.Data.Domain.Models.Cache;
using Core.Abstractions.Popup;
using Core.Abstractions.Services.Input;
using Core.DependencyInjection.Bindings;
using Core.DependencyInjection.Generated;
using Core.Infrastructure.Mediator.Abstractions;
using Core.Infrastructure.Mediator.Publishing;
using Core.Infrastructure.Mediator.Registration.Extensions;
using Core.Infrastructure.Popup;
using Core.Infrastructure.Services.Input;
using Cysharp.Threading.Tasks;
using Reflex.Core;
using Services;
using Core.MVVM.Hosts;
using UnityEngine;
using ILogger = Core.Abstractions.Services.ILogger;

public static class InfrastructureBindings
{
  public static ContainerBuilder AddCore(this ContainerBuilder builder)
  {
    builder.AddCoreStates();
    builder.AddCoreServices();
    builder.AddInputServices();
    builder.AddPopupServices();
    builder.AddInfrastructure();
    builder.AddMediatorServices();
    builder.AddViewModels();
    builder.AddElementServices();
    
    return builder;
  }

  private static ContainerBuilder AddMediatorServices(this ContainerBuilder builder)
  {
    builder.AddMediator(mediator =>
    {
      mediator.UseNotificationPublisher<SequentialPublisher>();
      mediator
        .RegisterViewHandlers()
        .RegisterAllHandlers()
        .RegisterElementHandlers();
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
      var configProvider = container.Resolve<IConfigProvider>();
      var audioConfig = configProvider.GetConfig<AudioConfig>();
      
      if (audioConfig == null)
      {
        Debug.LogWarning("[DI] AudioConfig not found, AudioService will not be initialized");
        return null;
      }
      
      var go = new GameObject("AudioService");
      Object.DontDestroyOnLoad(go);
      var service = go.AddComponent<AudioService>();
      
      service.Initialize(
        container.Resolve<IPrefsService>(),
        audioConfig.AudioMixer,
        audioConfig.SoundConfig,
        audioConfig.MixerGroupConfig,
        audioConfig.AudioSourcePrefab
      );
      
      return service;
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

    return builder;
  }
}
