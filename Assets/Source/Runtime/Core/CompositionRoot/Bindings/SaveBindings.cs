using System;
using Core.Abstractions.Services;
using Core.Abstractions.Services.Save;
using Core.Application.Services.Save;
using Core.Application.Services.Save.Migrations;
using Core.Infrastructure.Services.Save;
using Reflex.Core;

namespace Core.DependencyInjection.Bindings
{
    public static class SaveBindings
    {
        public static ContainerBuilder AddSaveServices(this ContainerBuilder builder)
        {
            builder.AddSingleton<JsonSnapshotMigrationPipeline>(_ =>
                new JsonSnapshotMigrationPipeline(
                    Array.Empty<IJsonSnapshotMigration>(),
                    SaveSchema.CurrentVersion));

            builder.AddSingleton<IRawSaveStore>(container =>
                new FileRawSaveStore(container.Resolve<ILogger>()));

            builder.AddSingleton<ISnapshotCodec>(container =>
                new VersionedJsonSnapshotCodec(
                    container.Resolve<JsonSnapshotMigrationPipeline>()));

            builder.AddSingleton<ISaveService>(container =>
                new SaveService(
                    container.Resolve<IRawSaveStore>(),
                    container.Resolve<ISnapshotCodec>(),
                    container.Resolve<ILogger>()));

            return builder;
        }
    }
}
