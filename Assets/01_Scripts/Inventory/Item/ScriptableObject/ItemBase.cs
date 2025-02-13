using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class ItemBase : ScriptableObject
{
	[Header("아이템 기본 정보")]
	public int id; //아이템 고유 ID
	public string itemName; //아이템 이름
	[TextArea] public string description; //아이템 설명
	public int currentItemCount; //현재 아이템 갯수
	public int itemClass; //아이템 종류
	public float price; //아이템 가격 / 퀘스트 아이템 = -1
	public float dropPercent; //아이템 드랍률

	[Header("아이콘 및 이펙트")]
	public Sprite image; //아이템 아이콘
	public GameObject dropEffect; //빛 기둥 프리팹
}