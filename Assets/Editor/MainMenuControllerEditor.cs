using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MainMenuController))]
public class MainMenuControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MainMenuController t = (MainMenuController)target;

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Use the buttons below to auto-assign UI CanvasGroup references from the current scene. Prefer assigning references in the inspector for stable behavior.", MessageType.Info);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Auto-assign Panels from Scene"))
        {
            AutoAssignPanels(t);
        }
        if (GUILayout.Button("Ping Assigned Objects"))
        {
            PingAssigned(t);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("If you enable 'allowRuntimeKeyGuideCreation' the controller may create a KeyGuide at runtime when no reference is assigned. Prefer leaving it disabled and assign the reference in the scene.", MessageType.Warning);

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }

    private void AutoAssignPanels(MainMenuController t)
    {
        // Find possible settings panel
        CanvasGroup foundSettings = null;
        CanvasGroup foundKeyGuide = null;

        foreach (var cg in Object.FindObjectsOfType<CanvasGroup>())
        {
            var name = cg.gameObject.name.ToLower();
            if (foundSettings == null && name.Contains("setting"))
            {
                foundSettings = cg;
            }
            if (foundKeyGuide == null && (name.Contains("keyguide") || name.Contains("key_guide") || name.Contains("key guide")))
            {
                foundKeyGuide = cg;
            }
        }

        Undo.RecordObject(t, "Auto-assign MainMenuController panels");
        bool changed = false;

        if (foundSettings != null)
        {
            t.settingsPanel = foundSettings;
            changed = true;
            Debug.Log($"[MainMenuControllerEditor] Assigned settingsPanel -> {foundSettings.gameObject.name}");
        }
        else
        {
            Debug.LogWarning("[MainMenuControllerEditor] No candidate Settings panel (CanvasGroup with 'setting' in name) found in scene.");
        }

        if (foundKeyGuide != null)
        {
            t.keyGuidePanel = foundKeyGuide;
            changed = true;
            Debug.Log($"[MainMenuControllerEditor] Assigned keyGuidePanel -> {foundKeyGuide.gameObject.name}");
        }
        else
        {
            Debug.LogWarning("[MainMenuControllerEditor] No candidate KeyGuide panel (CanvasGroup with 'keyguide' in name) found in scene.");
        }

        if (changed)
        {
            EditorUtility.SetDirty(t);
        }
    }

    private void PingAssigned(MainMenuController t)
    {
        if (t.settingsPanel != null)
        {
            EditorGUIUtility.PingObject(t.settingsPanel.gameObject);
            Debug.Log($"Pinged settingsPanel: {t.settingsPanel.gameObject.name}");
        }
        if (t.keyGuidePanel != null)
        {
            EditorGUIUtility.PingObject(t.keyGuidePanel.gameObject);
            Debug.Log($"Pinged keyGuidePanel: {t.keyGuidePanel.gameObject.name}");
        }
    }
}
