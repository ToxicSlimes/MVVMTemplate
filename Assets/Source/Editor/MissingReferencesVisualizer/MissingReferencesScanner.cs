#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MissingReferencesVisualizer
{
    public static class MissingReferencesScanner
    {
        private static Dictionary<string, List<MissingRefInfo>> cachedResults = new Dictionary<string, List<MissingRefInfo>>();
        private static string lastScenesHash = "";

        public static List<MissingRefInfo> ScanActiveScene()
        {
            return ScanAllScenes();
        }

        public static List<MissingRefInfo> ScanAllScenes()
        {
            List<MissingRefInfo> allMissingRefs = new List<MissingRefInfo>();
            List<string> scenePaths = new List<string>();

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;

                string scenePath = scene.path;
                string sceneKey = string.IsNullOrEmpty(scenePath) ? $"Untitled_{scene.GetHashCode()}" : scenePath;
                scenePaths.Add(sceneKey);

                if (cachedResults.ContainsKey(sceneKey))
                {
                    allMissingRefs.AddRange(cachedResults[sceneKey]);
                    continue;
                }

                List<MissingRefInfo> sceneMissingRefs = new List<MissingRefInfo>();
                GameObject[] rootObjects = scene.GetRootGameObjects();

                foreach (GameObject root in rootObjects)
                {
                    ScanGameObject(root, sceneMissingRefs, sceneKey);
                }

                cachedResults[sceneKey] = sceneMissingRefs;
                allMissingRefs.AddRange(sceneMissingRefs);
            }

            string currentScenesHash = string.Join("|", scenePaths.OrderBy(s => s));
            if (currentScenesHash != lastScenesHash)
            {
                List<string> scenesToRemove = cachedResults.Keys.Where(k => !scenePaths.Contains(k)).ToList();
                foreach (string key in scenesToRemove)
                {
                    cachedResults.Remove(key);
                }
                lastScenesHash = currentScenesHash;
            }
            
            allMissingRefs.RemoveAll(r => r.gameObject == null);

            return allMissingRefs;
        }

        private static void ScanGameObject(GameObject go, List<MissingRefInfo> results, string sceneKey)
        {
            if (go == null) return;

            Component[] components = go.GetComponents<Component>();
            foreach (Component comp in components)
            {
                if (comp == null)
                {
                    string path = GetHierarchyPath(go);
                    results.Add(new MissingRefInfo(go, null, "Missing Script", "Missing Script", $"[{sceneKey}] {path}"));
                    continue;
                }

                SerializedObject so = new SerializedObject(comp);
                SerializedProperty prop = so.GetIterator();

                while (prop.NextVisible(true))
                {
                    if (prop.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        if (prop.objectReferenceValue == null && prop.objectReferenceEntityIdValue.GetHashCode() != 0)
                        {
                            string path = GetHierarchyPath(go);
                            string componentType = comp.GetType().Name;
                            string fieldName = prop.displayName;
                            
                            results.Add(new MissingRefInfo(go, comp, fieldName, componentType, $"[{sceneKey}] {path}"));
                        }
                    }
                }
            }

            foreach (Transform child in go.transform)
            {
                ScanGameObject(child.gameObject, results, sceneKey);
            }
        }

        private static string GetHierarchyPath(GameObject go)
        {
            string path = go.name;
            Transform current = go.transform.parent;
            
            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }
            
            return path;
        }

        public static bool HasMissingRefs(GameObject go)
        {
            if (go == null) return false;
            
            List<MissingRefInfo> allRefs = ScanAllScenes();
            return allRefs.Any(refInfo => 
                refInfo.gameObject != null && 
                (refInfo.gameObject == go || IsChildOf(refInfo.gameObject, go)));
        }

        public static List<MissingRefInfo> GetMissingRefsForGameObject(GameObject go)
        {
            if (go == null) return new List<MissingRefInfo>();
            
            List<MissingRefInfo> allRefs = ScanAllScenes();
            return allRefs.Where(refInfo => refInfo.gameObject != null && refInfo.gameObject == go).ToList();
        }

        public static bool IsChildOf(GameObject child, GameObject parent)
        {
            if (child == null || parent == null) return false;
            
            Transform current = child.transform;
            while (current != null)
            {
                if (current.gameObject == parent)
                    return true;
                current = current.parent;
            }
            return false;
        }

        public static void ClearCache()
        {
            cachedResults.Clear();
            lastScenesHash = "";
        }
    }
}
#endif

