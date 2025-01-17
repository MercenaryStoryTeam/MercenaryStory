using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ZoneManager : MonoBehaviour
{
    // 활성화된 Poison 리스트
    private List<Coroutine> activePoisonEffects = new List<Coroutine>();

    // 슬로우 효과
    // 현재 진행 중인 슬로우 코루틴
    private Coroutine activeSlowCoroutine;

    // 현재 적용된 슬로우 비율
    private float currentSlowPercentage;  

    private Player player;

    private void Awake()
    {
        // Player 태그를 이용하여 오브젝트 자동으로 찾아서 할당
        // 목적: 현재 체력, 이동 속도 참조
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.GetComponent<Player>();
            }
            else
            {
                Debug.LogError("태그가 'Player'인 오브젝트를 찾을 수 없습니다. ZoneManager를 비활성화합니다.");
                enabled = false;
            }
        }
    }

    // 효과를 적용하는 메서드 (타입, 크기, 지속 시간)
    public void ApplyEffect(string effectType, float effectValue, float duration)
    {
        switch (effectType)
        {
            // 독
            case "Poison":
                StartPoisonEffect(effectValue, duration);
                break;

            // 슬로우
            case "Slow":
                StartSlowEffect(effectValue, duration);
                break;

            default:
                Debug.LogWarning($"알 수 없는 효과 타입: {effectType}");
                break;
        }
    }

    // 독 효과를 시작하는 메서드
    private void StartPoisonEffect(float damagePerSecondPercentage, float duration)
    {
        Coroutine poisonCoroutine = null;
        poisonCoroutine = StartCoroutine(HandlePoison(damagePerSecondPercentage, duration, () => activePoisonEffects.Remove(poisonCoroutine)));
        activePoisonEffects.Add(poisonCoroutine);
    }

    // 슬로우 효과를 시작하는 메서드
    // 기존 슬로우 효과가 있으면 중단 후 새로 적용
    private void StartSlowEffect(float slowAmountPercentage, float duration)
    {
        // 기존 슬로우 효과 중단
        if (activeSlowCoroutine != null)
        {
            StopCoroutine(activeSlowCoroutine);
            activeSlowCoroutine = null;
        }

            // 슬로우 비율 갱신
            currentSlowPercentage = slowAmountPercentage;

        // 기본 이동 속도 슬로우 처리
        UpdateMoveSpeed();

        // 새 슬로우 효과 적용
        activeSlowCoroutine = StartCoroutine(HandleSlow(slowAmountPercentage, duration));
    }

    // 독 효과를 처리하는 코루틴
    private IEnumerator HandlePoison(float damagePerSecondPercentage, float duration, System.Action onComplete)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            if (player != null)
            {
                float damage = player.maxHp * (damagePerSecondPercentage / 100f);
                player.currentHp -= damage;
                player.currentHp = Mathf.Clamp(player.currentHp, 0, player.maxHp);
                Debug.Log($"Poison 데미지: {damage}, 현재 체력: {player.currentHp}/{player.maxHp}");
            }

            Debug.Log($"Poison 남은 지속 시간: {(int)(duration - elapsedTime)}초");

            yield return new WaitForSeconds(1f);
            elapsedTime += 1f;
        }

        Debug.Log("Poison 효과가 종료되었습니다.");
        onComplete?.Invoke();
    }

    // 슬로우 효과를 처리하는 코루틴
    private IEnumerator HandleSlow(float slowAmountPercentage, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            Debug.Log($"Slow 남은 지속 시간: {(int)(duration - elapsedTime)}초");

            yield return new WaitForSeconds(1f);
            elapsedTime += 1f;
        }

        Debug.Log("Slow 효과가 종료되었습니다.");

        // 슬로우 효과가 끝났으므로 비율 초기화
        currentSlowPercentage = 0f;

        // 기본 이동 속도 회복 처리
        UpdateMoveSpeed();

        // 현재 슬로우 코루틴 제거
        activeSlowCoroutine = null;
    }

    // 기본 이동 속도 슬로우 및 회복 처리
    private void UpdateMoveSpeed()
    {
        // Player 오브젝트가 존재
        if (player != null)
        {
            // 슬로우 비율 범위 제한
            float clampedSlow = Mathf.Clamp(currentSlowPercentage, 0, 100);
            
            // 슬로우 비율 %로 변환 후
            // 이동 속도 슬로우 처리
            // 기본 이동 속도에 슬로우 처리된 이동 속도 할당
            player.moveSpeed = player.originalMoveSpeed * (1f - (clampedSlow / 100f));

            Debug.Log($"현재 적용된 슬로우 비율: {clampedSlow}%, 이동 속도: {player.moveSpeed}");
        }
    }
}

//
