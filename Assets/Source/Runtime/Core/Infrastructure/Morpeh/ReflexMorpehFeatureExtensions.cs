using System;
using System.Runtime.CompilerServices;
using Reflex.Core;
using Scellecs.Morpeh.Addons.Feature;

namespace Core.Infrastructure.Morpeh
{
    public static class ReflexMorpehFeatureExtensions
    {
        /// <summary>
        /// Constructs an unregistered Morpeh feature through Reflex.
        /// Constructor and attribute injection are applied, and every call returns a new instance.
        /// The container does not own the feature lifetime; the caller or Morpeh world must dispose it.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TFeature CreateFeature<TFeature>(this Container container)
            where TFeature : BaseFeature
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            return container.Construct<TFeature>();
        }
    }
}
