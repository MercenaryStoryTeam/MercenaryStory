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

    private Transform followTarget; // 따라갈 대상
    private Transform lookAtTarget; // 바라볼 대상

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
        SetTargets();
        if (!followTarget || !lookAtTarget)
        {
            Debug.LogError("Player 태그를 가진 오브젝트를 찾을 수 없습니다.");
            enabled = false;
            return;
        }

        transposer = vCam.GetCinemachineComponent<CinemachineTransposer>();
        vCam.m_Lens.FieldOfView = fieldOfView;
        vCam.Follow = followTarget;
        vCam.LookAt = lookAtTarget;
        ConfigureTransposer();
        transform.rotation = Quaternion.Euler(rotationX, rotationY, rotationZ);
    }

    void LateUpdate()
    {
        if (!followTarget || !lookAtTarget)
        {
            SetTargets();
        }

        if (followTarget && lookAtTarget)
        {
            vCam.m_Lens.FieldOfView = fieldOfView;
            ConfigureTransposer();
            transform.rotation = Quaternion.Euler(rotationX, rotationY, rotationZ);
        }
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

    private void SetTargets()
    {
        if (!followTarget || !lookAtTarget)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player)
            {
                followTarget = player.transform;
                lookAtTarget = player.transform;
                vCam.Follow = followTarget;
                vCam.LookAt = lookAtTarget;
            }
        }
    }
}
