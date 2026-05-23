using UnityEngine;

public class UIHelpers : MonoBehaviour
{
    [Tooltip("Scene name to load when calling LoadMainMenu")] public string mainMenuSceneName = "MainMenu";

    public void LoadMainMenu()
    {
        LoadingScreenManager.LoadScene(mainMenuSceneName);
    }
}
