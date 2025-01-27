using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
	[Header("현재 체력")] // UserData 공유
	public float currentHp = 0f;

	[Header("최대 체력")] public float maxHp = 100;

	[Header("이동 속도")] public float moveSpeed = 5f;

	[Header("흡혈 비율")] public float suckBlood = 3f;

	[Header("hit 애니메이션 쿨타임 시간 (초 단위)")] public float hitCooldown = 6f;

	[HideInInspector] public float originalMoveSpeed;

	[HideInInspector]
	public List<(GameObject droppedLightLine, ItemBase droppedItem)>
		droppedItems =
			new List<(GameObject droppedLightLine, ItemBase droppedItem)>();

	// 골드 변경 이벤트
	public delegate void GoldChanged(float newGold);

	public event GoldChanged OnGoldChanged;

	// 무적 상태를 관리하기 위한 변수
	private bool isInvincible = false;
	private Coroutine invincibilityCoroutine = null;

	// SkillFsm 참조
	private SkillFsm skillFsm;

	// 히트 상태 쿨다운을 관리하기 위한 변수
	private bool isHitCooldown = false; // hit 애니메이션 쿨타임 여부

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
		FirebaseManager.Instance.CurrentUserData.user_HP = currentHp;
	}

	// 데미지 처리
	public void TakeDamage(float damage)
	{
		// 사운드 클립 3개 중 랜덤 재생 
		string[] soundClips =
			{ "sound_player_hit1", "sound_player_hit2", "sound_player_hit3" };
		string randomClip = soundClips[Random.Range(0, soundClips.Length)];
		SoundManager.Instance.PlaySFX(randomClip, gameObject);

		if (isInvincible)
		{
			Debug.Log("무적 상태이므로 데미지를 받지 않습니다.");
			return;
		}

		// 데미지 항상 적용
		currentHp -= damage;
		currentHp = Mathf.Clamp(currentHp, 0, maxHp);

		Debug.Log($"플레이어 체력: {currentHp}/{maxHp} (받은 데미지: {damage})");
		FirebaseManager.Instance.CurrentUserData.user_HP = currentHp;

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
				// hit 애니메이션 트리거
				PlayerFsm playerFsm = GetComponent<PlayerFsm>();
				if (playerFsm != null)
				{
					playerFsm.TakeDamage();
				}

				// 쿨타임 시작
				StartCoroutine(HitCooldownCoroutine());
			}
			else
			{
				Debug.Log("hit 애니메이션 쿨타임 중입니다. 애니메이션을 트리거하지 않습니다.");
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
				invincibilityCoroutine =
					StartCoroutine(InvincibilityCoroutine(duration));
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
		isHitCooldown = true; // 쿨타임 시작
		yield return new WaitForSeconds(hitCooldown); // 쿨타임 시간 대기
		isHitCooldown = false; // 쿨타임 종료
		Debug.Log("hit 애니메이션 쿨타임 종료.");
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
		FirebaseManager.Instance.CurrentUserData.user_HP = currentHp;
		SceneManager.Instance.currentPlayerFsm.ReturnToTown();
	}

	// 골드를 소모
	public bool SpendGold(float amount)
	{
		if (FirebaseManager.Instance.CurrentUserData.user_Gold >= amount)
		{
			FirebaseManager.Instance.CurrentUserData.user_Gold -= amount;
			Debug.Log(
				$"[Player] 골드 {amount}을 사용했습니다. 남은 골드: {FirebaseManager.Instance.CurrentUserData.user_Gold}");

			// 골드 변경 이벤트 호출
			OnGoldChanged?.Invoke(FirebaseManager.Instance.CurrentUserData.user_Gold);
			return true;
		}
		else
		{
			Debug.LogWarning(
				$"[Player] 골드가 부족합니다. 필요: {amount}, 현재: {FirebaseManager.Instance.CurrentUserData.user_Gold}");
			return false;
		}
	}

	// 골드를 추가
	public void AddGold(float amount)
	{
		FirebaseManager.Instance.CurrentUserData.user_Gold += amount;
		Debug.Log(
			$"[Player] 골드 {amount}을 획득했습니다. 현재 골드: {FirebaseManager.Instance.CurrentUserData.user_Gold}");

		// 골드 변경 이벤트 호출
		OnGoldChanged?.Invoke(FirebaseManager.Instance.CurrentUserData.user_Gold);
	}
}