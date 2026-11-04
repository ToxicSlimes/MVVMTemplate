using System.Collections.Generic;
using System.Reflection;
using Core.Abstractions.StateMachines;
using Core.Abstractions.StateMachines.States;
using Core.DependencyInjection;
using Core.Editor.CodeGenPipeline;
using Core.Mediator.Registration;
using Core.Mediator.Registration.Extensions;
using NUnit.Framework;
using Reflex.Core;

namespace Core.Tests
{
    public sealed class ConsumerRegistrationContributorTests
    {
        [Test]
        public void Apply_InvokesEveryBindingAndMediatorContributionOnce()
        {
            var first = new RecordingContributor();
            var second = new RecordingContributor();
            var contributors = new List<ICoreRegistrationContributor> { first, second };
            var builder = new ContainerBuilder();

            CoreRegistrationContributors.ApplyBindings(builder, contributors);
            builder.AddMediator(mediator =>
                CoreRegistrationContributors.ApplyMediator(mediator, contributors));

            Assert.That(first.BindingCalls, Is.EqualTo(1));
            Assert.That(first.MediatorCalls, Is.EqualTo(1));
            Assert.That(second.BindingCalls, Is.EqualTo(1));
            Assert.That(second.MediatorCalls, Is.EqualTo(1));
        }

        [Test]
        public void AddCore_BootUsesSameContributorsInRegistrationOrder()
        {
            var calls = new List<string>();
            var first = new LifecycleContributor("first", calls);
            var second = new LifecycleContributor("second", calls);
            var builder = new ContainerBuilder();
            builder.AddCore(false, new ICoreRegistrationContributor[] { first, second });
            builder.AddSingleton<IGameStateMachine>(_ => new RecordingStateMachine(calls));
            using (var container = builder.Build())
            {
                typeof(ProjectInstaller).GetMethod("Boot", BindingFlags.NonPublic | BindingFlags.Static)
                    .Invoke(null, new object[] { container });
            }
            CollectionAssert.AreEqual(new[] { "mediator:first", "mediator:second", "binding:first", "binding:second", "startup:first", "startup:second", "state" }, calls);
        }

        private sealed class LifecycleContributor : ICoreRegistrationContributor, ICoreStartupContributor
        {
            private readonly string _name;
            private readonly List<string> _calls;
            private bool _mediator;
            private bool _binding;
            public LifecycleContributor(string name, List<string> calls) { _name = name; _calls = calls; }
            public void RegisterMediator(MediatorBuilder builder) { _mediator = true; _calls.Add("mediator:" + _name); }
            public void RegisterBindings(ContainerBuilder builder) { _binding = true; _calls.Add("binding:" + _name); }
            public void Initialize(Container container)
            {
                Assert.That(_mediator && _binding, Is.True, "startup must use the registered instance");
                _calls.Add("startup:" + _name);
            }
        }

        private sealed class RecordingStateMachine : IGameStateMachine
        {
            private readonly List<string> _calls;
            public RecordingStateMachine(List<string> calls) { _calls = calls; }
            public void Enter<TState>() where TState : class, IEnterState { _calls.Add("state"); }
            public void Enter<TState, TPayload>(TPayload payload) where TState : class, IPayloadState<TPayload> { _calls.Add("state"); }
        }

        [Test]
        public void ConvertToConsumerPart_PreservesContributionWithoutFrameworkRegistrar()
        {
            const string frameworkRegistration =
                "using Reflex.Core;\n" +
                "namespace Core.DependencyInjection.Generated\n" +
                "{\n" +
                "    public static class ViewModelRegistration\n" +
                "    {\n" +
                "        public static ContainerBuilder RegisterViewModels(this ContainerBuilder builder)\n" +
                "        {\n" +
                "            builder.AddSingleton(_ => new ConsumerViewModel());\n" +
                "            return builder;\n" +
                "        }\n" +
                "    }\n" +
                "}\n";

            var consumerPart = ConsumerCodeGenContext.ConvertToConsumerPart(
                frameworkRegistration,
                "RegisterViewModels",
                "RegisterViewModels",
                "ContainerBuilder");

            StringAssert.Contains("partial class ConsumerRegistration", consumerPart);
            StringAssert.Contains("builder.AddSingleton(_ => new ConsumerViewModel());", consumerPart);
            StringAssert.DoesNotContain("class ViewModelRegistration", consumerPart);
            StringAssert.DoesNotContain("return builder;", consumerPart);
        }

        private sealed class RecordingContributor : ICoreRegistrationContributor
        {
            public int BindingCalls { get; private set; }
            public int MediatorCalls { get; private set; }

            public void RegisterBindings(ContainerBuilder builder)
            {
                BindingCalls++;
            }

            public void RegisterMediator(MediatorBuilder builder)
            {
                MediatorCalls++;
            }
        }
    }
}
