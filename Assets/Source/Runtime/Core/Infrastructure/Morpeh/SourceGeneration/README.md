# Morpeh source generators

Installed from `kandreyc/Scellecs.Morpeh.SourceGenerator` release `2023.2-RC` (commit `73dd7ab19f0f49e5fe47d2a5aa21db20a7082491`).

Release assets:

- `AspectGenerator.zip`: SHA-256 `4214632a8dc8b409490bc28ef07f92cbd296eb2f9c458d1e61376ffdfdf59cbe`
- `FilterSystemGenerator.zip`: SHA-256 `37970c4d0d5b51f34e6c9cbb93307df96a0fc26d4f22277d9aa75b8dd2b91a37`

The upstream binaries generate bootstrap sources in every Unity assembly. That is incompatible with an asmdef-based Morpeh installation because most assemblies do not reference `Scellecs.Morpeh`. The installed DLLs are rebuilt from the pinned commit with only their unconditional `OnPostInitialize` bootstrap output disabled; equivalent bootstrap types live in this assembly instead.

Installed patched DLLs:

- `Scellecs.Morpeh.SourceGenerator.AspectGenerator.dll`: SHA-256 `7f12006131e98d68c35f3bfa08a68dfc6f181ec72ca8df3d385dfc76cc2700aa`
- `Scellecs.Morpeh.SourceGenerator.FilterSystemGenerator.dll`: SHA-256 `1e2cf3ba7024c49455bf1060e735ec57f6602a1d5782406a46927e687c45edad`

The DLLs are Editor-only managed plugins carrying the `RoslynAnalyzer` asset label. They are rebuilt from the pinned upstream commit for `netstandard2.0`, because Unity 6000 rejects Roslyn analyzers that reference `netstandard` 2.1. Roslyn dependency DLLs are included under `Roslyn/` so plugin reference validation succeeds when this folder is consumed as a UPM package.
