using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("현재 체력")] // UserData 공유
    public float currentHp = 0f;

    [Header("최대 체력")]
    public float maxHp = 100;

    [Header("이동 속도")]
    public float moveSpeed = 5f;

    [Header("흡혈 비율")]
    public float suckBlood = 3f;

    [Header("골드")] // UserData 공유 -> 추후에, 일단 고정
    public float gold = 0f;

    [HideInInspector] public float originalMoveSpeed;

    [HideInInspector]
    public List<(GameObject droppedLightLine, ItemBase droppedItem)> droppedItems =
        new List<(GameObject droppedLightLine, ItemBase droppedItem)>();

    // 골드 변경 이벤트
    public delegate void GoldChanged(float newGold);
    public event GoldChanged OnGoldChanged;

    // 무적 상태를 관리하기 위한 변수
    private bool isInvincible = false;
    private Coroutine invincibilityCoroutine = null;

    // SkillFsm 참조
    private SkillFsm skillFsm;

    private void Start()
    {
        // 원래 이동 속도 저장
        originalMoveSpeed = moveSpeed;

        // FirebaseManager UserData에서 현재 체력 가져오기 (현재 주석 처리됨)
        currentHp = FirebaseManager.Instance.CurrentUserData.user_HP;

        // SkillFsm 컴포넌트 가져오기
        skillFsm = GetComponent<SkillFsm>();
        if (skillFsm == null)
        {
            Debug.LogError("SkillFsm 컴포넌트를 찾을 수 없습니다.");
        }
    }

    // 흡혈 처리
    public void SuckBlood()
    {
        if (currentHp >= maxHp) return;

        float suckBloodPercentage = suckBlood / 100f;
        float healAmount = maxHp * suckBloodPercentage;

        currentHp += healAmount;
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);

        Debug.Log($"흡혈 회복량: {healAmount}/현재 체력: {currentHp}/{maxHp}");
    }

    // 데미지 처리
    public void TakeDamage(float damage)
    {
        // 사운드 클립 3개중에 랜덤 재생 
        string[] soundClips = { "sound_player_hit1", "sound_player_hit2", "sound_player_hit3" };
        string randomClip = soundClips[Random.Range(0, soundClips.Length)];
        SoundManager.Instance.PlaySFX(randomClip, gameObject);

        if (isInvincible)
        {
            Debug.Log("무적 상태이므로 데미지를 받지 않습니다.");
            return;
        }


        currentHp -= damage;
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);

        Debug.Log($"플레이어 체력: {currentHp}/{maxHp} (받은 데미지: {damage})");

        // 체력이 0 이하라면 사망 처리
        if (currentHp <= 0)
        {
            Die();
        }
        else
        {
            // 체력이 남아 있다면 PlayerFsm 실행
            PlayerFsm playerFsm = GetComponent<PlayerFsm>();
            if (playerFsm != null)
            {
                playerFsm.TakeDamage();
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

    private void Die()
    {
        Debug.Log("Player Die");

        // PlayerFsm 실행
        PlayerFsm playerFsm = GetComponent<PlayerFsm>();
        if (playerFsm != null)
        {
            // Die 상태 애니 구현
            playerFsm.Die();
        }
        else
        {
            Debug.LogWarning("PlayerFsm 스크립트를 찾을 수 없습니다.");
        }

        // 체력을 최대값으로 복원
        currentHp = maxHp;
        StageManager.Instance.currentPlayerFsm.ReturnToTown();
    }

    // 드랍된 아이템 상호작용 하는 메서드
    public void DropItemInteraction()
    {
        if (droppedItems.Count > 0)
        {
            for (int i = droppedItems.Count - 1; i >= 0; i--)
            {
                if (droppedItems[i].droppedItem == null || droppedItems[i].droppedLightLine == null)
                {
                    droppedItems.RemoveAt(i);
                    continue;
                }

                if (Vector3.Distance(transform.position, droppedItems[i].droppedLightLine.transform.position) < 3f)
                {
                    if (Input.GetKeyDown(KeyCode.E)) // E키 누르면 반경 안에 있는 아이템 인벤토리로 들어감. 테스트용 키임
                    {
                        if (droppedItems[i].droppedItem != null && droppedItems[i].droppedLightLine != null)
                        {
                            bool isDropped = InventoryManger.Instance.UpdateSlotData();
                            if (isDropped)
                            {
                                InventoryManger.Instance.AddItemToInventory(droppedItems[i].droppedItem);
                                Destroy(droppedItems[i].droppedLightLine);
                                droppedItems.RemoveAt(i);
                            }
                        }
                    }
                }
            }
        }
    }

    // 골드를 소모
    public bool SpendGold(float amount)
    {
        if (gold >= amount)
        {
            gold -= amount;
            Debug.Log($"[Player] 골드 {amount}을 사용했습니다. 남은 골드: {gold}");

            // 골드 변경 이벤트 호출
            OnGoldChanged?.Invoke(gold);
            return true;
        }
        else
        {
            Debug.LogWarning($"[Player] 골드가 부족합니다. 필요: {amount}, 현재: {gold}");
            return false;
        }
    }

    // 골드를 추가
    public void AddGold(float amount)
    {
        gold += amount;
        Debug.Log($"[Player] 골드 {amount}을 획득했습니다. 현재 골드: {gold}");

        // 골드 변경 이벤트 호출
        OnGoldChanged?.Invoke(gold);
    }
}