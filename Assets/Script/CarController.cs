using UnityEngine;

public class CarController : MonoBehaviour
{
    public WheelCollider frontLeft, frontRight;
    public WheelCollider backLeft, backRight;
    
    [Header("HUD")]
    public SpeedAndGearUI hud; // assign HUD component to receive speed/gear updates
    public SteeringIndicatorUI steeringUi; // optional: assign steering indicator UI

    public float maxTorque = 500f;   // 엔진 기본 토크 (N·m)
    public float maxSteerAngle = 45f; 
    public float steerSensitivity = 1f;
    public float steerInputMultiplier = 2f;
    public float brakeTorque = 3000f; 
    public float handbrakeTorque = 2000f;
    public float rollingResistanceBrake = 10f;
    public float engineBrakeTorque = 10f;
    public float throttleDrag = 0.03f;
    public float neutralDrag = 0.08f;
    public float driveDrag = 0.6f;
    public float brakeDrag = 1.8f;
    public float debugLogInterval = 0.25f;
    public bool detailedWheelDebug = true;

    private float currentSteer = 0f; 
    private float debugLogTimer = 0f;
    private Rigidbody carRigidbody;
    private float throttleInput = 0f;
    private float brakeInput = 0f;
    private bool handbrakeActive = false;
    private float appliedMotorTorque = 0f;
    private float appliedBrakeTorque = 0f;
    private float previousForwardSpeed = 0f;
    private float longitudinalAcceleration = 0f;
    private bool suppressDriveInputUntilRelease = false;
    private CarEngineAudio engineAudio;

    // 기어 상태: -1 = R, 0 = N, 1 이상 = 전진 기어
    public int currentGear = 0;
    private readonly float reverseGearRatio = 2.8f;
    private readonly float[] forwardGearRatios = { 4.0f, 2.8f, 1.9f, 1.4f, 1.0f };
    // 각 기어별 최고 속도 (km/h)
    private readonly float[] gearMaxSpeeds = { 50f, 85f, 130f, 160f, 200f };

    public bool EnableSpeedLogs = true; // Allows enabling/disabling logs in the Unity editor

    // 게임 초기화를 위한 초기 위치/회전 저장
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    void Start()
    {
        // 마우스 커서를 게임 화면 중앙에 고정 (정교한 조작을 위해 필수)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        // Ensure a CursorLockManager exists to enforce cursor state across scenes
        if (Object.FindFirstObjectByType<CursorLockManager>() == null)
        {
            var go = new GameObject("_CursorLockManager");
            var mgr = go.AddComponent<CursorLockManager>();
            mgr.enforceDuringPlay = true;
            mgr.dontDestroy = true;
        }
        // 차체 바닥(중앙보다 조금 아래)으로 무게 중심 강제 고정
        carRigidbody = GetComponent<Rigidbody>();
        carRigidbody.centerOfMass = new Vector3(0, -0.5f, 0);
        // 물리 기반 이동에서 카메라 끊김을 줄이기 위한 보간 설정
        carRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        previousForwardSpeed = GetForwardSpeedMps();
        // Ensure we start in neutral gear on scene start
        currentGear = 0;
        
        // 게임 초기 상태 저장 (리셋용)
        initialPosition = carRigidbody.position;
        initialRotation = carRigidbody.rotation;

        engineAudio = GetComponent<CarEngineAudio>();
        if (engineAudio == null)
        {
            Debug.LogWarning("[CarController] CarEngineAudio component is missing. Add it to this car object to enable engine sounds.");
        }
    }

    void Update()
    {
        if (suppressDriveInputUntilRelease)
        {
            throttleInput = 0f;
            brakeInput = 0f;
            handbrakeActive = false;
            currentSteer = 0f;
            appliedMotorTorque = 0f;
            appliedBrakeTorque = 0f;

            bool driveKeysReleased = !Input.GetKey(KeyCode.W)
                && !Input.GetKey(KeyCode.S)
                && !Input.GetKey(KeyCode.Space)
                && !Input.GetKey(KeyCode.Alpha1)
                && !Input.GetKey(KeyCode.Alpha2);

            if (driveKeysReleased)
            {
                suppressDriveInputUntilRelease = false;
                Debug.Log("[CarController] Drive input lock released after reset.");
            }

            if (engineAudio != null)
            {
                engineAudio.SetDriveState(GetCurrentSpeedKmh(), throttleInput, handbrakeActive);
            }

            return;
        }

        // 1. 조향 (마우스 X축: 움직일 때만 각도 변경 — 멈추면 현재 각도 유지)
        float mouseX = Input.GetAxis("Mouse X");
        if (Mathf.Abs(mouseX) > 0.0001f)
        {
            currentSteer += mouseX * steerSensitivity * steerInputMultiplier;
            currentSteer = Mathf.Clamp(currentSteer, -maxSteerAngle, maxSteerAngle);
        }

        // Update steering indicator UI (normalized -1..1)
        if (steeringUi != null)
        {
            float normalized = maxSteerAngle != 0f ? currentSteer / maxSteerAngle : 0f;
            steeringUi.SetSteer(normalized);
        }

        // 2. 입력 분리: W는 가속, S는 브레이크
        throttleInput = Input.GetKey(KeyCode.W) ? 1f : 0f;
        brakeInput = Input.GetKey(KeyCode.S) ? 1f : 0f;
        // Space: Handbrake (side / emergency brake)
        handbrakeActive = Input.GetKey(KeyCode.Space);

        // 기어 변속: 2는 업시프트, 1은 다운시프트
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ShiftUp();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ShiftDown();
        }

        if (engineAudio != null)
        {
            engineAudio.SetDriveState(GetCurrentSpeedKmh(), throttleInput, handbrakeActive);
        }

        debugLogTimer += Time.deltaTime;
        if (debugLogTimer >= debugLogInterval && EnableSpeedLogs)
        {
            debugLogTimer = 0f;
            string hb = handbrakeActive ? " | Handbrake: ON" : "";
            Debug.Log(string.Format("Speed: {0:F1} km/h | Accel: {1:F2} m/s^2 | Throttle: {2} | Brake: {3} | Gear: {4} | Motor: {5:F1} | BrakeTorque: {6:F1} | Slope: {7:F1} deg{8}", GetCurrentSpeedKmh(), longitudinalAcceleration, throttleInput > 0f ? "ON" : "OFF", brakeInput > 0f ? "ON" : "OFF", GetGearLabel(), appliedMotorTorque, appliedBrakeTorque, GetGroundSlopeAngle(), GetWheelDebugSuffix()) + hb);
        }
    }

    /// <summary>
    /// 게임 상태를 초기값으로 리셋합니다. (위치, 회전, 속도, 기어, 입력)
    /// </summary>
    public void ResetGameState()
    {
        // 물리 상태를 먼저 멈춘 다음 위치/회전을 되돌린다.
        carRigidbody.WakeUp();
        carRigidbody.linearVelocity = Vector3.zero;
        carRigidbody.angularVelocity = Vector3.zero;
        carRigidbody.position = initialPosition;
        carRigidbody.rotation = initialRotation;
        transform.SetPositionAndRotation(initialPosition, initialRotation);
        Physics.SyncTransforms();

        ResetWheelState();
        carRigidbody.linearVelocity = Vector3.zero;
        carRigidbody.angularVelocity = Vector3.zero;
        carRigidbody.Sleep();
        
        // 기어 및 입력 상태 초기화
        currentGear = 0;
        throttleInput = 0f;
        brakeInput = 0f;
        handbrakeActive = false;
        currentSteer = 0f;
        appliedMotorTorque = 0f;
        appliedBrakeTorque = 0f;
        previousForwardSpeed = GetForwardSpeedMps();
        suppressDriveInputUntilRelease = true;
        
        Debug.Log($"[CarController] Game state reset | pos={carRigidbody.position} rot={carRigidbody.rotation.eulerAngles} gear={currentGear}");
    }

    private void ResetWheelState()
    {
        if (frontLeft != null)
        {
            frontLeft.motorTorque = 0f;
            frontLeft.brakeTorque = 0f;
            frontLeft.steerAngle = 0f;
        }

        if (frontRight != null)
        {
            frontRight.motorTorque = 0f;
            frontRight.brakeTorque = 0f;
            frontRight.steerAngle = 0f;
        }

        if (backLeft != null)
        {
            backLeft.motorTorque = 0f;
            backLeft.brakeTorque = 0f;
            backLeft.steerAngle = 0f;
        }

        if (backRight != null)
        {
            backRight.motorTorque = 0f;
            backRight.brakeTorque = 0f;
            backRight.steerAngle = 0f;
        }
    }

    void FixedUpdate()
    {
        if (throttleInput > 0f)
        {
            carRigidbody.linearDamping = throttleDrag;
        }
        else if (brakeInput > 0f)
        {
            carRigidbody.linearDamping = brakeDrag;
        }
        else
        {
            carRigidbody.linearDamping = currentGear == 0 ? neutralDrag : driveDrag;
        }

        // 3. 가속 및 브레이크 로직
        // Handbrake has highest priority when active: apply strong brake on rear wheels
        if (handbrakeActive)
        {
            frontLeft.steerAngle = frontRight.steerAngle = currentSteer;
            backLeft.motorTorque = backRight.motorTorque = 0f;
            // keep a small front brake so front doesn't lock immediately
            frontLeft.brakeTorque = frontRight.brakeTorque = engineBrakeTorque;
            backLeft.brakeTorque = backRight.brakeTorque = handbrakeTorque;
            appliedMotorTorque = 0f;
            appliedBrakeTorque = handbrakeTorque;
        }
        else if (throttleInput > 0f)
        {
            // 기어별 최고 속도 제한 적용: 속도 도달 시 토크 감소
            float currentSpeedKmh = GetCurrentSpeedKmh();
            float gearMaxSpeed = GetGearMaxSpeed();
            float speedRatio = Mathf.Clamp01(1f - (currentSpeedKmh / (gearMaxSpeed + 1f)));
            float motor = maxTorque * throttleInput * GetCurrentGearRatio() * speedRatio;
            frontLeft.steerAngle = frontRight.steerAngle = currentSteer;
            backLeft.motorTorque = backRight.motorTorque = motor;
            frontLeft.brakeTorque = frontRight.brakeTorque = 0f;
            backLeft.brakeTorque = backRight.brakeTorque = 0f;
            appliedMotorTorque = motor;
            appliedBrakeTorque = 0f;
        }
        else if (brakeInput > 0f)
        {
            frontLeft.steerAngle = frontRight.steerAngle = currentSteer;
            backLeft.motorTorque = backRight.motorTorque = 0f;
            frontLeft.brakeTorque = frontRight.brakeTorque = brakeTorque;
            backLeft.brakeTorque = backRight.brakeTorque = brakeTorque;
            appliedMotorTorque = 0f;
            appliedBrakeTorque = brakeTorque;
        }
        else
        {
            // 엑셀과 브레이크 모두 떼면: 중립은 구름 저항, 전진/후진 기어는 엔진 브레이크
            frontLeft.steerAngle = frontRight.steerAngle = currentSteer;
            backLeft.motorTorque = backRight.motorTorque = 0f;
            float brakeTq = currentGear == 0 ? rollingResistanceBrake : engineBrakeTorque;
            frontLeft.brakeTorque = frontRight.brakeTorque = brakeTq;
            backLeft.brakeTorque = backRight.brakeTorque = brakeTq;
            appliedMotorTorque = 0f;
            appliedBrakeTorque = brakeTq;
        }
        longitudinalAcceleration = (GetForwardSpeedMps() - previousForwardSpeed) / Time.fixedDeltaTime;
        previousForwardSpeed = GetForwardSpeedMps();
        // Push speed and gear values to HUD if assigned (push method recommended for accuracy)
        if (hud != null)
        {
            hud.SetSpeed(GetCurrentSpeedKmh());
            hud.SetGear(currentGear);
        }
    }


    private float GetCurrentGearRatio()
    {
        if (currentGear < 0)
        {
            return -reverseGearRatio;
        }

        if (currentGear == 0)
        {
            return 0f;
        }

        int forwardGearIndex = currentGear - 1;
        if (forwardGearIndex < 0 || forwardGearIndex >= forwardGearRatios.Length)
        {
            return 0f;
        }

        return forwardGearRatios[forwardGearIndex];
    }

    private void ShiftUp()
    {
        if (currentGear < forwardGearRatios.Length)
        {
            currentGear++;
        }
    }

    private void ShiftDown()
    {
        if (currentGear > -1)
        {
            currentGear--;
        }
    }

    private float GetCurrentSpeedKmh()
    {
        if (carRigidbody == null)
        {
            return 0f;
        }

        return carRigidbody.linearVelocity.magnitude * 3.6f;
    }

    private float GetForwardSpeedMps()
    {
        if (carRigidbody == null)
        {
            return 0f;
        }

        return Vector3.Dot(carRigidbody.linearVelocity, transform.forward);
    }

    private string GetGearLabel()
    {
        if (currentGear < 0)
        {
            return "R";
        }

        if (currentGear == 0)
        {
            return "N";
        }

        return currentGear.ToString();
    }

    private float GetGroundSlopeAngle()
    {
        if (carRigidbody == null)
        {
            return 0f;
        }

        RaycastHit hit;
        Vector3 origin = carRigidbody.position + Vector3.up * 0.5f;
        if (Physics.Raycast(origin, Vector3.down, out hit, 2.0f))
        {
            return Vector3.Angle(hit.normal, Vector3.up);
        }

        return 0f;
    }

    private string GetWheelDebugSuffix()
    {
        if (!detailedWheelDebug)
        {
            return string.Empty;
        }

        return string.Format(
            " | FL rpm:{0:F0} slip:{1} | FR rpm:{2:F0} slip:{3} | BL rpm:{4:F0} slip:{5} | BR rpm:{6:F0} slip:{7}",
            frontLeft.rpm, GetWheelSlipText(frontLeft),
            frontRight.rpm, GetWheelSlipText(frontRight),
            backLeft.rpm, GetWheelSlipText(backLeft),
            backRight.rpm, GetWheelSlipText(backRight));
    }

    private float GetGearMaxSpeed()
    {
        if (currentGear < 0)
        {
            return 40f; // 후진 최고 속도
        }

        if (currentGear == 0)
        {
            return 0f; // 중립
        }

        int forwardGearIndex = currentGear - 1;
        if (forwardGearIndex < 0 || forwardGearIndex >= gearMaxSpeeds.Length)
        {
            return 0f;
        }

        return gearMaxSpeeds[forwardGearIndex];
    }

    private string GetWheelSlipText(WheelCollider wheel)
    {
        WheelHit hit;
        if (wheel.GetGroundHit(out hit))
        {
            return string.Format("F{0:F2}/S{1:F2}", hit.forwardSlip, hit.sidewaysSlip);
        }

        return "air";
    }

    private float GetCurrentSpeed()
    {
        if (carRigidbody == null)
        {
            return 0f;
        }

        return carRigidbody.linearVelocity.magnitude * 3.6f;
    }
}