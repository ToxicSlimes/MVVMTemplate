#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Core.Editor.PrefabBuilderSystem.Core;
using UnityEngine;

namespace Core.Editor.PrefabBuilderSystem.Validation
{
    public class ReferenceValidator : BaseUIValidator
    {
        public override string Name => "Reference Validator";
        public override string Description => "Validates registered component references in Context";

        public override IEnumerable<ValidationIssue> Validate(GameObject root, PrefabBuilderContext context)
        {
            var issues = new List<ValidationIssue>();

            if (context == null)
            {
                issues.Add(Info("No PrefabBuilderContext provided - skipping reference validation"));
                return issues;
            }

            var keys = context.GetAllKeys().ToList();

            foreach (var key in keys)
            {
                var go = context.GetGameObject(key);
                if (go == null)
                {
                    issues.Add(Error(
                        $"Registered key '{key}' has null GameObject reference",
                        null,
                        "Ensure the registered object is not destroyed"));
                    continue;
                }

                if (!IsChildOf(go.transform, root.transform) && go != root)
                {
                    issues.Add(Warning(
                        $"Registered key '{key}' points to GameObject outside of root hierarchy",
                        go,
                        "Ensure registered objects are part of the prefab hierarchy"));
                }
            }

            var duplicateGameObjects = keys
                .Select(k => context.GetGameObject(k))
                .Where(go => go != null)
                .GroupBy(go => go)
                .Where(g => g.Count() > 1)
                .ToList();

            foreach (var dup in duplicateGameObjects)
            {
                var keysForObject = keys.Where(k => context.GetGameObject(k) == dup.Key).ToList();
                issues.Add(Info(
                    $"Same GameObject registered under multiple keys: {string.Join(", ", keysForObject)}",
                    dup.Key,
                    "Consider using a single key per GameObject"));
            }

            return issues;
        }

        private bool IsChildOf(Transform child, Transform parent)
        {
            var current = child;
            while (current != null)
            {
                if (current == parent)
                    return true;
                current = current.parent;
            }
            return false;
        }
    }
}
#endif
