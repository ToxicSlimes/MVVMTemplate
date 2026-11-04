using System.Linq;
using Core.DependencyInjection;
using NUnit.Framework;

namespace Core.Tests
{
    public sealed class EcsBackendIsolationTests
    {
        [Test]
        public void CompositionRoot_DoesNotReferenceMorpehAssemblies()
        {
            var references = typeof(CoreRegistrationContributors).Assembly.GetReferencedAssemblies();
            Assert.That(references.Any(reference => reference.Name.Contains("Morpeh")), Is.False);
        }
    }
}
