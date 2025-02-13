using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon Item", menuName = "Items/Weapon Item")]
[Serializable]
public class WeaponItem : ItemBase
{
	[Header("장비 아이템 기본 정보")]
	public int rank; // 아이템 등급
	public int damage; // 공격력
	
	[Header("장비 프리팹")]
	public GameObject equipPrefab; //아이템 프리팹
}