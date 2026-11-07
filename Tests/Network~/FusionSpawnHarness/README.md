# FusionSpawner contract probe

This .NET 9 harness compiles the actual `FusionSpawner.cs` with small test doubles for Fusion, UniTask and Unity. It checks the adapter's branching and cleanup calls; it does **not** run Fusion, Unity or a real network session. Fusion lifecycle, authority, callback exception behaviour and actual despawn remain integration-test responsibilities.

From the repository root, use a dedicated scratch directory (absolute paths, with trailing slashes) for build products. For example, on this Windows host with Git Bash:

```sh
dotnet run --project 'Tests/Network~/FusionSpawnHarness/FusionSpawnHarness.csproj' \
  -p:BaseIntermediateOutputPath='C:/Users/Ouroboros/AppData/Local/hermes/cache/scratch/fusion-spawn-repro/obj/' \
  -p:OutputPath='C:/Users/Ouroboros/AppData/Local/hermes/cache/scratch/fusion-spawn-repro/bin/'
```

Use a separate scratch directory per concurrent run. Exit status is nonzero if any probe fails.
