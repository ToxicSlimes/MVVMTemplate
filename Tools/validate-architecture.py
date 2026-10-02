#!/usr/bin/env python3
import json
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / "Assets" / "Source"


def load_assemblies():
    assemblies = {}
    guid_to_name = {}

    for path in SOURCE.rglob("*.asmdef"):
        data = json.loads(path.read_text(encoding="utf-8-sig"))
        name = data["name"]
        assemblies[name] = (path, data)

        meta_path = Path(f"{path}.meta")
        if not meta_path.exists():
            continue

        match = re.search(r"^guid:\s*(\S+)", meta_path.read_text(encoding="utf-8-sig"), re.MULTILINE)
        if match:
            guid_to_name[match.group(1)] = name

    return assemblies, guid_to_name


def resolve_internal_references(data, guid_to_name):
    resolved = set()
    for reference in data.get("references", []):
        if reference.startswith("GUID:"):
            name = guid_to_name.get(reference.removeprefix("GUID:"))
            if name:
                resolved.add(name)
        elif reference.startswith("Core."):
            resolved.add(reference)
    return resolved


def main():
    assemblies, guid_to_name = load_assemblies()
    errors = []

    expected_paths = {
        "Core.Domain": "Assets/Source/Runtime/Core/Domain/Core.Domain.asmdef",
        "Core.Application": "Assets/Source/Runtime/Core/Application/Core.Application.asmdef",
        "Core.Presentation.States": "Assets/Source/Runtime/Core/Presentation/Core.Presentation.States.asmdef",
        "Core.Presentation.MVVM": "Assets/Source/Runtime/Core/Presentation/MVVM/Core.Presentation.MVVM.asmdef",
        "Core.Presentation.UI": "Assets/Source/Runtime/Core/Presentation/UI/Core.Presentation.UI.asmdef",
        "Core.Presentation.UI.Toolkit": "Assets/Source/Runtime/Core/Presentation/UI/Toolkit/Core.Presentation.UI.Toolkit.asmdef",
        "Core.Presentation.UI.Toolkit.Elements": "Assets/Source/Runtime/Core/Presentation/UI/Toolkit/Elements/Core.Presentation.UI.Toolkit.Elements.asmdef",
        "Core.Infrastructure": "Assets/Source/Runtime/Core/Infrastructure/Core.Infrastructure.asmdef",
        "Core.Infrastructure.Configs": "Assets/Source/Runtime/Core/Infrastructure/Configs/Core.Infrastructure.Configs.asmdef",
        "Core.CompositionRoot": "Assets/Source/Runtime/Core/CompositionRoot/Core.CompositionRoot.asmdef",
        "Core.Mediator.Contracts": "Assets/Source/Runtime/Core/Mediator/Contracts/Core.Mediator.Contracts.asmdef",
        "Core.Mediator.Reflex": "Assets/Source/Runtime/Core/Mediator/Reflex/Core.Mediator.Reflex.asmdef",
    }

    legacy_assemblies = {
        "Core.Data",
        "Core.Configs",
        "Core.DependencyInjection",
        "Core.MVVM",
        "Core.Presentation",
        "Core.UI",
        "Core.UI.Toolkit",
        "Core.UI.Toolkit.Elements",
        "Core.Infrastructure.Mediator",
    }

    for name, expected_path in expected_paths.items():
        entry = assemblies.get(name)
        if entry is None:
            errors.append(f"Missing assembly: {name}")
            continue

        actual_path = entry[0].relative_to(ROOT).as_posix()
        if actual_path != expected_path:
            errors.append(f"{name} is at {actual_path}, expected {expected_path}")

    found_legacy = legacy_assemblies.intersection(assemblies)
    if found_legacy:
        errors.append(f"Legacy assembly names still exist: {sorted(found_legacy)}")

    application = assemblies.get("Core.Application")
    if application:
        references = resolve_internal_references(application[1], guid_to_name)
        forbidden = {
            reference
            for reference in references
            if reference.startswith("Core.Presentation.")
            or reference.startswith("Core.Infrastructure.")
            or reference == "Core.Mediator.Reflex"
        }
        if forbidden:
            errors.append(f"Core.Application has outward dependencies: {sorted(forbidden)}")
        if "Core.Mediator.Contracts" not in references:
            errors.append("Core.Application must reference Core.Mediator.Contracts")

    infrastructure = assemblies.get("Core.Infrastructure")
    if infrastructure:
        references = resolve_internal_references(infrastructure[1], guid_to_name)
        forbidden = sorted(
            reference for reference in references if reference.startswith("Core.Presentation.")
        )
        if forbidden:
            errors.append(f"Core.Infrastructure depends on Presentation: {forbidden}")

    for name, (_, data) in assemblies.items():
        if not name.startswith("Core.Presentation."):
            continue
        references = resolve_internal_references(data, guid_to_name)
        forbidden = sorted(
            reference for reference in references if reference.startswith("Core.Infrastructure.")
        )
        if forbidden:
            errors.append(f"{name} depends on Infrastructure: {forbidden}")

    contracts = assemblies.get("Core.Mediator.Contracts")
    if contracts and not contracts[1].get("noEngineReferences", False):
        errors.append("Core.Mediator.Contracts must set noEngineReferences=true")

    for source_path in SOURCE.rglob("*.cs"):
        source = source_path.read_text(encoding="utf-8-sig")
        if "Core.Infrastructure.MVVM" in source:
            relative_path = source_path.relative_to(ROOT).as_posix()
            errors.append(f"Legacy MVVM namespace remains: {relative_path}")
        if "Core.MVVM" in source:
            relative_path = source_path.relative_to(ROOT).as_posix()
            errors.append(f"Legacy presentation namespace remains: {relative_path}")
        if "Core.Infrastructure.Mediator" in source:
            relative_path = source_path.relative_to(ROOT).as_posix()
            errors.append(f"Legacy mediator namespace remains: {relative_path}")
        if "Core.Data" in source:
            relative_path = source_path.relative_to(ROOT).as_posix()
            errors.append(f"Legacy domain namespace remains: {relative_path}")
        if "Core.Configs" in source:
            relative_path = source_path.relative_to(ROOT).as_posix()
            errors.append(f"Legacy config namespace remains: {relative_path}")

    views_root = SOURCE / "Runtime" / "Core" / "Presentation" / "MVVM" / "Views"
    for view_path in views_root.rglob("*.cs"):
        source = view_path.read_text(encoding="utf-8-sig")
        if re.search(r"\bIMediator\b", source):
            relative_path = view_path.relative_to(ROOT).as_posix()
            errors.append(
                f"View must communicate through its ViewModel, not IMediator directly: {relative_path}"
            )

    if errors:
        for error in errors:
            print(f"ERROR: {error}")
        return 1

    print("Architecture validation passed.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
