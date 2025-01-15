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
    [Range(10f, 100f)] public float fieldOfView = 11f;
    [Range(1f, 5f)] public float damping = 3f;

    [Header("플레이어 태그")]
    [Tooltip("1.Player 태그를 가진 오브젝트를 Follow 및 LookAt 자동 할당\n2.설정이 되어야 카메라가 플레이어를 따라 이동함")]
    public string playerTag = "Player";

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

        // Player 태그를 가진 오브젝트를 Follow 및 LookAt으로 자동 할당
        // 설정이 되어야 카메라가 플레이어를 따라 이동함
        GameObject player = GameObject.FindWithTag(playerTag);
        if (player != null)
        {
            vCam.Follow = player.transform;
            vCam.LookAt = player.transform;
        }
        else
        {
            Debug.LogError($"{playerTag} 태그를 가진 오브젝트가 존재하지 않습니다.");
        }
    }

    void Start()
    {
        // Body를 Transposer로 사용 (인스펙터에서도 지정 가능)
        transposer = vCam.GetCinemachineComponent<CinemachineTransposer>();

        // vCam에 설정된 Lens FOV
        vCam.m_Lens.FieldOfView = fieldOfView;

        // 오프셋, 회전, Damping 초기 반영
        ConfigureTransposer();
        transform.rotation = Quaternion.Euler(rotationX, rotationY, rotationZ);
    }

    void LateUpdate()
    {
        // 매 프레임 값 반영
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
