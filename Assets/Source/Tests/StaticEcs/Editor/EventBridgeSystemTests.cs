using System.Collections.Generic;
using Core.Infrastructure.StaticEcs;
using FFS.Libraries.StaticEcs;
using NUnit.Framework;

namespace Core.Tests.StaticEcs
{
    public sealed class EventBridgeSystemTests
    {
        private struct TestWorld : IWorldType { }
        private struct TestSystems : ISystemsType { }
        private struct TestEvent : IEvent { public int Value; }

        private sealed class RecordingBridge : EventBridgeSystem<TestWorld, TestEvent>
        {
            public readonly List<int> Values = new List<int>();
            protected override void Bridge(in TestEvent value) => Values.Add(value.Value);
        }

        [Test]
        public void Pipeline_DeliversEachEventOnce()
        {
            var bridge = new RecordingBridge();
            using (var runtime = new StaticEcsRuntime<TestWorld, TestSystems>())
            {
                World<TestWorld>.Types().Event<TestEvent>();
                runtime.Add(bridge);
                runtime.Initialize();
                Assert.That(World<TestWorld>.SendEvent(new TestEvent { Value = 42 }), Is.True);
                runtime.Update();
                runtime.Update();
                Assert.That(bridge.Values, Is.EqualTo(new[] { 42 }));
                bridge.Destroy();
                Assert.That(World<TestWorld>.SendEvent(new TestEvent { Value = 7 }), Is.False);
            }
        }
    }
}
