using UnityEngine;
using System;

namespace MissingReferencesVisualizer
{
    [Serializable]
    public class MissingRefInfo
    {
        public GameObject gameObject;
        public Component component;
        public string fieldName;
        public string componentTypeName;
        public string hierarchyPath;

        public MissingRefInfo(GameObject go, Component comp, string field, string compType, string path)
        {
            gameObject = go;
            component = comp;
            fieldName = field;
            componentTypeName = compType;
            hierarchyPath = path;
        }

        public override string ToString()
        {
            return $"{hierarchyPath} → {componentTypeName}.{fieldName}";
        }
    }
}

