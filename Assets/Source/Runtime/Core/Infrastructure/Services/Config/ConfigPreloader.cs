using System;
using System.Collections.Generic;
using Core.Abstractions.Config;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Core.Infrastructure.Services.Config
{
  public static class ConfigPreloader
  {
    public const string Label = "Configs";

    private static IReadOnlyList<ConfigBase> _configs;

    public static bool IsReady => _configs != null;

    public static IReadOnlyList<ConfigBase> Configs =>
      _configs ?? throw new InvalidOperationException($"Configs with label '{Label}' are not preloaded yet");

    public static async UniTask PreloadAsync()
    {
      if (IsReady)
      {
        return;
      }

      var handle = Addressables.LoadAssetsAsync<ConfigBase>(Label, null);
      var configs = await handle.ToUniTask();
      if (configs.Count == 0)
      {
        throw new InvalidOperationException($"No configs found by Addressables label '{Label}'");
      }

      _configs = new List<ConfigBase>(configs);
    }

    public static void Provide(IEnumerable<ConfigBase> configs)
    {
      _configs = new List<ConfigBase>(configs);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void Reset()
    {
      _configs = null;
    }
  }
}
