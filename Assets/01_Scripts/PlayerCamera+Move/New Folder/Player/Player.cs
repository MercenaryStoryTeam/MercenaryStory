using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Player : MonoBehaviour
{
    [Header("기본 스펙 (템플릿)")]
    public PlayerData playerData; // ScriptableObject에서 기본값 로드

    [Header("개별 플레이어 스탯")]
    public float currentHp;
    public float maxHp;
    public float moveSpeed;
    public float suckBlood;
    public float hitCooldown;
    public float gold;  // 개별 플레이어가 보유한 골드

    private bool isInvincible = false;
    private Coroutine invincibilityCoroutine = null;
    private bool isHitCooldown = false;
    private PlayerFsm playerFsm;

    public delegate void GoldChanged(float newGold);
    public event GoldChanged OnGoldChanged;

    private void Awake()
    {
        playerFsm = GetComponent<PlayerFsm>();

        // ScriptableObject의 값으로 초기화
        if (playerData != null)
        {
            maxHp = playerData.maxHp;
            moveSpeed = playerData.moveSpeed;
            suckBlood = playerData.suckBlood;
            hitCooldown = playerData.hitCooldown;
        }
        else
        {
            Debug.LogWarning("PlayerData가 설정되지 않았습니다.");
        }

        // Firebase에서 데이터를 가져와 현재 체력, 골드 초기값 설정
        currentHp = FirebaseManager.Instance.CurrentUserData.user_HP;
        gold = FirebaseManager.Instance.CurrentUserData.user_Gold;
    }

        private void Update()
        {
            // 체력 및 골드 업데이트
            currentHp = FirebaseManager.Instance.CurrentUserData.user_HP;
            gold = FirebaseManager.Instance.CurrentUserData.user_Gold;
        }

    private void UpdatePlayerHp(float newHp)
    {
        currentHp = Mathf.Clamp(newHp, 0f, maxHp);
        FirebaseManager.Instance.CurrentUserData.user_HP = currentHp;
    }

    public void SuckBlood()
    {
        if (currentHp >= maxHp) return;

        float healAmount = maxHp * (suckBlood / 100f);
        UpdatePlayerHp(currentHp + healAmount);

        Debug.Log($"흡혈 회복량: {healAmount}/현재 체력: {currentHp}/{maxHp}");
    }

    public void TakeDamage(float damage)
    {
        if (isInvincible) return;

        UpdatePlayerHp(currentHp - damage);
        Debug.Log($"플레이어 체력: {currentHp}/{maxHp} (받은 데미지: {damage})");

        if (currentHp <= 0)
        {
            Die();
        }
        else if (!isHitCooldown)
        {
            playerFsm?.TakeDamage();
            StartCoroutine(HitCooldownCoroutine());
        }
    }

    private IEnumerator HitCooldownCoroutine()
    {
        isHitCooldown = true;
        yield return new WaitForSeconds(hitCooldown);
        isHitCooldown = false;
    }

    private void Die()
    {
        Debug.Log("Player Die");

        playerFsm?.Die();
        UpdatePlayerHp(maxHp);
        GameManager.Instance.currentPlayerFsm.ReturnToTown();
    }

    // 골드 사용
    public bool SpendGold(float amount)
    {
        if (gold >= amount)
        {
            gold -= amount;
            FirebaseManager.Instance.CurrentUserData.user_Gold = gold;
            Debug.Log($"[Player] 골드 {amount} 사용. 남은 골드: {gold}");

            // 골드 변경 이벤트 호출
            OnGoldChanged?.Invoke(gold);
            return true;
        }
        else
        {
            Debug.LogWarning($"[Player] 골드 부족! 필요: {amount}, 현재: {gold}");
            return false;
        }
    }

    // 골드 획득
    public void AddGold(float amount)
    {
        gold += amount;
        FirebaseManager.Instance.CurrentUserData.user_Gold = gold;
        Debug.Log($"[Player] 골드 {amount} 획득. 현재 골드: {gold}");

        // 골드 변경 이벤트 호출
        OnGoldChanged?.Invoke(gold);
    }
}
