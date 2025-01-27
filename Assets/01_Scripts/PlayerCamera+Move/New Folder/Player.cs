using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("현재 체력")]
    public float currentHp = 0f;

    [Header("최대 체력")]
    public float maxHp = 100f;

    [Header("이동 속도")]
    public float moveSpeed = 5f;

    [Header("흡혈 비율(%)")]
    public float suckBlood = 3f;

    [Header("hit 애니메이션 쿨타임(초 단위)")]
    public float hitCooldown = 6f;

    [HideInInspector]
    public float originalMoveSpeed;

    [HideInInspector]
    public List<(GameObject droppedLightLine, ItemBase droppedItem)> droppedItems =
        new List<(GameObject droppedLightLine, ItemBase droppedItem)>();

    // 골드 변경 이벤트
    public delegate void GoldChanged(float newGold);
    public event GoldChanged OnGoldChanged;

    // 무적 상태를 관리하기 위한 변수
    private bool isInvincible = false;
    private Coroutine invincibilityCoroutine = null;

    // hit 애니메이션 쿨타임 여부
    private bool isHitCooldown = false;

    private PlayerFsm playerFsm;

    private void Awake()
    {
        // 현재 오브젝트에 있는 PlayerFsm 스크립트를 한 번만 가져와 저장
        playerFsm = GetComponent<PlayerFsm>();
    }

    private void Start()
    {
        // 원래 이동 속도 저장(슬로우 존 등에서 복구 시 사용)
        originalMoveSpeed = moveSpeed;

        // Firebase에 저장된 체력을 불러와 플레이어 체력 초기화
        currentHp = FirebaseManager.Instance.CurrentUserData.user_HP;
    }

    // 체력을 HP 범위 내로 보정하고, Firebase에 반영하는 메서드
    private void UpdatePlayerHp(float newHp)
    {
        currentHp = Mathf.Clamp(newHp, 0f, maxHp);
        FirebaseManager.Instance.CurrentUserData.user_HP = currentHp;
    }

    // 흡혈 처리
    public void SuckBlood()
    {
        // 이미 체력이 가득 차 있다면 더 이상 회복하지 않음
        if (currentHp >= maxHp) return;

        float suckBloodPercentage = suckBlood / 100f;
        float healAmount = maxHp * suckBloodPercentage;

        // 계산된 체력을 UpdatePlayerHp 메서드로 전달하여 처리
        UpdatePlayerHp(currentHp + healAmount);

        Debug.Log($"흡혈 회복량: {healAmount}/현재 체력: {currentHp}/{maxHp}");
    }

    // 데미지 처리
    public void TakeDamage(float damage)
    {
        // 사운드 클립 3개 중 랜덤 재생
        string[] soundClips = { "sound_player_hit1", "sound_player_hit2", "sound_player_hit3" };
        string randomClip = soundClips[Random.Range(0, soundClips.Length)];
        SoundManager.Instance.PlaySFX(randomClip, gameObject);

        // 무적 상태인 경우 데미지 무시
        if (isInvincible)
        {
            Debug.Log("무적 상태이므로 데미지를 받지 않습니다.");
            return;
        }

        // 체력 감소 후 업데이트
        UpdatePlayerHp(currentHp - damage);
        Debug.Log($"플레이어 체력: {currentHp}/{maxHp} (받은 데미지: {damage})");

        // 체력이 0 이하라면 사망 처리
        if (currentHp <= 0)
        {
            Die();
        }
        else
        {
            // 쿨타임이 아닐 때만 hit 애니메이션 트리거
            if (!isHitCooldown)
            {
                if (playerFsm != null)
                {
                    playerFsm.TakeDamage();
                }
                else
                {
                    Debug.LogWarning("[Player] PlayerFsm 스크립트를 찾지 못했습니다.");
                }

                StartCoroutine(HitCooldownCoroutine());
            }
            else
            {
                Debug.Log("hit 애니메이션 쿨타임 중이므로 트리거하지 않습니다.");
            }
        }
    }

    // 무적 상태를 설정하는 메서드
    public void SetInvincible(bool invincible, float duration = 0f)
    {
        if (invincible)
        {
            if (!isInvincible)
            {
                invincibilityCoroutine = StartCoroutine(InvincibilityCoroutine(duration));
            }
        }
        else
        {
            if (invincibilityCoroutine != null)
            {
                StopCoroutine(invincibilityCoroutine);
                invincibilityCoroutine = null;
            }
            isInvincible = false;
        }
    }

    // 무적 상태를 처리하는 코루틴
    private IEnumerator InvincibilityCoroutine(float duration)
    {
        isInvincible = true;
        Debug.Log($"무적 상태 시작: {duration}초 동안 무적입니다.");

        yield return new WaitForSeconds(duration);

        isInvincible = false;
        invincibilityCoroutine = null;
        Debug.Log("무적 상태 종료.");
    }

    // hit 애니메이션 쿨타임 처리 코루틴
    private IEnumerator HitCooldownCoroutine()
    {
        isHitCooldown = true;
        yield return new WaitForSeconds(hitCooldown);
        isHitCooldown = false;
        Debug.Log("hit 애니메이션 쿨타임 종료.");
    }

    // 플레이어 사망 처리
    private void Die()
    {
        Debug.Log("Player Die");

        if (playerFsm != null)
        {
            playerFsm.Die();
        }
        else
        {
            Debug.LogWarning("PlayerFsm 스크립트를 찾을 수 없습니다.");
        }

        // 체력을 최대값으로 복원 후, 마을로 귀환
        UpdatePlayerHp(maxHp);

        GameManager.Instance.currentPlayerFsm.ReturnToTown();
    }

    // 골드를 사용
    public bool SpendGold(float amount)
    {
        if (FirebaseManager.Instance.CurrentUserData.user_Gold >= amount)
        {
            FirebaseManager.Instance.CurrentUserData.user_Gold -= amount;
            Debug.Log($"[Player] 골드 {amount}을 사용했습니다. 남은 골드: {FirebaseManager.Instance.CurrentUserData.user_Gold}");

            // 골드 변경 이벤트 호출
            OnGoldChanged?.Invoke(FirebaseManager.Instance.CurrentUserData.user_Gold);
            return true;
        }
        else
        {
            Debug.LogWarning($"[Player] 골드가 부족합니다. 필요: {amount}, 현재: {FirebaseManager.Instance.CurrentUserData.user_Gold}");
            return false;
        }
    }

    // 골드를 획득
    public void AddGold(float amount)
    {
        FirebaseManager.Instance.CurrentUserData.user_Gold += amount;
        Debug.Log($"[Player] 골드 {amount}을 획득했습니다. 현재 골드: {FirebaseManager.Instance.CurrentUserData.user_Gold}");

        // 골드 변경 이벤트 호출
        OnGoldChanged?.Invoke(FirebaseManager.Instance.CurrentUserData.user_Gold);
    }
}
