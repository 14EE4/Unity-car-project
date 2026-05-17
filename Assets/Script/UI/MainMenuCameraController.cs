using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public class MainMenuCameraController : MonoBehaviour
{
    [Tooltip("Name of the scene that should use this camera (case-sensitive)")]
    public string menuSceneName = "MainMenu";

    [Header("Background Camera Options")]
    public bool setAsBackgroundCamera = true;
    public Color backgroundColor = new Color(0.08f, 0.09f, 0.12f, 1f);
    public float backgroundDepth = -100f;

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
        Debug.Log($"MainMenuCameraController: active scene='{SceneManager.GetActiveScene().name}', menuSceneName='{menuSceneName}', shouldBeActive={shouldBeActive}");
        // Enable the camera only in the main-menu scene; otherwise keep it disabled
        cam.enabled = shouldBeActive;

        if (shouldBeActive && setAsBackgroundCamera)
        {
            // Configure as background camera: clear to solid color and place depth low
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = backgroundColor;
            cam.depth = backgroundDepth;
            // Default culling mask: everything (caller may change if needed)
        }
    }
}
