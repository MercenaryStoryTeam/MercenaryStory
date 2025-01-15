using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ZoneManager : MonoBehaviour
{
    // 활성화된 독 효과의 리스트
    private List<Coroutine> activePoisonEffects = new List<Coroutine>();

    // 활성화된 슬로우 효과의 리스트
    private List<float> activeSlowAmounts = new List<float>();

    // 슬로우 효과의 총 합산 비율
    private float totalSlowPercentage = 0;

    // 효과를 적용하는 메서드
    // 괄호안의 내용: 타입, 크기, 지속 시간 처리
    public void ApplyEffect(string effectType, float effectValue, float duration)
    {
        switch (effectType)
        {
            // 문자열과 일치할 경우 실행할 코드
            // 독
            case "Poison":
                // StartPoisonEffect 메서드를 호출
                // 메서드 호출시 괄호안의 인수를 메서드에 전달
                StartPoisonEffect(effectValue, duration);
                break;
            
            // 슬로우
            case "Slow":
                // StartSlowEffect 메서드를 호출
                // 메서드 호출시 괄호안의 인수를 메서드에 전달
                StartSlowEffect(effectValue, duration);
                break;

            // switch 문에서 지정된 모든 case에 해당하지 않는 값이 들어왔을 때 실행
            // 따라서 PoisonZone, SlowZone 스크립트에서 "Poison", Slow" 문자열 입력 제대로 해야됨
            default:
                Debug.LogWarning($"알 수 없는 효과 타입: {effectType}");
                break;
        }
    }

    // 독 효과를 시작하는 메서드
    private void StartPoisonEffect(float damagePerSecondPercentage, float duration)
    {
        Coroutine poisonCoroutine = null;
        // 독 효과를 처리하는 HandlePoison 코루틴이 종료되면 콜백을 통해 리스트에서 제거
        poisonCoroutine = StartCoroutine(HandlePoison(damagePerSecondPercentage, duration, () => activePoisonEffects.Remove(poisonCoroutine)));
        activePoisonEffects.Add(poisonCoroutine);
    }

    // 슬로우 효과를 시작하는 메서드
    private void StartSlowEffect(float slowAmountPercentage, float duration)
    {
        // 슬로우 비율 추가
        activeSlowAmounts.Add(slowAmountPercentage);
        UpdateMoveSpeed();

        // 슬로우 효과를 처리하는 HandleSlow 코루틴 시작
        Coroutine slowCoroutine = StartCoroutine(HandleSlow(slowAmountPercentage, duration));
        // 슬로우 효과가 끝나면 리스트에서 제거하고 이동 속도 회복
        StartCoroutine(RemoveSlowAfterDuration(slowAmountPercentage, duration, slowCoroutine));
    }

    // 독 효과를 처리하는 코루틴
    private IEnumerator HandlePoison(float damagePerSecondPercentage, float duration, System.Action onComplete)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            PlayerData player = PlayerData.Instance;
            if (player != null)
            {
                // 최대 체력의 비율에 따른 데미지 계산
                float damage = player.maxHp * (damagePerSecondPercentage / 100f);
                player.currentHp -= damage;
                player.currentHp = Mathf.Clamp(player.currentHp, 0, player.maxHp);
                Debug.Log($"Poison 데미지: {damage}, 현재 체력: {player.currentHp}/{player.maxHp}");
            }

            // 남은 지속 시간을 정수로 출력
            Debug.Log($"Poison 남은 지속 시간: {(int)(duration - elapsedTime)}초");

            yield return new WaitForSeconds(1f);
            elapsedTime += 1f;
        }

        // 독 효과 종료 시 로그 출력 및 콜백 호출
        Debug.Log("Poison 효과가 종료되었습니다.");
        onComplete?.Invoke();
    }

    // 슬로우 효과를 처리하는 코루틴
    private IEnumerator HandleSlow(float slowAmountPercentage, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // 남은 지속 시간을 정수로 디버그 출력
            Debug.Log($"Slow 남은 지속 시간: {(int)(duration - elapsedTime)}초");

            yield return new WaitForSeconds(1f);
            elapsedTime += 1f;
        }

        Debug.Log("Slow 효과가 종료되었습니다.");
    }

    // 슬로우 효과가 끝난 후 리스트에서 제거하고 이동 속도 회복
    private IEnumerator RemoveSlowAfterDuration(float slowAmountPercentage, float duration, Coroutine slowCoroutine)
    {
        yield return new WaitForSeconds(duration);

        // 슬로우 비율 제거
        activeSlowAmounts.Remove(slowAmountPercentage);

        UpdateMoveSpeed();
        Debug.Log($"Slow 효과 ({slowAmountPercentage}%)가 제거되었습니다.");
    }

    // 현재 활성화된 슬로우 효과를 합산하여 이동 속도 처리
    // 회복: 슬로우 효과가 끝나면 리스트에서 슬로우 비율이 제거된 상태에서 이동 속도 처리
    private void UpdateMoveSpeed()
    {
        PlayerData player = PlayerData.Instance;
        if (player != null)
        {
            // 목적: 총 슬로우 비율 계산 (합산, 최대 100%)
            totalSlowPercentage = 0;

            // activeSlowAmounts 리스트에 저장된 모든 슬로우 비율
            // 루프 돌면서 slow 변수에 할당
            foreach (float slow in activeSlowAmounts)
            {
                // 리스트에 저장된 슬로우 비율을 누적
                totalSlowPercentage += slow;
            }

            // 슬로우 비율 범위 제한 0과 100사이
            totalSlowPercentage = Mathf.Clamp(totalSlowPercentage, 0, 100);

            // 슬로우 적용
            player.moveSpeed = player.originalMoveSpeed * (1f - (totalSlowPercentage / 100f));
            Debug.Log($"현재 적용된 슬로우 비율: {totalSlowPercentage}%, 이동 속도: {player.moveSpeed}");
        }
    }
}

// 완성
