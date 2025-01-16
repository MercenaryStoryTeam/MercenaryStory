using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class VirtualCameraController : MonoBehaviour
{
    [Header("카메라 오프셋 (Y=60 고정)")]
    public float offsetX = -42f;
    public float offsetZ = -42f;
    private const float offsetY = 60f;

    [Header("카메라 회전 (Y=45 고정)")]
    public float rotationX = 45f;
    public float rotationZ = 0f;
    private const float rotationY = 45f;

    [Header("카메라 설정")]
    [Range(10f, 100f)] public float fieldOfView = 35f;
    [Range(1f, 5f)] public float damping = 3f;

    private CinemachineVirtualCamera vCam;
    private CinemachineTransposer transposer;

    void Awake()
    {
        vCam = GetComponent<CinemachineVirtualCamera>();
        if (!vCam)
        {
            Debug.LogError("CinemachineVirtualCamera가 존재하지 않습니다.");
            enabled = false;
            return;
        }
    }

    void Start()
    {
        // Body를 Transposer로 설정 (인스펙터에서도 설정 가능)
        transposer = vCam.GetCinemachineComponent<CinemachineTransposer>();

        // vCam의 기본적인 Lens FOV
        vCam.m_Lens.FieldOfView = fieldOfView;

        // 오프셋, 회전, Damping 초기 반영
        ConfigureTransposer();
        transform.rotation = Quaternion.Euler(rotationX, rotationY, rotationZ);
    }

    void LateUpdate()
    {
        // 값 변경시 즉시 반영
        vCam.m_Lens.FieldOfView = fieldOfView;
        ConfigureTransposer();
        transform.rotation = Quaternion.Euler(rotationX, rotationY, rotationZ);
    }

    void ConfigureTransposer()
    {
        if (transposer)
        {
            transposer.m_BindingMode = CinemachineTransposer.BindingMode.WorldSpace;
            transposer.m_FollowOffset = new Vector3(offsetX, offsetY, offsetZ);
            transposer.m_XDamping = damping;
            transposer.m_YDamping = damping;
            transposer.m_ZDamping = damping;
        }
    }

    void OnValidate()
    {
        if (!Application.isPlaying && vCam)
        {
            vCam.m_Lens.FieldOfView = fieldOfView;
            ConfigureTransposer();
            transform.rotation = Quaternion.Euler(rotationX, rotationY, rotationZ);
        }
    }
}