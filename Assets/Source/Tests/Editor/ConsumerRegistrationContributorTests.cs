using System.Collections.Generic;
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
