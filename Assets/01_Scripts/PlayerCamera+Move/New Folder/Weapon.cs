using UnityEngine;

public class Weapon : MonoBehaviour
{
	[Header("무기 공격력")] public int damage = 10;

	// Player 스크립트 참조
	private Player player;
	//private FSMManager manager;

	private void Start()
	{
		// 방법1: 부모 객체에 붙어있는 Player 스크립트 참조
		player = GetComponentInParent<Player>();
		//manager = GetComponentInParent<FSMManager>();
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
		if (collider.CompareTag("Monster"))
		{
			if (collider.gameObject.TryGetComponent<Monster>(out Monster monster))
			{
				monster.TakeDamage(damage);
			}

			else if (collider.gameObject.TryGetComponent<BossMonster>(out BossMonster bossMonster))
			{
				bossMonster.TakeDamage(damage);
			}

			player.SuckBlood();
		}
		else if (collider.CompareTag("Minion"))
		{
			print("Minion Damage");
			collider.gameObject.GetComponent<Minion>().TakeDamage(damage);
			player.SuckBlood();
		}
	}
}

// 중간 완성

//1. 히트 스톱 / 특정 창이 열렸을때 입력 처리 못하도록, 델타타임을 0으로

//2. 스킬 사용시 스킬 콜라이더 활성화, 파티클에 콜라이더 붙여서 일반 공격이랑 구현

//3. 넉백 - 조건 보스몬스터의 스킬

//5. 콜라이더 손보고 -> 업그레이드 시 데미지 처리

//