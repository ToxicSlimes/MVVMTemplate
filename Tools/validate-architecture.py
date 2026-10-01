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
        "Core.Mediator.Contracts": "Assets/Source/Runtime/Core/Mediator/Contracts/Core.Mediator.Contracts.asmdef",
        "Core.Mediator.Reflex": "Assets/Source/Runtime/Core/Mediator/Reflex/Core.Mediator.Reflex.asmdef",
    }

    for name, expected_path in expected_paths.items():
        entry = assemblies.get(name)
        if entry is None:
            errors.append(f"Missing assembly: {name}")
            continue

        actual_path = entry[0].relative_to(ROOT).as_posix()
        if actual_path != expected_path:
            errors.append(f"{name} is at {actual_path}, expected {expected_path}")

    if "Core.Infrastructure.Mediator" in assemblies:
        errors.append("Legacy assembly Core.Infrastructure.Mediator still exists")

    application = assemblies.get("Core.Application")
    if application:
        references = resolve_internal_references(application[1], guid_to_name)
        forbidden = references.intersection({"Core.Mediator.Reflex", "Core.Infrastructure.Mediator"})
        if forbidden:
            errors.append(f"Core.Application depends on mediator implementation: {sorted(forbidden)}")
        if "Core.Mediator.Contracts" not in references:
            errors.append("Core.Application must reference Core.Mediator.Contracts")

    contracts = assemblies.get("Core.Mediator.Contracts")
    if contracts and not contracts[1].get("noEngineReferences", False):
        errors.append("Core.Mediator.Contracts must set noEngineReferences=true")

    if errors:
        for error in errors:
            print(f"ERROR: {error}")
        return 1

    print("Architecture validation passed.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
