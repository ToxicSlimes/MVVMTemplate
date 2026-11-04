using System;
using Core.Abstractions.Config;
using Core.Abstractions.Services;
using Core.Infrastructure.Services.Config;
using Core.Presentation.MVVM.Config;
using NUnit.Framework;
using UnityEngine;
using ILogger = Core.Abstractions.Services.ILogger;

namespace Core.Tests.Infrastructure.Config
{
    public sealed class ConfigProviderTests
    {
        [SetUp]
        public void SetUp() => ConfigPreloader.Reset();

        [TearDown]
        public void TearDown() => ConfigPreloader.Reset();

        [Test]
        public void GetConfig_ServesPreloadedConfigFromCache()
        {
            var config = ScriptableObject.CreateInstance<ViewConfig>();
            ConfigPreloader.Provide(new ConfigBase[] { config });
            var provider = new ConfigProvider(null, new TestLogger());

            Assert.That(provider.GetConfig<ViewConfig>(), Is.SameAs(config));
            Assert.That(provider.IsConfigLoaded<ViewConfig>(), Is.True);
            UnityEngine.Object.DestroyImmediate(config);
        }

        [Test]
        public void GetConfig_ThrowsOnCacheMissAfterPreload()
        {
            ConfigPreloader.Provide(Array.Empty<ConfigBase>());
            var provider = new ConfigProvider(null, new TestLogger());

            Assert.Throws<InvalidOperationException>(() => provider.GetConfig<ViewConfig>());
            Assert.That(provider.IsConfigLoaded<ViewConfig>(), Is.False);
        }

        [Test]
        public void GetConfig_ThrowsWhenNothingWasPreloaded()
        {
            var provider = new ConfigProvider(null, new TestLogger());

            Assert.Throws<InvalidOperationException>(() => provider.GetConfig<ViewConfig>());
        }

        private sealed class TestLogger : ILogger
        {
            public LogLevel MinimumLevel { get; set; }
            public void Log(LogLevel level, string messageTemplate, params object[] args) { }
            public void Debug(string messageTemplate, params object[] args) { }
            public void Info(string messageTemplate, params object[] args) { }
            public void Warn(string messageTemplate, params object[] args) { }
            public void Error(string messageTemplate, params object[] args) { }
            public void Error(Exception exception, string messageTemplate = "", params object[] args) { }
        }
    }
}
