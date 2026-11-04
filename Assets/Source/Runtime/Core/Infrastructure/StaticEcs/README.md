# StaticEcs composition

Enable `STATIC_ECS` and disable `MORPEH` and Morpeh's own auto-initialization. The neutral composition root discovers `StaticEcsRegistrationContributor`, registers one Reflex singleton and starts it via `ICoreStartupContributor`. The singleton creates the default static world, configures features, initializes world then systems, and creates a persistent Unity runner. Each frame updates systems then advances tracking. Runner destruction or Reflex container disposal disposes the owner exactly once; systems are destroyed before the world. Never start two project containers concurrently with this world marker.

A consumer contributor registers each feature explicitly in `RegisterBindings`:

```csharp
builder.AddSingleton(_ => new MyFeature(), typeof(IStaticEcsFeature));
```

`MyFeature.Configure(runtime, container)` may resolve dependencies and call `runtime.Add(system, order)`. Features execute in Reflex binding order; systems initialize/update/destroy in ascending `order`, stable by registration order. Register all systems before startup. `Initialize(container, false)` disables the Unity runner for a caller-provided update loop; call `owner.Update()` once per frame and dispose its container. Do not use manual and Unity runners together.

Consumer asmdefs need `Core.Infrastructure.StaticEcs`, `FFS.StaticEcs`, `Reflex`, and `Core.Mediator.Reflex` (plus `Core.Mediator.Contracts` if using contributor mediator signatures). Preserve reflection-discovered external contributors and their constructors with `[UnityEngine.Scripting.Preserve]` or link.xml; preserve feature/system types reached only by reflection, and ensure concrete generic instantiations used by the project survive IL2CPP stripping. This package preserves its own contributor and runner. Verify an IL2CPP player: preservation attributes are not a substitute for a player build.

Use a unique world marker and a unique systems marker for every simultaneously live runtime, even across different worlds: upstream pipeline resources are shared by systems marker. Both markers remain reserved after cleanup failure. These guards cannot detect pipelines created directly through upstream APIs; do not mix direct and managed ownership.

A failed system initialization is not transactional in the upstream pipeline: previously initialized systems may not receive Destroy. Cleanup failure retains the world reservation until domain reload. All calls are main-thread only. Do not manipulate the same static world outside its owner.

## Template contracts

- `StaticEcsRepository<TWorld>`: base for repositories (`CreateEntity`, `RemoveEntity`, `AddComponent`/`GetComponent`/`HasComponent`); construct after the runtime creates the world, e.g. inside `IStaticEcsFeature.Configure`. Component types must be registered via `World<TWorld>.Types()` before initialization.
- `EventBridgeSystem<TWorld, TEvent>`: register the event type, add the bridge system, events sent after `Initialize` are delivered once per `Update`.
- `StaticEcsFeature` + `container.CreateFeature<TFeature>()`: Reflex-constructed (constructor and `[Inject]`) group of systems added in the feature constructor with `AddSystem`. StaticEcs cannot add systems after initialization, so register the feature (singleton in Reflex, `runtime.Add(feature, order)` in `Configure`) before startup and call `Enable()` in state Enter and `Disable()` in Exit; a disabled feature skips Update but still runs Init/Destroy with the pipeline.
