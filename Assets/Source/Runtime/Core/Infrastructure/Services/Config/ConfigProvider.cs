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
        private readonly IAssetProvider _assetProvider;
        private readonly ILogger _logger;
        private readonly Dictionary<string, ConfigBase> _loadedConfigs = new();
        private static readonly Dictionary<string, string> LabelByConfigName = new()
        {
            { "AudioConfig", "Audio" }
        };

        public ConfigProvider(IAssetProvider assetProvider, ILogger logger)
        {
            _assetProvider = assetProvider;
            _logger = logger;
            
            PreloadAllConfigsAsync().Forget();
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
            
            var config = LoadConfig<T>(configName);
            if (config == null)
            {
                _logger.Error($"[ConfigProvider] Failed to load config '{configName}' synchronously");
                return null;
            }

            _loadedConfigs[cacheKey] = config;
            _logger.Info($"[ConfigProvider] Config '{configName}' loaded synchronously");
            return config;
        }

        public bool IsConfigLoaded<T>() where T : ConfigBase
        {
            var cacheKey = GetCacheKey<T>(typeof(T).Name);
            return _loadedConfigs.ContainsKey(cacheKey);
        }

        public async UniTask PreloadAllConfigsAsync()
        {
            _logger.Info("[ConfigProvider] Preloading all configs...");

            var configNames = new[]
            {
                "NetworkConfig",
                "ProceduralUiConfig",
                "DesignSystemSettings",
                "ColorPalette",
                "TypographySettings",
                "SpacingSettings",
                "ViewConfig",
                "AudioConfig",
                "ElementConfig"
            };

            var tasks = Enumerable.Select(configNames, PreloadConfigInternalAsync).ToList();

            await UniTask.WhenAll(tasks);

            _logger.Info($"[ConfigProvider] Preloaded {_loadedConfigs.Count} configs");
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
            _logger.Info($"[ConfigProvider] Loading config '{configName}' from Addressables...");
            T config = null;

            if (!LabelByConfigName.ContainsKey(configName))
            {
                try
                {
                    config = await _assetProvider.LoadAsync<T>(configName);
                }
                catch (Exception ex)
                {
                    _logger.Error($"[ConfigProvider] Direct async load failed for '{configName}': {ex.GetType().Name}");
                }

                if (config != null)
                {
                    return config;
                }
            }

            if (LabelByConfigName.TryGetValue(configName, out var label))
            {
                _logger.Info($"[ConfigProvider] Fallback loading config '{configName}' by label '{label}'");
                var configs = await _assetProvider.LoadAllAsync<ConfigBase>(label);
                var typedConfig = configs.OfType<T>().FirstOrDefault();
                if (typedConfig != null)
                {
                    return typedConfig;
                }
            }

            return null;
        }

        private T LoadConfig<T>(string configName) where T : ConfigBase
        {
            _logger.Info($"[ConfigProvider] Loading config '{configName}' synchronously from Addressables...");
            T config = null;

            if (!LabelByConfigName.ContainsKey(configName))
            {
                try
                {
                    config = _assetProvider.LoadSync<T>(configName);
                }
                catch (Exception ex)
                {
                    _logger.Error($"[ConfigProvider] Direct sync load failed for '{configName}': {ex.GetType().Name}");
                }

                if (config != null)
                {
                    return config;
                }
            }

            if (LabelByConfigName.TryGetValue(configName, out var label))
            {
                _logger.Info($"[ConfigProvider] Fallback loading config '{configName}' synchronously by label '{label}'");
                if (_assetProvider is Core.Infrastructure.AssetManagement.AssetProvider concrete)
                {
                    var configs = concrete.LoadAllSync<ConfigBase>(label);
                    var typedConfig = configs.OfType<T>().FirstOrDefault();
                    if (typedConfig != null)
                    {
                        return typedConfig;
                    }
                }
                else
                {
                    _logger.Warn($"[ConfigProvider] Sync label load skipped for '{configName}' because provider is not AssetProvider");
                }
            }

            return null;
        }
    }
}
