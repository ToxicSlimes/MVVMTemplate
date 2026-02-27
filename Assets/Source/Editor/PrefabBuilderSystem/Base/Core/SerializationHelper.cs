#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Core.Editor.PrefabBuilderSystem.Core
{
    public static class SerializationHelper
    {
        public static void Wire(SerializedObject so, string fieldName, Object value)
        {
            var prop = so.FindProperty(fieldName);
            if (prop != null)
            {
                prop.objectReferenceValue = value;
            }
            else
            {
                Debug.LogWarning($"[SerializationHelper] Field '{fieldName}' not found on {so.targetObject.GetType().Name}");
            }
        }

        public static void WireColor(SerializedObject so, string fieldName, Color value)
        {
            var prop = so.FindProperty(fieldName);
            if (prop != null)
            {
                prop.colorValue = value;
            }
            else
            {
                Debug.LogWarning($"[SerializationHelper] Field '{fieldName}' not found on {so.targetObject.GetType().Name}");
            }
        }

        public static void WireVector2(SerializedObject so, string fieldName, Vector2 value)
        {
            var prop = so.FindProperty(fieldName);
            if (prop != null)
            {
                prop.vector2Value = value;
            }
            else
            {
                Debug.LogWarning($"[SerializationHelper] Field '{fieldName}' not found on {so.targetObject.GetType().Name}");
            }
        }

        public static void WireFloat(SerializedObject so, string fieldName, float value)
        {
            var prop = so.FindProperty(fieldName);
            if (prop != null)
            {
                prop.floatValue = value;
            }
            else
            {
                Debug.LogWarning($"[SerializationHelper] Field '{fieldName}' not found on {so.targetObject.GetType().Name}");
            }
        }

        public static void WireInt(SerializedObject so, string fieldName, int value)
        {
            var prop = so.FindProperty(fieldName);
            if (prop != null)
            {
                prop.intValue = value;
            }
            else
            {
                Debug.LogWarning($"[SerializationHelper] Field '{fieldName}' not found on {so.targetObject.GetType().Name}");
            }
        }

        public static void WireBool(SerializedObject so, string fieldName, bool value)
        {
            var prop = so.FindProperty(fieldName);
            if (prop != null)
            {
                prop.boolValue = value;
            }
            else
            {
                Debug.LogWarning($"[SerializationHelper] Field '{fieldName}' not found on {so.targetObject.GetType().Name}");
            }
        }

        public static void WireString(SerializedObject so, string fieldName, string value)
        {
            var prop = so.FindProperty(fieldName);
            if (prop != null)
            {
                prop.stringValue = value;
            }
            else
            {
                Debug.LogWarning($"[SerializationHelper] Field '{fieldName}' not found on {so.targetObject.GetType().Name}");
            }
        }
    }
}
#endif
