using UnityEditor;
using UnityEngine;

public static class ClearTutorialPref
{
    [MenuItem("Tools/Clear TutorialCompleted")]
    public static void ClearTutorialCompleted()
    {
        PlayerPrefs.DeleteKey("TutorialCompleted");
        PlayerPrefs.Save();
        Debug.Log("PlayerPrefs: Cleared TutorialCompleted");
    }
}
