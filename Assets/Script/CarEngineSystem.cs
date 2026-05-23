using UnityEngine;

public class CarEngineSystem : MonoBehaviour
{
    [Header("Engine")]
    public float idleRPM = 1000f;
    public float maxRPM = 8000f;
    public float fuelCutRPM = 7500f;
    public float rpmWarningThreshold = 7000f;
    public float lowRpmTorqueEndRPM = 2000f;
    [Range(0.1f, 1f)] public float lowRpmTorqueMultiplier = 0.45f;

    [Header("Gearing")]
    public float finalDrive = 3.5f;
    public float reverseGearRatio = 2.8f;
    public float[] forwardGearRatios = { 4.0f, 2.8f, 1.9f, 1.4f, 1.0f };
    public float[] gearMaxSpeeds = { 50f, 85f, 130f, 160f, 200f };
    public float defaultDrivenWheelRadius = 0.35f;

    [Header("Outputs")]
    public CarEngineAudio engineAudio;
    public CarRpmDisplay rpmDisplay;
    public bool updateAudio = true;
    public bool updateRpmDisplay = true;

    public float CurrentRPM { get; private set; } = 1000f;
    public bool IsFuelCutActive { get; private set; }
    public bool IsRpmWarning { get; private set; }
    public float CurrentMotorTorque { get; private set; }
    public float CurrentGearRatio { get; private set; }
    public float CurrentWheelRPM { get; private set; }
    public float CurrentSpeedKmh { get; private set; }

    private void Awake()
    {
        if (engineAudio == null)
        {
            engineAudio = GetComponent<CarEngineAudio>();
        }

        if (rpmDisplay == null)
        {
            rpmDisplay = FindFirstObjectByType<CarRpmDisplay>();
        }
    }

    public void Step(float speedKmh, float throttleInput, bool handbrakeActive, int gear, float maxTorque, WheelCollider driveLeft, WheelCollider driveRight)
    {
        CurrentSpeedKmh = speedKmh;
        CurrentGearRatio = GetGearRatio(gear);
        CurrentWheelRPM = GetDrivenWheelRPM(speedKmh, driveLeft, driveRight);

        CurrentRPM = CalculateEngineRPM(speedKmh, throttleInput, gear, CurrentWheelRPM, driveLeft, driveRight);
        CurrentRPM = Mathf.Clamp(CurrentRPM, idleRPM, maxRPM);
        IsRpmWarning = CurrentRPM >= rpmWarningThreshold;
        IsFuelCutActive = throttleInput > 0f && CurrentRPM >= fuelCutRPM;

        CurrentMotorTorque = CalculateMotorTorque(speedKmh, throttleInput, gear, maxTorque);

        if (updateAudio && engineAudio != null)
        {
            engineAudio.SetDriveState(speedKmh, throttleInput, handbrakeActive, gear, CurrentRPM);
        }

        if (updateRpmDisplay && rpmDisplay != null)
        {
            rpmDisplay.SetRPM(CurrentRPM, IsRpmWarning);
        }
    }

    public float GetGearRatio(int gear)
    {
        if (gear < 0)
        {
            return -reverseGearRatio;
        }

        if (gear == 0)
        {
            return 0f;
        }

        int index = gear - 1;
        if (index < 0 || index >= forwardGearRatios.Length)
        {
            return 0f;
        }

        return forwardGearRatios[index];
    }

    private float CalculateEngineRPM(float speedKmh, float throttleInput, int gear, float wheelRPM, WheelCollider driveLeft, WheelCollider driveRight)
    {
        if (gear == 0)
        {
            float neutralThrottle = Mathf.Clamp01(throttleInput);
            return Mathf.Lerp(idleRPM, maxRPM, neutralThrottle * 0.6f);
        }

        float wheelDrivenRPM = wheelRPM;
        if (wheelDrivenRPM <= 0f)
        {
            wheelDrivenRPM = GetWheelRPMFromSpeed(speedKmh, driveLeft, driveRight);
        }

        float motionRPM = Mathf.Abs(wheelDrivenRPM) * Mathf.Abs(GetGearRatio(gear)) * finalDrive;
        float throttleLift = throttleInput > 0f
            ? Mathf.Lerp(0f, 1200f, Mathf.Clamp01(throttleInput))
            : 0f;

        return Mathf.Max(idleRPM, motionRPM + throttleLift * 0.15f);
    }

    private float CalculateMotorTorque(float speedKmh, float throttleInput, int gear, float maxTorque)
    {
        if (throttleInput <= 0f || gear == 0)
        {
            return 0f;
        }

        float gearRatio = GetGearRatio(gear);
        float gearMaxSpeed = GetGearMaxSpeed(gear);
        float speedRatio = gearMaxSpeed > 0f ? Mathf.Clamp01(1f - (speedKmh / (gearMaxSpeed + 1f))) : 1f;

        float torqueCurveT = Mathf.InverseLerp(idleRPM, lowRpmTorqueEndRPM, CurrentRPM);
        float lowRpmFactor = Mathf.Lerp(lowRpmTorqueMultiplier, 1f, torqueCurveT);

        if (IsFuelCutActive)
        {
            return 0f;
        }

        return maxTorque * throttleInput * gearRatio * finalDrive * speedRatio * lowRpmFactor;
    }

    private float GetGearMaxSpeed(int gear)
    {
        if (gear < 0)
        {
            return 40f;
        }

        if (gear == 0)
        {
            return 0f;
        }

        int index = gear - 1;
        if (index < 0 || index >= gearMaxSpeeds.Length)
        {
            return 0f;
        }

        return gearMaxSpeeds[index];
    }

    private float GetDrivenWheelRPM(float speedKmh, WheelCollider driveLeft, WheelCollider driveRight)
    {
        float sum = 0f;
        int count = 0;

        if (driveLeft != null)
        {
            sum += Mathf.Abs(driveLeft.rpm);
            count++;
        }

        if (driveRight != null)
        {
            sum += Mathf.Abs(driveRight.rpm);
            count++;
        }

        if (count > 0)
        {
            return sum / count;
        }

        return GetWheelRPMFromSpeed(speedKmh, driveLeft, driveRight);
    }

    private float GetWheelRPMFromSpeed(float speedKmh, WheelCollider driveLeft, WheelCollider driveRight)
    {
        float radius = GetDrivenWheelRadius(driveLeft, driveRight);
        float circumference = Mathf.Max(0.01f, 2f * Mathf.PI * radius);
        float speedMps = Mathf.Abs(speedKmh) / 3.6f;
        return speedMps / circumference * 60f;
    }

    private float GetDrivenWheelRadius(WheelCollider driveLeft, WheelCollider driveRight)
    {
        if (driveLeft != null)
        {
            return driveLeft.radius;
        }

        if (driveRight != null)
        {
            return driveRight.radius;
        }

        return defaultDrivenWheelRadius;
    }
}
