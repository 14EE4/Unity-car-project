using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class TutorialUI : MonoBehaviour
{
    [System.Serializable]
    public class TutorialStep
    {
        public string message;
        public System.Func<bool> completionCondition;
        public bool completed = false;
    }

    [Header("UI Elements")]
    public TextMeshProUGUI tutorialText;

    [Header("Vehicle Source")]
    [Tooltip("Assign your CarController component here (required).")]
    public CarController vehicleScript;
    public string gearFieldName = "currentGear";  // 소문자로 수정

    private List<TutorialStep> tutorialSteps = new List<TutorialStep>();
    private int currentStepIndex = 0;
    private bool allTutorialsCompleted = false;
    private float uiDebugTimer = 0f;

    void Start()
    {
        if (tutorialText == null) tutorialText = GetComponent<TextMeshProUGUI>();
        if (tutorialText == null)
        {
            Debug.LogError("[TutorialUI] No TextMeshProUGUI found!");
            return;
        }

        // Do NOT auto-assign: require manual inspector assignment of CarController
        if (vehicleScript == null)
        {
            Debug.LogWarning("[TutorialUI] vehicleScript is not assigned. Please assign your CarController in the inspector. Tutorial will not progress until assigned.");
            return;
        }

        InitializeTutorialSteps();
        ShowCurrentStep();

        // TextMeshPro 폰트 설정: 우선 리소스에서 시도하고, 없으면 TMP_Settings의 기본 폰트를 사용
        if (tutorialText.font == null)
        {
            var font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            if (font != null)
            {
                tutorialText.font = font;
            }
            else if (TMP_Settings.defaultFontAsset != null)
            {
                tutorialText.font = TMP_Settings.defaultFontAsset;
                Debug.Log("[TutorialUI] Assigned TMP_Settings.defaultFontAsset as fallback font.");
            }
        }
    }

    void Update()
    {
        if (allTutorialsCompleted) return;

        var currentStep = tutorialSteps[currentStepIndex];
        
        // Check completion condition
        if (!currentStep.completed && currentStep.completionCondition != null && currentStep.completionCondition.Invoke())
        {
            currentStep.completed = true;
            Debug.Log($"[TutorialUI] Tutorial step {currentStepIndex} completed!");
            
            // Move to next step
            currentStepIndex++;
            if (currentStepIndex >= tutorialSteps.Count)
            {
                allTutorialsCompleted = true;
                HideTutorial();
            }
            else
            {
                ShowCurrentStep();
            }
        }

        // Periodic debug: print current gear and active tutorial step every 0.5s
        uiDebugTimer += Time.deltaTime;
        if (uiDebugTimer >= 0.5f)
        {
            uiDebugTimer = 0f;
            Debug.Log($"[TutorialUI] Active step: {currentStepIndex}, Message: {tutorialSteps[currentStepIndex].message}, Gear: {GetCurrentGear()}");
        }
    }

    void InitializeTutorialSteps()
    {
        tutorialSteps.Clear();

        // Step 1: Shift up (any forward gear)
        tutorialSteps.Add(new TutorialStep
        {
            message = "Shift up (press 2)",
            completionCondition = () => 
            {
                int gear = GetCurrentGear();
                Debug.Log($"[TutorialUI] Current gear: {gear}");
                return gear > 0;
            }
        });
        // Step 2: Accelerate
        tutorialSteps.Add(new TutorialStep
        {
            message = "Press W to accelerate",
            completionCondition = () => GetCurrentSpeed() > 5f
        });

        // Step 3: Steer
        tutorialSteps.Add(new TutorialStep
        {
            message = "Move mouse to steer",
            completionCondition = () =>
            {
                // Complete when the player moves the mouse (Mouse X or Mouse Y)
                float mx = Input.GetAxis("Mouse X");
                float my = Input.GetAxis("Mouse Y");
                return Mathf.Abs(mx) > 0.01f || Mathf.Abs(my) > 0.01f;
            }
        });
    }

    void ShowCurrentStep()
    {
        if (currentStepIndex < tutorialSteps.Count)
        {
            tutorialText.text = tutorialSteps[currentStepIndex].message;
            tutorialText.alpha = 1f;
            Debug.Log($"[TutorialUI] Showing step {currentStepIndex}: {tutorialSteps[currentStepIndex].message}");
        }
    }

    void HideTutorial()
    {
        tutorialText.alpha = 0f;
        Debug.Log("[TutorialUI] All tutorial steps completed!");
    }

    float GetCurrentSpeed()
    {
        if (vehicleScript == null) return 0f;

        var rb = vehicleScript.GetComponent<Rigidbody>();
        if (rb != null) return rb.linearVelocity.magnitude * 3.6f;

        return 0f;
    }

    int GetCurrentGear()
    {
        if (vehicleScript == null)
        {
            Debug.LogWarning("[TutorialUI] vehicleScript is null!");
            return 0;
        }
        Debug.Log($"[TutorialUI] Read gear directly from CarController.currentGear: {vehicleScript.currentGear}");
        return vehicleScript.currentGear;
    }

    float ToFloat(object o)
    {
        if (o == null) return 0f;
        if (o is float) return (float)o;
        if (o is double) return (float)(double)o;
        if (o is int) return (int)o;
        float res;
        if (float.TryParse(o.ToString(), out res)) return res;
        return 0f;
    }

    int ToInt(object o)
    {
        if (o == null) return 0;
        if (o is int) return (int)o;
        if (o is float) return Mathf.RoundToInt((float)o);
        int res;
        if (int.TryParse(o.ToString(), out res)) return res;
        return 0;
    }
}
