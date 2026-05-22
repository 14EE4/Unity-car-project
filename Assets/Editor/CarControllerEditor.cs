using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CarController))]
public class CarControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.HelpBox("Audio setup: assign the clips below to hear engine, gear shift, and handbrake sounds in play mode.", MessageType.Info);

        DrawProperty("engineStartClip", "Engine Start Clip");
        DrawProperty("engineLoopClip", "Engine Loop Clip");
        DrawProperty("gearShiftUpClip", "Gear Shift Up Clip");
        DrawProperty("gearShiftDownClip", "Gear Shift Down Clip");
        DrawProperty("handbrakeOnClip", "Handbrake On Clip");
        DrawProperty("handbrakeOffClip", "Handbrake Off Clip");

        EditorGUILayout.Space(6f);
        EditorGUILayout.LabelField("Engine Tuning", EditorStyles.boldLabel);
        DrawProperty("engineMinPitch", "Min Pitch");
        DrawProperty("engineMaxPitch", "Max Pitch");
        DrawProperty("engineMinVolume", "Min Volume");
        DrawProperty("engineMaxVolume", "Max Volume");
        DrawProperty("engineFullPitchSpeedKmh", "Full Pitch Speed (km/h)");

        EditorGUILayout.Space(10f);
        DrawDefaultInspectorExceptAudio();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawProperty(string propertyName, string label)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            EditorGUILayout.PropertyField(property, new GUIContent(label));
        }
    }

    private void DrawDefaultInspectorExceptAudio()
    {
        SerializedProperty iterator = serializedObject.GetIterator();
        bool enterChildren = true;
        while (iterator.NextVisible(enterChildren))
        {
            enterChildren = false;

            if (iterator.name == "m_Script")
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.PropertyField(iterator, true);
                }

                continue;
            }

            if (iterator.name == "engineStartClip"
                || iterator.name == "engineLoopClip"
                || iterator.name == "gearShiftUpClip"
                || iterator.name == "gearShiftDownClip"
                || iterator.name == "handbrakeOnClip"
                || iterator.name == "handbrakeOffClip"
                || iterator.name == "engineMinPitch"
                || iterator.name == "engineMaxPitch"
                || iterator.name == "engineMinVolume"
                || iterator.name == "engineMaxVolume"
                || iterator.name == "engineFullPitchSpeedKmh")
            {
                continue;
            }

            EditorGUILayout.PropertyField(iterator, true);
        }
    }
}