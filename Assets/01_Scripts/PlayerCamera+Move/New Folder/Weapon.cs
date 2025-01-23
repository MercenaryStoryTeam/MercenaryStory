using UnityEngine;

public class Weapon : MonoBehaviour
{
	[Header("무기 공격력")] public int damage = 10;

	[Header("몬스터 레이어")] public LayerMask Monster;

	// Player 스크립트 참조
	private Player player;

	private void Start()
	{
		// 방법1: 부모 객체에 붙어있는 Player 스크립트 참조
		player = GetComponentInParent<Player>();

		// 방법1 실패
		if (player == null)
		{
			// 방법2: 태그를 이용해 Player 스크립트 참조
			GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
			if (playerObject != null)
			{
				player = playerObject.GetComponent<Player>();
			}

			if (player == null)
			{
				Debug.LogError("Player 참조x -> 부모 객체 확인 및 태크 확인");
			}
		}
	}

	private void Update()
	{
		SetWeaponDamage();
	}

	private void SetWeaponDamage()
	{
		int currentWeaponId = FirebaseManager.Instance.CurrentUserData.user_weapon_item_Id;
		ItemBase currentWeapon =
			InventoryManger.Instance.allItems.Find(x => x.id == currentWeaponId);
		if (currentWeapon is WeaponItem weapon)
		{
			damage = weapon.damage;
		}
	}

	// 콜라이더 충돌 시 호출
	private void OnTriggerEnter(Collider collider)
	{
		// 충돌한 객체의 레이어가 몬스터라면 실행
		if (((1 << collider.gameObject.layer) & Monster.value) != 0)
		{
			// 충돌한 객체에서 Monster 스크립트를 참조
			Monster monster = collider.gameObject.GetComponent<Monster>();
			if (monster != null)
			{
				// 몬스터에게 데미지 적용
				monster.TakeDamage(damage);

				// 플레이어 스크립트가 유효하다면 흡혈 처리
				if (player != null)
				{
					player.SuckBlood(); // Player 스크립트의 SuckBlood 메서드 호출
				}
				else
				{
					Debug.LogWarning("Player가 설정되지 않았습니다. SuckBlood 호출 불가");
				}
			}
			else
			{
				Debug.LogError($"충돌한 객체에 Monster 스크립트가 없습니다. 객체 이름: {collider.gameObject.name}");
			}
		}
	}
}

// 중간 완성

//1. 히트 스톱 / 특정 창이 열렸을때 입력 처리 못하도록, 델타타임을 0으로

//2. 스킬 사용시 스킬 콜라이더 활성화, 파티클에 콜라이더 붙여서 일반 공격이랑 구현

//3. 넉백 - 조건 보스몬스터의 스킬

//4. 대쉬 무적 

//5. 콜라이더 손보고 -> 업그레이드 시 데미지 처리

//6. ui 활성화될 때 입력처리 플레이어의 입력처리 안되도록
//인풋매니저 비활성화 - 가장 직접적인 방법

//7. 스킬 파티클 위치 조절,
//러쉬 나머지 3개의 일반 스킬 - 파티클 적당한 위치에 적당한거 골라서 붙이기
//레벨별 4개 

//8. 사운드 추가 - 공격할때, 스킬 사용할때, 클립명 뒤에 , gameObject 추가

//