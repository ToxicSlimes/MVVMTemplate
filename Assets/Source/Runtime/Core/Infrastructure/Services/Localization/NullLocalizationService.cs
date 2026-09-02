using System;
using System.Threading;
using Core.Abstractions.Services;
using Core.Data.Domain.Enums;
using Cysharp.Threading.Tasks;

namespace Core.Infrastructure.Services.Localization
{
    public sealed class NullLocalizationService : ILocalizationService
    {
        public GameLanguage CurrentLanguage { get; private set; } = GameLanguage.English;
        public bool IsInitialized { get; private set; }
        public event Action<GameLanguage> LanguageChanged;

        public UniTask InitializeAsync(CancellationToken ct = default)
        {
            IsInitialized = true;
            return UniTask.CompletedTask;
        }

        public UniTask SetLanguageAsync(GameLanguage language, CancellationToken ct = default)
        {
            if (CurrentLanguage == language)
                return UniTask.CompletedTask;

            CurrentLanguage = language;
            LanguageChanged?.Invoke(language);
            return UniTask.CompletedTask;
        }

        public string Localize(string table, string key) => key;

        public string Localize(string table, string key, params object[] arguments) => key;

        public UniTask<string> LocalizeAsync(string table, string key, CancellationToken ct = default)
        {
            return UniTask.FromResult(key);
        }

        public UniTask<string> LocalizeAsync(string table, string key, object[] arguments, CancellationToken ct = default)
        {
            return UniTask.FromResult(key);
        }
    }
}
