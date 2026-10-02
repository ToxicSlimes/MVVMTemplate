namespace Core.Mediator.Registration.Extensions
{
    using System;
    using Reflex.Core;

    public static class MediatorReflexExtensions
    {
        public static ContainerBuilder AddMediator(
            this ContainerBuilder builder,
            Action<MediatorBuilder> configure)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            var mediatorBuilder = new MediatorBuilder(builder);
            configure(mediatorBuilder);
            mediatorBuilder.Build();

            return builder;
        }
    }
}
