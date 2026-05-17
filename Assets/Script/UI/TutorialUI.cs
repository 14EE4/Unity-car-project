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
    public MonoBehaviour vehicleScript;
    public string gearFieldName = "currentGear";  // 소문자로 수정

    private List<TutorialStep> tutorialSteps = new List<TutorialStep>();
    private int currentStepIndex = 0;
    private bool allTutorialsCompleted = false;

    void Start()
    {
        if (tutorialText == null) tutorialText = GetComponent<TextMeshProUGUI>();
        if (tutorialText == null)
        {
            Debug.LogError("[TutorialUI] No TextMeshProUGUI found!");
            return;
        }

        InitializeTutorialSteps();
        ShowCurrentStep();
            // TextMeshPro 한글 폰트 설정
            if (tutorialText.font == null)
            {
                var font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
                if (font != null) tutorialText.font = font;
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
    }

    void InitializeTutorialSteps()
    {
        tutorialSteps.Clear();

        // Step 1: Shift to gear
        tutorialSteps.Add(new TutorialStep
        {
            message = "기어를 올리세요 (1 또는 2 키)",
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
            message = "W 키를 눌러 가속하세요",
            completionCondition = () => GetCurrentSpeed() > 5f
        });

        // Step 3: Steer
        tutorialSteps.Add(new TutorialStep
        {
            message = "마우스를 움직여 조향하세요",
            completionCondition = () => 
            {
                // Simple check: see if player has moved mouse (we can't directly check this easily, so complete after some time)
                return Time.timeSinceLevelLoad > 15f; // Auto-complete after 15 seconds
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

        var type = vehicleScript.GetType();
        var field = type.GetField("CurrentSpeed", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (field != null)
        {
            var val = field.GetValue(vehicleScript);
            return ToFloat(val);
        }

        return 0f;
    }

    int GetCurrentGear()
    {
        if (vehicleScript == null)
        {
            Debug.LogWarning("[TutorialUI] vehicleScript is null!");
            return 0;
        }

        var type = vehicleScript.GetType();
        var field = type.GetField(gearFieldName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (field != null)
        {
            var val = field.GetValue(vehicleScript);
            int gear = ToInt(val);
            Debug.Log($"[TutorialUI] Retrieved gear value: {gear} from field: {gearFieldName}");
            return gear;
        }

        Debug.LogWarning($"[TutorialUI] Could not find field: {gearFieldName}");
        return 0;
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
