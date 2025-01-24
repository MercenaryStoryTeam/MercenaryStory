using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ZoneManager : MonoBehaviour
{
    private List<Coroutine> activePoisonEffects = new List<Coroutine>();
    private Coroutine activeSlowCoroutine;
    private float currentSlowPercentage;
    private Player player;

    private void Awake()
    {
        TryFindPlayer();
    }

    private void TryFindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.GetComponent<Player>();
            if (player == null)
            {
                Debug.LogError("플레이어 오브젝트에 Player 컴포넌트가 없습니다.");
                StartRetryingFindPlayer();
            }
        }
        else
        {
            Debug.LogWarning("태그가 'Player'인 오브젝트를 찾을 수 없습니다. 계속해서 찾습니다.");
            StartRetryingFindPlayer();
        }
    }

    private void Start()
    {
        // 추가 초기화가 필요한 경우 여기에 작성
    }

    private void StartRetryingFindPlayer()
    {
        StartCoroutine(RetryFindPlayerCoroutine());
    }

    private IEnumerator RetryFindPlayerCoroutine()
    {
        while (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.GetComponent<Player>();
                if (player != null)
                {
                    Debug.Log("플레이어 오브젝트를 성공적으로 참조했습니다.");
                    break;
                }
                else
                {
                    Debug.LogWarning("플레이어 오브젝트를 찾았으나 Player 컴포넌트가 없습니다. 계속해서 찾습니다.");
                }
            }
            else
            {
                Debug.LogWarning("태그가 'Player'인 오브젝트를 아직 찾지 못했습니다. 계속해서 찾습니다.");
            }

            yield return new WaitForSeconds(1f); // 1초마다 다시 시도
        }
    }

    public void ApplyEffect(string effectType, float effectValue, float duration)
    {
        if (player == null)
        {
            Debug.LogWarning("플레이어 오브젝트가 아직 참조되지 않았습니다. 효과를 적용할 수 없습니다.");
            return;
        }

        switch (effectType)
        {
            case "Poison":
                StartPoisonEffect(effectValue, duration);
                break;

            case "Slow":
                StartSlowEffect(effectValue, duration);
                break;

            default:
                Debug.LogWarning($"알 수 없는 효과 타입: {effectType}");
                break;
        }
    }

    private void StartPoisonEffect(float damagePerSecondPercentage, float duration)
    {
        Coroutine poisonCoroutine = null;
        poisonCoroutine = StartCoroutine(HandlePoison(damagePerSecondPercentage, duration, () => activePoisonEffects.Remove(poisonCoroutine)));
        activePoisonEffects.Add(poisonCoroutine);
    }

    private void StartSlowEffect(float slowAmountPercentage, float duration)
    {
        if (activeSlowCoroutine != null)
        {
            StopCoroutine(activeSlowCoroutine);
            activeSlowCoroutine = null;
        }

        currentSlowPercentage = slowAmountPercentage;
        UpdateMoveSpeed();
        activeSlowCoroutine = StartCoroutine(HandleSlow(slowAmountPercentage, duration));
    }

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

        currentSlowPercentage = 0f;
        UpdateMoveSpeed();
        activeSlowCoroutine = null;
    }

    private void UpdateMoveSpeed()
    {
        if (player != null)
        {
            float clampedSlow = Mathf.Clamp(currentSlowPercentage, 0, 100);
            player.moveSpeed = player.originalMoveSpeed * (1f - (clampedSlow / 100f));
            Debug.Log($"현재 적용된 슬로우 비율: {clampedSlow}%, 이동 속도: {player.moveSpeed}");
        }
    }
}
