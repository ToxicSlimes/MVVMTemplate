#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MissingReferencesVisualizer
{
    [InitializeOnLoad]
    public static class SceneMissingRefsVisualizer
    {
        private const string ENABLE_KEY = "MissingRefsVisualizer_Enabled";
        private static bool isEnabled = true;
        private static GUIContent warningIcon;
        private static GUIStyle labelStyle;

        static SceneMissingRefsVisualizer()
        {
            SceneView.duringSceneGui += OnSceneGUI;
            warningIcon = EditorGUIUtility.IconContent("console.warnicon");
            
            labelStyle = new GUIStyle();
            labelStyle.normal.textColor = Color.red;
            labelStyle.fontSize = 11;
            labelStyle.alignment = TextAnchor.MiddleLeft;
            labelStyle.padding = new RectOffset(2, 2, 0, 0);
            
            isEnabled = EditorPrefs.GetBool(ENABLE_KEY, true);
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            if (!isEnabled) return;
            if (Application.isPlaying) return;

            List<MissingRefInfo> missingRefs = MissingReferencesScanner.ScanAllScenes();
            if (missingRefs == null || missingRefs.Count == 0) return;

            var groupedByGO = missingRefs.GroupBy(refInfo => refInfo.gameObject);

            foreach (var group in groupedByGO)
            {
                GameObject go = group.Key;
                if (go == null) continue;

                if (!IsObjectVisible(go, sceneView.camera)) continue;

                Vector3 worldPos = go.transform.position;
                float handleSize = HandleUtility.GetHandleSize(worldPos);

                Vector3 viewportPos = sceneView.camera.WorldToViewportPoint(worldPos);
                if (viewportPos.z <= 0) continue;

                var refsList = group.ToList();
                string labelText = $"Missing: {refsList.Count} ref(s)";
                
                bool isSelected = Selection.activeGameObject == go;
                if (isSelected)
                {
                    labelText = $"Missing References:\n";
                    foreach (var refInfo in refsList.Take(3))
                    {
                        labelText += $"- {refInfo.componentTypeName}.{refInfo.fieldName}\n";
                    }
                    if (refsList.Count > 3)
                    {
                        labelText += $"... and {refsList.Count - 3} more";
                    }
                }

                Handles.BeginGUI();
                Vector2 guiPos = HandleUtility.WorldToGUIPoint(worldPos);
                
                Rect iconRect = new Rect(guiPos.x - 8f, guiPos.y - 8f, 16f, 16f);
                GUI.color = Color.red;
                GUI.DrawTexture(iconRect, warningIcon.image);
                GUI.color = Color.white;

                Vector2 labelSize = labelStyle.CalcSize(new GUIContent(labelText));
                Rect labelRect = new Rect(guiPos.x + 12f, guiPos.y - labelSize.y / 2f, labelSize.x, labelSize.y);
                
                GUI.color = new Color(0, 0, 0, 0.7f);
                GUI.DrawTexture(labelRect, Texture2D.whiteTexture);
                GUI.color = Color.white;
                
                GUI.Label(labelRect, labelText, labelStyle);
                
                Handles.EndGUI();

                if (isSelected)
                {
                    Handles.color = new Color(1f, 0f, 0f, 0.5f);
                    Handles.DrawLine(worldPos, worldPos + Vector3.up * handleSize * 0.5f);
                    Handles.color = Color.white;
                }
            }
        }

        private static bool IsObjectVisible(GameObject go, Camera camera)
        {
            if (camera == null || go == null) return false;

            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                return GeometryUtility.TestPlanesAABB(
                    GeometryUtility.CalculateFrustumPlanes(camera),
                    renderer.bounds);
            }

            Vector3 viewportPos = camera.WorldToViewportPoint(go.transform.position);
            return viewportPos.x >= 0 && viewportPos.x <= 1 && 
                   viewportPos.y >= 0 && viewportPos.y <= 1 && 
                   viewportPos.z > 0;
        }

        public static void SetEnabled(bool enabled)
        {
            isEnabled = enabled;
            EditorPrefs.SetBool(ENABLE_KEY, enabled);
            SceneView.RepaintAll();
        }

        public static bool IsEnabled()
        {
            return isEnabled;
        }
    }
}
#endif

