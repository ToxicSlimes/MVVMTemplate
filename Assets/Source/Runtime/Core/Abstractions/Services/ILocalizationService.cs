using System;
using System.Threading;
using Core.Domain.Enums;
using Cysharp.Threading.Tasks;

namespace Core.Abstractions.Services
{
    public interface ILocalizationService
    {
        GameLanguage CurrentLanguage { get; }
        bool IsInitialized { get; }
        event Action<GameLanguage> LanguageChanged;

        UniTask InitializeAsync(CancellationToken ct = default);
        UniTask SetLanguageAsync(GameLanguage language, CancellationToken ct = default);

        string Localize(string table, string key);
        string Localize(string table, string key, params object[] arguments);
        UniTask<string> LocalizeAsync(string table, string key, CancellationToken ct = default);
        UniTask<string> LocalizeAsync(string table, string key, object[] arguments, CancellationToken ct = default);
    }
}
