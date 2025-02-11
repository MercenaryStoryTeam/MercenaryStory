using UnityEngine;
using Cinemachine;
using System.Collections;
using System.Collections.Generic;

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

    [Header("카메라 쉐이크 설정")]
    [Tooltip("쉐이크의 최소 강도 (0 ~ 0.15), 0.1 추천")]
    [Range(0f, 0.15f)] public float minShakeMagnitude = 0.1f;
    [Tooltip("쉐이크의 최대 강도 (0 ~ 0.15), 0.15 추천")]
    [Range(0f, 0.15f)] public float maxShakeMagnitude = 0.15f;

    [Header("카메라 쉐이크 방향 설정")]
    [Tooltip("X축 쉐이크 활성화")]
    public bool shakeX = true;
    [Tooltip("Y축 쉐이크 활성화")]
    public bool shakeY = true;
    [Tooltip("Z축 쉐이크 활성화")]
    public bool shakeZ = true;

    [Header("카메라 쉐이크 지속 시간")]
    public float sakeDuration = 0.5f;

    // VirtualCamera
    private CinemachineVirtualCamera vCam;

    // Follow, LookAt 자동 할당
    private Transform followTarget;
    private Transform lookAtTarget;

    // VirtualCamera Transposer
    private CinemachineTransposer transposer;

    // 카메라의 원래 오프셋 저장
    private Vector3 originalOffset;

    // 몬스터에게 데미지를 줄 때마다 발생하는 개별적인 카메라 쉐이크 효과를 담아서 관리
    private List<ShakeInstance> activeShakes = new List<ShakeInstance>();

    void Awake()
    {
        vCam = GetComponent<CinemachineVirtualCamera>();
        if (!vCam)
        {
            Debug.LogError("CinemachineVirtualCamera가 존재하지 않습니다.");
            enabled = false;
            return;
        }

        originalOffset = new Vector3(offsetX, offsetY, offsetZ);
    }

    void Start()
    {
        // 여기서도 SetTargets()를 통해 참조 세팅
        SetTargets();

        // Transposer 설정
        transposer = vCam.GetCinemachineComponent<CinemachineTransposer>();
        vCam.m_Lens.FieldOfView = fieldOfView;
        vCam.Follow = followTarget;
        vCam.LookAt = lookAtTarget;
        ConfigureTransposer();

        // 카메라 회전값 초기화
        transform.rotation = Quaternion.Euler(rotationX, rotationY, rotationZ);
    }

    void LateUpdate()
    {
        // 혹시 중간에 Player나 StageManager가 새로 갱신되었을 수도 있으니 재검사
        if (!followTarget || !lookAtTarget)
        {
            SetTargets();
        }

        // 정상적으로 타겟이 있으면 카메라 설정 반영
        if (followTarget && lookAtTarget)
        {
            vCam.m_Lens.FieldOfView = fieldOfView;
            ConfigureTransposer();
            transform.rotation = Quaternion.Euler(rotationX, rotationY, rotationZ);
        }

        // 카메라 쉐이크 처리
        if (activeShakes.Count > 0)
        {
            Vector3 totalShakeOffset = Vector3.zero;

            // 활성화된 모든 쉐이크 효과를 순회
            for (int i = activeShakes.Count - 1; i >= 0; i--)
            {
                ShakeInstance shake = activeShakes[i];
                shake.elapsed += Time.deltaTime;

                // 쉐이크가 완료되었으면 리스트에서 제거
                if (shake.elapsed > shake.duration)
                {
                    activeShakes.RemoveAt(i);
                    continue;
                }

                // 쉐이크 강도 계산 (지속 시간에 따라 점점 줄어들게)
                float remainingDuration = shake.duration - shake.elapsed;
                float currentMagnitude = shake.magnitude * (remainingDuration / shake.duration);

                // 활성화된 축에 따라 쉐이크 오프셋 계산
                float offsetXShake = shakeX ? Random.Range(-1f, 1f) * currentMagnitude : 0f;
                float offsetYShake = shakeY ? Random.Range(-1f, 1f) * currentMagnitude : 0f;
                float offsetZShake = shakeZ ? Random.Range(-1f, 1f) * currentMagnitude : 0f;

                // 총 쉐이크 오프셋에 추가
                totalShakeOffset += new Vector3(offsetXShake, offsetYShake, offsetZShake);
            }

            // 총 쉐이크 오프셋을 적용
            if (transposer != null)
            {
                transposer.m_FollowOffset = originalOffset + totalShakeOffset;
            }
        }
        else
        {
            // 활성화된 쉐이크가 없으면 원래 오프셋으로 복귀
            if (transposer != null)
            {
                transposer.m_FollowOffset = originalOffset;
            }
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

    // GameManager.Instance.currentPlayerFsm에서 플레이어를 받아 Follow/LookAt Target 세팅
    private void SetTargets()
    {
        // 1) GameManager 자체가 있는지 먼저 확인
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager.Instance가 존재하지 않습니다. 아직 초기화되지 않았거나 씬에 없습니다.");
            return;
        }

        // 2) currentPlayerFsm이 세팅되어 있는지 확인
        if (GameManager.Instance.currentPlayerFsm == null)
        {
            Debug.LogWarning("GameManager.Instance.currentPlayerFsm이 null입니다. 플레이어 FSM이 할당되지 않았습니다.");
            return;
        }

        // 3) 실제 player 오브젝트 체크
        GameObject player = GameManager.Instance.currentPlayerFsm.gameObject;
        if (player == null)
        {
            Debug.LogWarning("GameManager.Instance.currentPlayerFsm.gameObject가 null입니다. 플레이어 오브젝트를 찾을 수 없습니다.");
            return;
        }

        // 4) 정상적으로 찾았다면 Follow와 LookAt을 지정
        followTarget = player.transform;
        lookAtTarget = player.transform;

        if (vCam != null)
        {
            vCam.Follow = followTarget;
            vCam.LookAt = lookAtTarget;
        }
    }

    // 카메라 쉐이크를 발생시키는 메서드
    public void ShakeCamera(float duration)
    {
        // 설정된 범위 내에서 랜덤한 magnitude 선택
        float magnitude = Random.Range(minShakeMagnitude, maxShakeMagnitude);

        // 새로운 쉐이크 효과 추가
        activeShakes.Add(new ShakeInstance
        {
            duration = duration,
            magnitude = magnitude,
            elapsed = 0f
        });
    }

    // 쉐이크 인스턴스 클래스
    private class ShakeInstance
    {
        // 쉐이크 지속 시간
        public float duration;

        // 쉐이크 강도
        public float magnitude;

        // 경과 시간
        public float elapsed;
    }
}
