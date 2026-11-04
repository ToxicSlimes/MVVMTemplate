using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Services;
using Core.Abstractions.AssetManagement;
using Core.Abstractions.Config;
using Cysharp.Threading.Tasks;

namespace Core.Infrastructure.Services.Config
{
  public class ConfigProvider : IConfigProvider
  {
    private const string GlobalConfigLabel = ConfigPreloader.Label;
    
    private readonly IAssetProvider _assetProvider;
    private readonly ILogger _logger;
    private readonly Dictionary<string, ConfigBase> _loadedConfigs = new();

    public ConfigProvider(IAssetProvider assetProvider, ILogger logger)
    {
      _assetProvider = assetProvider;
      _logger = logger;

      if (ConfigPreloader.IsReady)
      {
        AdoptPreloaded();
      }
    }

    public async UniTask<T> GetConfigAsync<T>() where T : ConfigBase
    {
      var configName = typeof(T).Name;
      var cacheKey = GetCacheKey<T>(configName);

      if (_loadedConfigs.TryGetValue(cacheKey, out var cachedConfig))
      {
        _logger.Info($"[ConfigProvider] Config '{configName}' loaded from cache");
        return cachedConfig as T;
      }

      var config = await LoadConfigAsync<T>(configName);

      if (config == null)
      {
        _logger.Error($"[ConfigProvider] Failed to load config '{configName}'");
        return null;
      }

      _loadedConfigs[cacheKey] = config;
      _logger.Info($"[ConfigProvider] Config '{configName}' loaded successfully");

      return config;
    }

    public T GetConfig<T>() where T : ConfigBase
    {
      var configName = typeof(T).Name;
      var cacheKey = GetCacheKey<T>(configName);

      if (_loadedConfigs.TryGetValue(cacheKey, out var cachedConfig))
      {
        return cachedConfig as T;
      }

      if (ConfigPreloader.IsReady)
      {
        AdoptPreloaded();
        if (_loadedConfigs.TryGetValue(cacheKey, out cachedConfig))
        {
          return cachedConfig as T;
        }
      }

      throw new InvalidOperationException(ConfigPreloader.IsReady
        ? $"Config '{configName}' is not in the preloaded '{GlobalConfigLabel}' set"
        : $"Config '{configName}' requested before configs were preloaded");
    }

    public bool IsConfigLoaded<T>() where T : ConfigBase
    {
      var cacheKey = GetCacheKey<T>(typeof(T).Name);
      if (!_loadedConfigs.ContainsKey(cacheKey) && ConfigPreloader.IsReady)
      {
        AdoptPreloaded();
      }

      return _loadedConfigs.ContainsKey(cacheKey);
    }

    public async UniTask PreloadAllConfigsAsync()
    {
      _logger.Info("[ConfigProvider] Preloading all configs...");

      await ConfigPreloader.PreloadAsync();
      AdoptPreloaded();

      _logger.Info($"[ConfigProvider] Preloaded {_loadedConfigs.Count} configs");
    }

    private void AdoptPreloaded()
    {
      foreach (var config in ConfigPreloader.Configs)
      {
        var type = config.GetType();
        _loadedConfigs[$"{type.Name}_{type.Name}"] = config;
      }
    }

    public void ReleaseConfig<T>() where T : ConfigBase
    {
      var configName = typeof(T).Name;
      var cacheKey = GetCacheKey<T>(configName);

      if (!_loadedConfigs.TryGetValue(cacheKey, out var config))
        return;

      _assetProvider.Release(config);
      _loadedConfigs.Remove(cacheKey);
      _logger.Info($"[ConfigProvider] Config '{configName}' released");
    }

    private async UniTask PreloadConfigInternalAsync(string configName)
    {
      try
      {
        var config = await LoadConfigAsync<ConfigBase>(configName);
        if (config != null)
        {
          var cacheKey = $"{config.GetType().Name}_{configName}";
          _loadedConfigs[cacheKey] = config;
        }
      }
      catch (System.Exception ex)
      {
        _logger.Error($"[ConfigProvider] Failed to preload config '{configName}': {ex.Message}");
      }
    }

    private static string GetCacheKey<T>(string configName) where T : ConfigBase
    {
      return $"{typeof(T).Name}_{configName}";
    }

    private async UniTask<T> LoadConfigAsync<T>(string configName) where T : ConfigBase
    {
      _logger.Info($"[ConfigProvider] Loading config '{configName}' from Addressables by global label '{GlobalConfigLabel}'...");

      try
      {
        var configs = await _assetProvider.LoadAllAsync<ConfigBase>(GlobalConfigLabel);
        var typedConfig = configs.OfType<T>().FirstOrDefault();
        if (typedConfig != null)
        {
          return typedConfig;
        }
      }
      catch (Exception ex)
      {
        _logger.Error($"[ConfigProvider] Async load failed for '{configName}' by label '{GlobalConfigLabel}': {ex.GetType().Name}");
      }

      return null;
    }
  }
}