using UnityEngine;

public class TutorialDebugUI : MonoBehaviour
{
    public bool showOnStart = true;
    public Rect windowRect = new Rect(10, 10, 280, 140);
    bool visible = false;

    void Start()
    {
        visible = showOnStart;
    }

    void OnGUI()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (!visible)
        {
            if (GUI.Button(new Rect(10, 10, 160, 28), "Show Tutorial Status")) visible = true;
            return;
        }

        windowRect = GUI.Window(123456, windowRect, DrawWindow, "Tutorial Debug");
#endif
    }

    void DrawWindow(int id)
    {
        int val = PlayerPrefs.GetInt("TutorialCompleted", 0);
        GUILayout.Label("TutorialCompleted: " + val);

        if (GUILayout.Button("Clear TutorialCompleted"))
        {
            PlayerPrefs.DeleteKey("TutorialCompleted");
            PlayerPrefs.Save();
            Debug.Log("TutorialDebugUI: Cleared TutorialCompleted");
        }

        if (GUILayout.Button("Log TutorialUI status"))
        {
            var tut = FindObjectOfType<TutorialUI>();
            if (tut == null)
            {
                Debug.Log("TutorialDebugUI: No TutorialUI instance found in scene.");
            }
            else
            {
                Debug.Log($"TutorialDebugUI: TutorialUI found. vehicleScript assigned: {tut.vehicleScript != null}");
                if (tut.vehicleScript != null)
                {
                    Debug.Log($"TutorialDebugUI: CarController.currentGear = {tut.vehicleScript.currentGear}");
                }
            }
        }

        if (GUILayout.Button("Close")) visible = false;

        GUI.DragWindow(new Rect(0, 0, 10000, 20));
    }
}
