using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class VirtualCameraController : MonoBehaviour
{
    [Header("ī�޶� ������ (Y=60 ����)")]
    public float offsetX = -42f;
    public float offsetZ = -42f;
    private const float offsetY = 60f;

    [Header("ī�޶� ȸ�� (Y=45 ����)")]
    public float rotationX = 45f;
    public float rotationZ = 0f;
    private const float rotationY = 45f;

    [Header("ī�޶� ����")]
    [Range(10f, 100f)] public float fieldOfView = 35f;
    [Range(1f, 5f)] public float damping = 3f;

    private CinemachineVirtualCamera vCam;
    private CinemachineTransposer transposer;

    void Awake()
    {
        vCam = GetComponent<CinemachineVirtualCamera>();
        if (!vCam)
        {
            Debug.LogError("CinemachineVirtualCamera�� �������� �ʽ��ϴ�.");
            enabled = false;
            return;
        }
    }

    void Start()
    {
        // Body�� Transposer�� ��� (�ν����Ϳ����� ���� ����)
        transposer = vCam.GetCinemachineComponent<CinemachineTransposer>();

        // vCam�� ������ Lens FOV
        vCam.m_Lens.FieldOfView = fieldOfView;

        // ������, ȸ��, Damping �ʱ� �ݿ�
        ConfigureTransposer();
        transform.rotation = Quaternion.Euler(rotationX, rotationY, rotationZ);
    }

    void LateUpdate()
    {
        // �� ������ �� �ݿ�
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

// ��
