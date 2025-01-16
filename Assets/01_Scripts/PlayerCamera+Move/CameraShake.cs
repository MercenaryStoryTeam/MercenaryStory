using UnityEngine;
using Cinemachine;

public class CameraShake : MonoBehaviour
{
    [Header("Cinemachine Virtual Camera")]
    public CinemachineVirtualCamera virtualCamera;

    [Header("카메라 흔들림 세기")]
    public float maxAmplitude = 1.0f; 

    [Header("카메라 흔들림 지속 시간")]
    public float shakeDuration = 0f; 

    [Header("카메라 흔들리는 속도")]
    public float frequency = 10.0f; 

    // 초기 Follow Offset 저장
    private Vector3 initialFollowOffset;

    // Cinemachine Transposer 컴포넌트 참조
    private CinemachineTransposer transposer;

    void Start()
    {
        // Virtual Camera가 설정되지 않았다면 현재 오브젝트에서 찾기
        if (virtualCamera == null)
        {
            virtualCamera = GetComponent<CinemachineVirtualCamera>();
            if (virtualCamera == null)
            {
                Debug.LogError("CinemachineVirtualCamera 컴포넌트를 찾을 수 없습니다.");
                return;
            }
        }

        // Transposer 컴포넌트 가져오기
        transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        if (transposer != null)
        {
            initialFollowOffset = transposer.m_FollowOffset;
        }
        else
        {
            Debug.LogError("CinemachineTransposer 컴포넌트를 찾을 수 없습니다. Virtual Camera의 Body가 Transposer로 설정되어 있는지 확인하세요.");
        }
    }

    void Update()
    {
        if (shakeDuration > 0)
        {
            // 시간에 따른 흔들림 계산 (사인파 기반)
            float offsetY = Mathf.Sin(Time.time * frequency) * maxAmplitude;

            // 현재 Follow Offset에 Y축 흔들림 적용
            if (transposer != null)
            {
                transposer.m_FollowOffset = initialFollowOffset + new Vector3(0, offsetY, 0);
            }

            // 흔들림 지속 시간 감소
            shakeDuration -= Time.deltaTime;
        }
        else
        {
            // 흔들림이 끝난 후 초기 위치로 부드럽게 복귀
            if (transposer != null)
            {
                transposer.m_FollowOffset = Vector3.Lerp(transposer.m_FollowOffset, initialFollowOffset, Time.deltaTime * 5f);
            }
        }
    }

    /// <summary>
    /// 카메라 흔들림을 트리거하는 메서드
    /// </summary>
    /// <param name="amplitude">흔들림의 세기 (Y축 최대 이동량)</param>
    /// <param name="duration">흔들림의 지속 시간</param>
    /// <param name="freq">흔들림의 주파수 (흔들리는 속도)</param>
    public void TriggerShake(float amplitude, float duration, float freq)
    {
        maxAmplitude = amplitude;
        shakeDuration = duration;
        frequency = freq;
    }
}
