using UnityEngine;

public class CarController : MonoBehaviour
{
    public WheelCollider frontLeft, frontRight;
    public WheelCollider backLeft, backRight;

    public float maxTorque = 1500f;   
    public float maxSteerAngle = 30f; 
    public float brakeTorque = 3000f; 

    private float currentSteer = 0f; 

    // [추가된 부분] 기어 변속 관련
    public int currentGear = 1; 
    private float[] gearRatios = { 0f, 3.5f, 2.5f, 1.8f, 1.3f, 1.1f }; // 기어비

    void Start()
    {
        // 마우스 커서를 게임 화면 중앙에 고정 (정교한 조작을 위해 필수)
        Cursor.lockState = CursorLockMode.Locked;
        // 차체 바닥(중앙보다 조금 아래)으로 무게 중심 강제 고정
        GetComponent<Rigidbody>().centerOfMass = new Vector3(0, -0.5f, 0);
    }

    void Update()
    {
        // 1. 조향 (마우스 X축 누적)
        currentSteer += Input.GetAxis("Mouse X") * 2f; 
        currentSteer = Mathf.Clamp(currentSteer, -maxSteerAngle, maxSteerAngle);

        // 2. 입력 받기 (이 부분에서 motorInput을 선언합니다)
        float motorInput = Input.GetAxis("Vertical"); 

        // 3. 가속 및 브레이크 로직 (가속 안 할 때 차 밀림 방지)
        float motor = 0f;
        float brake = 0f;

        // 가속 페달(W)을 밟을 때
        if (Mathf.Abs(motorInput) > 0.1f) 
        {
            motor = maxTorque * motorInput * gearRatios[currentGear];
            brake = 0f;
        }
        else 
        {
            // 페달을 뗐을 때 (가만히 있을 때 앞으로 가는 현상 방지)
            motor = 0f;
            brake = 100f; // 미세하게 브레이크를 걸어서 굴러가지 않게 함
        }

        // S 키를 눌러 직접 브레이크를 밟을 때
        if (Input.GetKey(KeyCode.S)) 
        {
            brake = brakeTorque;
        }

        // 4. 물리 값 적용
        frontLeft.steerAngle = frontRight.steerAngle = currentSteer;
        
        backLeft.motorTorque = backRight.motorTorque = motor;
        
        // 모든 바퀴에 브레이크 적용
        frontLeft.brakeTorque = frontRight.brakeTorque = brake;
        backLeft.brakeTorque = backRight.brakeTorque = brake;

        // 기어 변속 (숫자 1, 2)
        if (Input.GetKeyDown(KeyCode.Alpha2) && currentGear < gearRatios.Length - 1) currentGear++;
        if (Input.GetKeyDown(KeyCode.Alpha1) && currentGear > 1) currentGear--;
    }
}