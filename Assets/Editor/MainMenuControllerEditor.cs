using UnityEditor;
using UnityEngine;

// MainMenuController 전용 에디터 검사기
// - 씬에서 적절한 CanvasGroup을 찾아 `settingsPanel`/`keyGuidePanel`에 할당하는 단축 버튼을 제공합니다.
// - 런타임 생성 대신 에디터에서 참조를 정리하도록 돕습니다.
[CustomEditor(typeof(MainMenuController))]
public class MainMenuControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MainMenuController t = (MainMenuController)target;

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("씬에서 CanvasGroup을 찾아 자동 할당하거나, 할당된 객체를 핑할 수 있는 도구입니다. 인스펙터 직접 연결을 우선하세요.", MessageType.Info);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("씬에서 패널 자동 할당"))
        {
            AutoAssignPanels(t);
        }
        if (GUILayout.Button("할당된 객체 핑"))
        {
            PingAssigned(t);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("'allowRuntimeKeyGuideCreation'을 활성화하면 참조가 없을 때 런타임 생성 시도합니다. 기본적으로 비활성화하고 에디터에서 할당하세요.", MessageType.Warning);

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

        // 씬의 모든 CanvasGroup을 검색하여 이름 기반 후보를 찾습니다.
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
            Debug.LogWarning("[MainMenuControllerEditor] 씬에서 'setting'을 이름에 포함한 Settings 패널 후보를 찾지 못했습니다.");
        }

        if (foundKeyGuide != null)
        {
            t.keyGuidePanel = foundKeyGuide;
            changed = true;
            Debug.Log($"[MainMenuControllerEditor] Assigned keyGuidePanel -> {foundKeyGuide.gameObject.name}");
        }
        else
        {
            Debug.LogWarning("[MainMenuControllerEditor] 씬에서 'keyguide'를 이름에 포함한 KeyGuide 패널 후보를 찾지 못했습니다.");
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
