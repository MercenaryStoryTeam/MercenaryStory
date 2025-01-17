using UnityEngine;

public class SlowZone : MonoBehaviour
{
    [Header("이동 속도 감소 비율 (%)")]
    public float slowAmountPercentage = 20f;

    [Header("초당 피해 비율 (%)")]
    public float damagePerSecondPercentage = 0.5f;

    [Header("상태 이상 지속 시간 (초)")]
    public float duration = 10f;

    private ZoneManager zoneManager;

    private void Start()
    {
        // 씬에서 ZoneManager를 찾음
        zoneManager = FindObjectOfType<ZoneManager>();
        if (zoneManager == null)
        {
            Debug.LogError("ZoneManager가 씬에 존재하지 않습니다.");
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        // 레이어가 플레이어인 객체가 트리거 영역 안에 있으면 실행
        if (collider.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            // zoneManager 있으면 실행
            if (zoneManager != null)
            {
                // ZoneManager를 통해 Slow 효과 적용
                zoneManager.ApplyEffect("Slow", slowAmountPercentage, duration);
                // ZoneManager를 통해 추가적인 Poison 효과 적용
                zoneManager.ApplyEffect("Poison", damagePerSecondPercentage, duration);
            }
        }
    }

    // 플레이어가 트리거 영역을 벗어나도 효과는 지속되므로 별도의 처리 없음
    private void OnTriggerExit(Collider collider)
    {

    }
}

// 완성
