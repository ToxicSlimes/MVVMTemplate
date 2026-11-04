using System;
using Reflex.Core;

namespace Core.Infrastructure.StaticEcs
{
    public static class ReflexStaticEcsFeatureExtensions
    {
        /// <summary>
        /// Constructs an unregistered StaticEcs feature through Reflex.
        /// Constructor and attribute injection are applied, and every call returns a new instance.
        /// The container does not own the feature; register it with the runtime, which destroys it with the pipeline.
        /// </summary>
        public static TFeature CreateFeature<TFeature>(this Container container)
            where TFeature : StaticEcsFeature
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            return container.Construct<TFeature>();
        }
    }
}
