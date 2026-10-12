# Morpeh source generators

Installed from `kandreyc/Scellecs.Morpeh.SourceGenerator` release `2023.2-RC` (commit `73dd7ab19f0f49e5fe47d2a5aa21db20a7082491`).

Release assets:

- `AspectGenerator.zip`: SHA-256 `4214632a8dc8b409490bc28ef07f92cbd296eb2f9c458d1e61376ffdfdf59cbe`
- `FilterSystemGenerator.zip`: SHA-256 `37970c4d0d5b51f34e6c9cbb93307df96a0fc26d4f22277d9aa75b8dd2b91a37`

The upstream binaries generate bootstrap sources in every Unity assembly. That is incompatible with an asmdef-based Morpeh installation because most assemblies do not reference `Scellecs.Morpeh`. The installed DLLs are rebuilt from the pinned commit with only their unconditional `OnPostInitialize` bootstrap output disabled; equivalent bootstrap types live in this assembly instead.

Installed patched DLLs:

- `Scellecs.Morpeh.SourceGenerator.AspectGenerator.dll`: SHA-256 `e0a27218fd9587593567eb5501da1cdd8a753c7b1fbd6474e2305cc020c28ed2`
- `Scellecs.Morpeh.SourceGenerator.FilterSystemGenerator.dll`: SHA-256 `ebee65557fbb9074523e4ce7d498b0d09eb7cba070689b2a4c0aee6a4a1700e7`

The DLLs are Editor-only managed plugins carrying the `RoslynAnalyzer` asset label. `UnityDependencies.zip` is intentionally not imported: Unity supplies its own Roslyn host assemblies.
