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
    public float rpmRiseRate = 5000f;
    public float rpmFallRate = 7000f;
    public float freeRevResponse = 1.4f;
    public float lowSpeedThrottleBlendKmh = 18f;
    public float lowSpeedThrottleAssistRPM = 450f;

    [Header("Gearing")]
    public float finalDrive = 3.5f;
    public float reverseGearRatio = 2.8f;
    public float[] forwardGearRatios = { 4.0f, 2.8f, 1.9f, 1.4f, 1.0f, 0.85f };
    public float[] gearMaxSpeeds = { 50f, 85f, 130f, 160f, 200f, 230f };
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

    private int lastGear = 0;
    private float shiftRpmCarry = -1f;
    private float shiftRpmCarryTimer = 0f;
    public float shiftRpmCarryDuration = 0.35f;

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

    public void Step(float speedKmh, float throttleInput, bool handbrakeActive, int gear, float maxTorque, WheelCollider driveLeft, WheelCollider driveRight, float deltaTime)
    {
        CurrentSpeedKmh = speedKmh;
        CurrentGearRatio = GetGearRatio(gear);
        CurrentWheelRPM = GetDrivenWheelRPM(speedKmh, driveLeft, driveRight);

        if (gear != lastGear)
        {
            float previousRatio = Mathf.Abs(GetGearRatio(lastGear));
            float currentRatio = Mathf.Abs(GetGearRatio(gear));

            if (previousRatio > 0.01f && currentRatio > 0.01f)
            {
                shiftRpmCarry = Mathf.Clamp(CurrentRPM * (currentRatio / previousRatio), idleRPM, maxRPM);
                shiftRpmCarryTimer = shiftRpmCarryDuration;
            }
            else if (gear == 0)
            {
                shiftRpmCarry = Mathf.Clamp(CurrentRPM, idleRPM, maxRPM);
                shiftRpmCarryTimer = 0.15f;
            }
            else
            {
                shiftRpmCarry = -1f;
                shiftRpmCarryTimer = 0f;
            }

            lastGear = gear;
        }

        CurrentRPM = UpdateEngineRPM(speedKmh, throttleInput, gear, CurrentWheelRPM, driveLeft, driveRight, deltaTime);
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

    public void NotifyGearChanged(int previousGear, int nextGear)
    {
        float previousRatio = Mathf.Abs(GetGearRatio(previousGear));
        float nextRatio = Mathf.Abs(GetGearRatio(nextGear));

        if (previousRatio > 0.01f && nextRatio > 0.01f)
        {
            shiftRpmCarry = Mathf.Clamp(CurrentRPM * (nextRatio / previousRatio), idleRPM, maxRPM);
            shiftRpmCarryTimer = shiftRpmCarryDuration;
            CurrentRPM = Mathf.Max(CurrentRPM, shiftRpmCarry);
        }
        else if (nextGear == 0)
        {
            shiftRpmCarry = Mathf.Clamp(CurrentRPM, idleRPM, maxRPM);
            shiftRpmCarryTimer = 0.15f;
        }
        else
        {
            shiftRpmCarry = -1f;
            shiftRpmCarryTimer = 0f;
        }

        lastGear = nextGear;
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

    private float UpdateEngineRPM(float speedKmh, float throttleInput, int gear, float wheelRPM, WheelCollider driveLeft, WheelCollider driveRight, float deltaTime)
    {
        float desiredRPM;

        if (gear == 0)
        {
            float neutralThrottle = Mathf.Clamp01(throttleInput);
            desiredRPM = Mathf.Lerp(idleRPM, maxRPM, Mathf.Pow(neutralThrottle, freeRevResponse));
        }
        else
        {
            float speedBasedRPM = GetSpeedBasedEngineRPM(speedKmh, gear);
            float lowSpeedBlend = Mathf.Clamp01(1f - (speedKmh / Mathf.Max(0.01f, lowSpeedThrottleBlendKmh)));
            float throttleAssist = lowSpeedThrottleAssistRPM * Mathf.Clamp01(throttleInput) * lowSpeedBlend;

            // In gear, RPM should primarily follow vehicle speed.
            // Throttle only adds a small low-speed assist instead of free-revving to redline.
            desiredRPM = Mathf.Max(idleRPM, speedBasedRPM + throttleAssist);

            if (shiftRpmCarryTimer > 0f && shiftRpmCarry > 0f)
            {
                desiredRPM = Mathf.Max(desiredRPM, shiftRpmCarry);
                shiftRpmCarryTimer = Mathf.Max(0f, shiftRpmCarryTimer - deltaTime);
            }
        }

        desiredRPM = Mathf.Clamp(desiredRPM, idleRPM, maxRPM);

        float rise = rpmRiseRate * Mathf.Max(0.02f, deltaTime);
        float fall = rpmFallRate * Mathf.Max(0.02f, deltaTime);
        float rate = desiredRPM >= CurrentRPM ? rise : fall;

        return Mathf.MoveTowards(CurrentRPM, desiredRPM, rate);
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

    private float GetSpeedBasedEngineRPM(float speedKmh, int gear)
    {
        float gearMaxSpeed = GetGearMaxSpeed(gear);
        if (gearMaxSpeed <= 0f)
        {
            return idleRPM;
        }

        float speedRatio = Mathf.Clamp01(speedKmh / gearMaxSpeed);
        return Mathf.Lerp(idleRPM, maxRPM, speedRatio);
    }

    public float GetGearMaxSpeed(int gear)
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
