using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraController : MonoBehaviour
{
    public Transform target; // 차량 루트
    public Transform firstPersonAnchor; // 1인칭 카메라 위치
    public Vector3 thirdPersonOffset = new Vector3(0f, 2f, -4f);
    public float mouseSensitivity = 1f;
    public float smoothTime = 0.02f;
    public bool startFirstPerson = true;
    public KeyCode toggleKey = KeyCode.C;
    public float minPitch = -20f;
    public float maxPitch = 60f;
    public float collisionRadius = 0.2f;
    public float collisionOffset = 0.2f;

    // 새 옵션: 3인칭에서 yaw(수평 회전)를 차량에 고정할지 여부
    public bool lockThirdPersonYaw = true;
    public bool lockThirdPersonPitch = false;
    // 새 옵션: 1인칭에서 마우스 룩 허용 여부 (기본: 비허용 => Anchor 고정)
    public bool allowFirstPersonMouseLook = true;

    [Header("Initial Camera Position (stored from Main scene)")]
    public Vector3 initialPosition = new Vector3(-0.814f, 0.961f, -30.139f);
    public Vector3 initialRotation = new Vector3(0f, 45f, 0f);

    float yaw = 0f;
    float pitch = 10f;
    bool firstPerson;
    Vector3 currentVel;

    void Start()
    {
        firstPerson = true; // force start in first-person by default
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (target == null)
        {
            Debug.LogWarning("CameraController: target not assigned.");
        }
        // 프리팹 자식에 카메라가 붙어있다면 런타임에 분리 (권장)
        transform.SetParent(null);

        // Reset camera to initial position when Main scene loads
        ResetCameraToInitial();

        // Ensure a firstPersonAnchor exists under the target so 1st-person can attach
        if (firstPersonAnchor == null && target != null)
        {
            var existing = target.Find("firstPersonAnchor");
            if (existing != null) firstPersonAnchor = existing;
            else
            {
                var go = new GameObject("firstPersonAnchor");
                go.transform.SetParent(target, false);
                go.transform.localPosition = new Vector3(0f, 1.2f, 0.6f);
                go.transform.localRotation = Quaternion.identity;
                firstPersonAnchor = go.transform;
            }
        }
        if (target != null)
            yaw = target.eulerAngles.y;
        }

    void ResetCameraToInitial()
    {
        transform.position = initialPosition;
        transform.eulerAngles = initialRotation;
        
        // Set initial pitch/yaw from rotation
        pitch = initialRotation.x;
        yaw = initialRotation.y;
    }
    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            firstPerson = !firstPerson;

        if (firstPerson)
        {
            if (allowFirstPersonMouseLook)
            {
                // 1인칭에서 마우스 룩 허용
                yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
                pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            }
            else
            {
                // 1인칭: 카메라를 차량 Anchor에 고정하기 위해 마우스 룩을 무시
                if (firstPersonAnchor != null)
                {
                    yaw = firstPersonAnchor.eulerAngles.y;
                    pitch = firstPersonAnchor.eulerAngles.x;
                }
                else if (target != null)
                {
                    yaw = target.eulerAngles.y;
                }
            }
        }
        else
        {
            // 3인칭: 옵션에 따라 yaw를 차량에 고정하거나 마우스로 회전 허용
            if (target != null && lockThirdPersonYaw)
                yaw = target.eulerAngles.y;
            else
                yaw += Input.GetAxis("Mouse X") * mouseSensitivity;

            if (!lockThirdPersonPitch)
                pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        }

        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
    }

    void LateUpdate()
    {
        if (target == null) return;

        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);

        if (firstPerson && firstPersonAnchor != null)
        {
            // 1인칭에서는 Anchor의 자식으로 만들어 위치/회전을 정확히 고정
            if (transform.parent != firstPersonAnchor)
            {
                transform.SetParent(firstPersonAnchor);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
            }
            currentVel = Vector3.zero;
        }
        else
        {
            // 1인칭에서 벗어나면 부모 해제
            if (transform.parent == firstPersonAnchor)
                transform.SetParent(null);

            Vector3 desiredPos = target.position + rot * thirdPersonOffset;
            Vector3 origin = target.position + Vector3.up * 1f;
            Vector3 dir = desiredPos - origin;
            float distance = dir.magnitude;
            RaycastHit hit;
            if (Physics.SphereCast(origin, collisionRadius, dir.normalized, out hit, distance))
            {
                desiredPos = hit.point - dir.normalized * collisionOffset;
            }

            transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref currentVel, smoothTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, 25f * Time.deltaTime);
        }
    }

    public void SetFirstPersonAnchor(Transform anchor)
    {
        firstPersonAnchor = anchor;
    }
}
