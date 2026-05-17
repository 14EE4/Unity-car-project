using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public class MainMenuCameraController : MonoBehaviour
{
    [Tooltip("Name of the scene that should use this camera (case-sensitive)")]
    public string menuSceneName = "MainMenu";

    void Awake()
    {
        // If this camera was accidentally tagged as MainCamera, remove that tag so Camera.main is not affected
        if (gameObject.tag == "MainCamera")
            gameObject.tag = "Untagged";
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        UpdateCameraState();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateCameraState();
    }

    void UpdateCameraState()
    {
        var cam = GetComponent<Camera>();
        bool shouldBeActive = SceneManager.GetActiveScene().name == menuSceneName;
        // Enable the camera only in the main-menu scene; otherwise keep it disabled
        cam.enabled = shouldBeActive;
    }
}
