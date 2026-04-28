using UnityEngine;

public class CarController : MonoBehaviour
{
    public WheelCollider frontLeft, frontRight;
    public WheelCollider backLeft, backRight;

    public float maxTorque = 1500f;   
    public float maxSteerAngle = 30f; 
    public float brakeTorque = 3000f; 
    public float rollingResistanceBrake = 10f;
    public float engineBrakeTorque = 60f;
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
    private float appliedMotorTorque = 0f;
    private float appliedBrakeTorque = 0f;

    // 기어 상태: -1 = R, 0 = N, 1 이상 = 전진 기어
    public int currentGear = 1;
    private readonly float reverseGearRatio = 2.8f;
    private readonly float[] forwardGearRatios = { 3.5f, 2.5f, 1.8f, 1.3f, 1.1f };

    void Start()
    {
        // 마우스 커서를 게임 화면 중앙에 고정 (정교한 조작을 위해 필수)
        Cursor.lockState = CursorLockMode.Locked;
        // 차체 바닥(중앙보다 조금 아래)으로 무게 중심 강제 고정
        carRigidbody = GetComponent<Rigidbody>();
        carRigidbody.centerOfMass = new Vector3(0, -0.5f, 0);
    }

    void Update()
    {
        // 1. 조향 (마우스 X축 누적)
        currentSteer += Input.GetAxis("Mouse X") * 2f; 
        currentSteer = Mathf.Clamp(currentSteer, -maxSteerAngle, maxSteerAngle);

        // 2. 입력 분리: W는 가속, S는 브레이크
        throttleInput = Input.GetKey(KeyCode.W) ? 1f : 0f;
        brakeInput = Input.GetKey(KeyCode.S) ? 1f : 0f;

        // 기어 변속: 2는 업시프트, 1은 다운시프트
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ShiftUp();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ShiftDown();
        }

        debugLogTimer += Time.deltaTime;
        if (debugLogTimer >= debugLogInterval)
        {
            debugLogTimer = 0f;
            Debug.Log(string.Format("Speed: {0:F1} km/h | Throttle: {1} | Brake: {2} | Gear: {3} | Motor: {4:F1} | BrakeTorque: {5:F1} | Slope: {6:F1} deg{7}", GetCurrentSpeedKmh(), throttleInput > 0f ? "ON" : "OFF", brakeInput > 0f ? "ON" : "OFF", GetGearLabel(), appliedMotorTorque, appliedBrakeTorque, GetGroundSlopeAngle(), GetWheelDebugSuffix()));
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
        float motor = 0f;
        float brake = 0f;

        if (throttleInput > 0f)
        {
            motor = maxTorque * throttleInput * GetCurrentGearRatio();
            brake = 0f;
        }
        else if (brakeInput > 0f)
        {
            motor = 0f;
            brake = brakeTorque;
        }
        else
        {
            // 엑셀과 브레이크 모두 떼면: 중립은 구름 저항, 전진/후진 기어는 엔진 브레이크
            motor = 0f;
            brake = currentGear == 0 ? rollingResistanceBrake : engineBrakeTorque;
        }

        // 4. 물리 값 적용
        frontLeft.steerAngle = frontRight.steerAngle = currentSteer;
        backLeft.motorTorque = backRight.motorTorque = motor;

        // 모든 바퀴에 브레이크 적용
        frontLeft.brakeTorque = frontRight.brakeTorque = brake;
        backLeft.brakeTorque = backRight.brakeTorque = brake;

        appliedMotorTorque = motor;
        appliedBrakeTorque = brake;
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

    private string GetWheelSlipText(WheelCollider wheel)
    {
        WheelHit hit;
        if (wheel.GetGroundHit(out hit))
        {
            return string.Format("F{0:F2}/S{1:F2}", hit.forwardSlip, hit.sidewaysSlip);
        }

        return "air";
    }
}