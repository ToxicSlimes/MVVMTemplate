using System;
using System.IO;
using System.Threading.Tasks;
using Core.Abstractions.Services;
using Core.Infrastructure.Services.Save;
using NUnit.Framework;

namespace Core.Tests.Infrastructure.Save
{
    public sealed class FileRawSaveStoreTests
    {
        private string _root;
        private FileRawSaveStore _store;

        [SetUp]
        public void SetUp()
        {
            _root = Path.Combine(Path.GetTempPath(), "mvvm-save-tests", Guid.NewGuid().ToString("N"));
            _store = new FileRawSaveStore(_root, new TestLogger());
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_root))
                Directory.Delete(_root, recursive: true);
        }

        [Test]
        public async Task SaveAsync_SecondWriteCreatesBackupAndReplacesPrimary()
        {
            var first = await _store.SaveAsync("saves/slot.sav", new byte[] { 1 });
            var second = await _store.SaveAsync("saves/slot.sav", new byte[] { 2 });
            var copies = await _store.LoadCopiesAsync("saves/slot.sav");

            Assert.That(first.IsSuccess, Is.True, first.Message);
            Assert.That(second.IsSuccess, Is.True, second.Message);
            Assert.That(copies.IsSuccess, Is.True, copies.Message);
            Assert.That(copies.Value.Primary.Value, Is.EqualTo(new byte[] { 2 }));
            Assert.That(copies.Value.Backup.Value, Is.EqualTo(new byte[] { 1 }));
        }

        [Test]
        public async Task SaveAsync_RejectsPathTraversal()
        {
            var result = await _store.SaveAsync("../outside.sav", new byte[] { 1 });

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(File.Exists(Path.Combine(_root, "..", "outside.sav")), Is.False);
        }

        [TestCase("slot.bak")]
        [TestCase("slot.tmp")]
        [TestCase("SLOT.BAK")]
        [TestCase("slot.bak/child.sav")]
        [TestCase("slot.tmp./child.sav")]
        [TestCase("slot.sav.bak::$DATA")]
        public async Task SaveAsync_RejectsReservedInternalSuffix(string slot)
        {
            var result = await _store.SaveAsync(slot, new byte[] { 1 });

            Assert.That(result.IsSuccess, Is.False);
        }

        [Test]
        public async Task SaveAsync_RejectsOversizedPayload()
        {
            var data = new byte[FileRawSaveStore.MaxSaveFileSizeBytes + 1];

            var result = await _store.SaveAsync("saves/large.sav", data);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Does.Contain("maximum"));
        }

        [Test]
        public async Task DeleteAsync_RemovesPrimaryBackupAndStaleTemp()
        {
            const string slot = "saves/slot.sav";
            await _store.SaveAsync(slot, new byte[] { 1 });
            await _store.SaveAsync(slot, new byte[] { 2 });
            File.WriteAllBytes(Path.Combine(_root, slot + ".tmp"), new byte[] { 3 });

            var result = await _store.DeleteAsync(slot);

            Assert.That(result.IsSuccess, Is.True, result.Message);
            Assert.That(File.Exists(Path.Combine(_root, slot)), Is.False);
            Assert.That(File.Exists(Path.Combine(_root, slot + ".bak")), Is.False);
            Assert.That(File.Exists(Path.Combine(_root, slot + ".tmp")), Is.False);
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
