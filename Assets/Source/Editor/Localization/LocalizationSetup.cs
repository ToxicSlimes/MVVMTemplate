#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Localization;
using Core.Configs.Services;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace Core.Editor.Localization
{
    public static class LocalizationSetup
    {
        private const string RootPath = "Assets/Source/00. Configs/Localization";
        private const string TablesPath = RootPath + "/Tables";
        private const string LocalesPath = RootPath + "/Locales";
        private const string SettingsPath = RootPath + "/Localization Settings.asset";
        private const string ConfigPath = RootPath + "/LocalizationConfig.asset";

        private static readonly (string Code, string Name)[] LocaleDefinitions =
        {
            ("en", "English"),
            ("ru", "Russian"),
            ("de", "German"),
            ("fr", "French"),
            ("es", "Spanish"),
            ("zh-Hans", "Chinese (Simplified)"),
            ("ja", "Japanese"),
            ("ko", "Korean")
        };

        private static readonly (string Key, string English, string Russian)[] PilotEntries =
        {
            (UiLocalizationKeys.MainMenu.Title, "MVVM Template", "MVVM Шаблон"),
            (UiLocalizationKeys.MainMenu.Subtitle, "Unity 6 · MVVM + CQRS Examples", "Unity 6 · Примеры MVVM + CQRS"),
            (UiLocalizationKeys.MainMenu.Counter, "Counter", "Счётчик"),
            (UiLocalizationKeys.MainMenu.Settings, "Settings", "Настройки"),
            (UiLocalizationKeys.MainMenu.Inventory, "Inventory", "Инвентарь"),
            (UiLocalizationKeys.MainMenu.Multiplayer, "Multiplayer", "Мультиплеер"),
            (UiLocalizationKeys.MainMenu.Quit, "Quit", "Выход"),
            (UiLocalizationKeys.MainMenu.FooterHint, "Open BootstrapScene → Play", "Откройте BootstrapScene → Play"),
            (UiLocalizationKeys.Settings.Title, "Settings", "Настройки"),
            (UiLocalizationKeys.Settings.TabAudio, "Audio", "Аудио"),
            (UiLocalizationKeys.Settings.TabVideo, "Video", "Видео"),
            (UiLocalizationKeys.Settings.TabDisplay, "Display", "Экран"),
            (UiLocalizationKeys.Settings.TabControls, "Controls", "Управление"),
            (UiLocalizationKeys.Settings.TabGame, "Game", "Игра"),
            (UiLocalizationKeys.Settings.Language, "Language", "Язык"),
            (UiLocalizationKeys.Settings.Apply, "Apply", "Применить"),
            (UiLocalizationKeys.Settings.Reset, "Reset", "Сброс"),
            (UiLocalizationKeys.Settings.Cancel, "Cancel", "Отмена"),
            (UiLocalizationKeys.Settings.VolumeLabel, "Volume: {0}%", "Громкость: {0}%")
        };

        private static readonly string[] RequiredPilotKeys =
        {
            UiLocalizationKeys.MainMenu.Title,
            UiLocalizationKeys.MainMenu.Counter,
            UiLocalizationKeys.MainMenu.Settings,
            UiLocalizationKeys.Settings.Title,
            UiLocalizationKeys.Settings.Language,
            UiLocalizationKeys.Settings.Apply,
            UiLocalizationKeys.Settings.VolumeLabel
        };

        [MenuItem("Tools/MVVM/Setup Localization")]
        public static void SetupLocalization()
        {
            EnsureDirectories();
            var locales = EnsureLocales();
            EnsureLocalizationSettings();
            var collection = EnsureStringTableCollection(locales);
            EnsurePilotEntries(collection);
            EnsureLocalizationConfig();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Localization] Setup complete. String table 'UI' and LocalizationConfig are ready.");
        }

        public static int CountBlockingIssues()
        {
            var issues = 0;

            if (LocalizationEditorSettings.ActiveLocalizationSettings == null)
            {
                Debug.LogError("[Template] Localization Settings missing. Run Tools → MVVM → Setup Localization.");
                issues++;
            }

            var config = AssetDatabase.LoadAssetAtPath<LocalizationConfig>(ConfigPath);
            if (config == null)
            {
                Debug.LogError($"[Template] LocalizationConfig missing at {ConfigPath}. Run Tools → MVVM → Setup Localization.");
                issues++;
            }

            var collection = LocalizationEditorSettings.GetStringTableCollection(UiLocalizationKeys.Table);
            if (collection == null)
            {
                Debug.LogError("[Template] UI string table collection missing. Run Tools → MVVM → Setup Localization.");
                issues++;
                return issues;
            }

            foreach (var key in RequiredPilotKeys)
            {
                if (collection.SharedData.GetEntry(key) == null)
                {
                    Debug.LogError($"[Template] Missing localization key '{key}' in UI string table.");
                    issues++;
                }
            }

            return issues;
        }

        private static void EnsureDirectories()
        {
            EnsureDirectory(RootPath);
            EnsureDirectory(TablesPath);
            EnsureDirectory(LocalesPath);
        }

        private static void EnsureDirectory(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            var parent = System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/');
            var folderName = System.IO.Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
                EnsureDirectory(parent);

            AssetDatabase.CreateFolder(parent, folderName);
        }

        private static List<Locale> EnsureLocales()
        {
            var locales = new List<Locale>();

            foreach (var (code, name) in LocaleDefinitions)
            {
                var assetPath = $"{LocalesPath}/{name} ({code}).asset";
                var locale = AssetDatabase.LoadAssetAtPath<Locale>(assetPath);
                if (locale == null)
                {
                    locale = Locale.CreateLocale(code);
                    AssetDatabase.CreateAsset(locale, assetPath);
                }

                locales.Add(locale);
            }

            return locales;
        }

        private static void EnsureLocalizationSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<LocalizationSettings>(SettingsPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<LocalizationSettings>();
                AssetDatabase.CreateAsset(settings, SettingsPath);
            }

            LocalizationEditorSettings.ActiveLocalizationSettings = settings;
            EditorUtility.SetDirty(settings);
        }

        private static StringTableCollection EnsureStringTableCollection(IReadOnlyList<Locale> locales)
        {
            var collection = LocalizationEditorSettings.GetStringTableCollection(UiLocalizationKeys.Table);
            if (collection != null)
                return collection;

            return LocalizationEditorSettings.CreateStringTableCollection(
                UiLocalizationKeys.Table,
                TablesPath,
                locales.ToList());
        }

        private static void EnsurePilotEntries(StringTableCollection collection)
        {
            if (collection == null)
                return;

            foreach (var (key, english, russian) in PilotEntries)
            {
                var sharedEntry = collection.SharedData.GetEntry(key) ?? collection.SharedData.AddKey(key);

                foreach (var stringTable in collection.StringTables)
                {
                    var localeCode = stringTable.LocaleIdentifier.Code;
                    var value = localeCode switch
                    {
                        "ru" => russian,
                        _ => english
                    };

                    var tableEntry = stringTable.GetEntry(sharedEntry.Id);
                    if (tableEntry == null)
                        stringTable.AddEntry(sharedEntry.Id, value);
                    else
                        tableEntry.Value = value;
                }
            }

            EditorUtility.SetDirty(collection);
            EditorUtility.SetDirty(collection.SharedData);
            foreach (var stringTable in collection.StringTables)
                EditorUtility.SetDirty(stringTable);
        }

        private static void EnsureLocalizationConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<LocalizationConfig>(ConfigPath);
            if (config != null)
                return;

            config = ScriptableObject.CreateInstance<LocalizationConfig>();
            AssetDatabase.CreateAsset(config, ConfigPath);
            EditorUtility.SetDirty(config);
        }
    }
}
#endif
