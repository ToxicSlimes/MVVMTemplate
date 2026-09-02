using System;
using System.Threading;
using Core.Abstractions.Localization;
using Core.Abstractions.Services;
using Core.Configs.Services;
using Core.Data.Domain.Enums;
using Core.Data.Domain.Models;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace Core.Infrastructure.Services.Localization
{
    public sealed class LocalizationService : ILocalizationService
    {
        private const string GameSettingsKey = "Settings.Game";

        private readonly IPrefsService _prefsService;
        private readonly string _defaultTable;

        public GameLanguage CurrentLanguage { get; private set; } = GameLanguage.English;
        public bool IsInitialized { get; private set; }
        public event Action<GameLanguage> LanguageChanged;

        public LocalizationService(IPrefsService prefsService, LocalizationConfig config = null)
        {
            _prefsService = prefsService ?? throw new ArgumentNullException(nameof(prefsService));
            _defaultTable = config != null ? config.DefaultTable : UiLocalizationKeys.Table;
        }

        public async UniTask InitializeAsync(CancellationToken ct = default)
        {
            if (IsInitialized)
                return;

            await WaitForUnityLocalizationAsync(ct);

            var language = LoadSavedLanguage();
            await SetLanguageAsync(language, ct);
            IsInitialized = true;
        }

        public async UniTask SetLanguageAsync(GameLanguage language, CancellationToken ct = default)
        {
            await WaitForUnityLocalizationAsync(ct);

            var localeCode = GameLanguageLocaleMapper.ToLocaleCode(language);
            var locale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
            if (locale == null)
            {
                Debug.LogWarning($"[Localization] Locale '{localeCode}' not found for {language}");
                return;
            }

            LocalizationSettings.SelectedLocale = locale;

            if (CurrentLanguage != language)
            {
                CurrentLanguage = language;
                LanguageChanged?.Invoke(language);
            }
        }

        public string Localize(string table, string key)
        {
            return LocalizeInternal(table, key, null);
        }

        public string Localize(string table, string key, params object[] arguments)
        {
            return LocalizeInternal(table, key, arguments);
        }

        public async UniTask<string> LocalizeAsync(string table, string key, CancellationToken ct = default)
        {
            return await LocalizeAsync(table, key, null, ct);
        }

        public async UniTask<string> LocalizeAsync(string table, string key, object[] arguments, CancellationToken ct = default)
        {
            await WaitForUnityLocalizationAsync(ct);

            try
            {
                var handle = arguments == null || arguments.Length == 0
                    ? LocalizationSettings.StringDatabase.GetLocalizedStringAsync(ResolveTable(table), key)
                    : LocalizationSettings.StringDatabase.GetLocalizedStringAsync(ResolveTable(table), key, arguments);

                await handle.ToUniTask(cancellationToken: ct);
                return string.IsNullOrEmpty(handle.Result) ? key : handle.Result;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Localization] Async localize failed for '{table}/{key}': {ex.Message}");
                return key;
            }
        }

        private string LocalizeInternal(string table, string key, object[] arguments)
        {
            if (!IsInitialized || LocalizationSettings.Instance == null)
                return key;

            try
            {
                var result = arguments == null || arguments.Length == 0
                    ? LocalizationSettings.StringDatabase.GetLocalizedString(ResolveTable(table), key)
                    : LocalizationSettings.StringDatabase.GetLocalizedString(ResolveTable(table), key, arguments);

                return string.IsNullOrEmpty(result) ? key : result;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Localization] Localize failed for '{table}/{key}': {ex.Message}");
                return key;
            }
        }

        private string ResolveTable(string table)
        {
            return string.IsNullOrWhiteSpace(table) ? _defaultTable : table;
        }

        private GameLanguage LoadSavedLanguage()
        {
            if (!_prefsService.HasKey(GameSettingsKey))
                return GameLanguage.English;

            var data = _prefsService.Load<GameSettingsData>(GameSettingsKey);
            return data?.Language ?? GameLanguage.English;
        }

        private static async UniTask WaitForUnityLocalizationAsync(CancellationToken ct)
        {
            if (LocalizationSettings.Instance == null)
                return;

            var operation = LocalizationSettings.InitializationOperation;
            if (operation.IsDone)
                return;

            await operation.ToUniTask(cancellationToken: ct);
        }
    }
}
