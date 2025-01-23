using System.Collections;
using UnityEngine;

public class HitStop : MonoBehaviour
{
    [Header("정지 상태 지속 시간")]
    [Tooltip("0.03 추천")]
    public float duration = 0.03f;

    // 히트스톱 상태 확인
    private bool isHitStopping = false;

    // 원래 fixedDeltaTime 저장 -> 나중에 원래 시간 복원할 때 사용된다.
    private float originalFixedDeltaTime; 

    private void Awake()
    {
        // 원래 fixedDeltaTime 저장
        originalFixedDeltaTime = Time.fixedDeltaTime;
    }

    // 플레이어가 몬스터에게 데미지를 줄 때마다 호출
    public void TriggerHitStop()
    {
        // isHitStopping이 false일 경우 실행
        if (!isHitStopping)
        {
            Debug.Log("히트스톱 트리거됨");

            // 정지 상태를 구현 해주는 코루틴 실행
            StartCoroutine(ApplyHitStop());
        }
        else
        {
            Debug.Log("현재 히트스톱 중입니다.");
        }
    }

    // IEnumerator는 C#에서 코루틴을 구현할 때 사용하는 인터페이스
    private IEnumerator ApplyHitStop()
    {
        isHitStopping = true;

        // timeScale -> 게임의 시간 흐름의 속도
        // 시간 정지 -> 애니 동작에도 영향을 줌
        Time.timeScale = 0f;

        // fixedDeltaTime -> 움직임과 관련된 시간 개념
        //게임 시간(Time.timeScale)에 따라 물리 움직임의 속도가 달라지도록 조정
        Time.fixedDeltaTime = originalFixedDeltaTime * Time.timeScale;

        Debug.Log($"히트스톱 적용됨: Duration={duration}초");

        // 실시간 시간을 기준으로 duration(초)만큼 대기한 뒤
        // 코루틴의 다음 단계 진행
        yield return new WaitForSecondsRealtime(duration);

        // 시간 복구
        Time.timeScale = 1f;

        // 물리 움직임 복구
        Time.fixedDeltaTime = originalFixedDeltaTime;

        Debug.Log("히트스톱 종료됨");

        isHitStopping = false;
    }
}
