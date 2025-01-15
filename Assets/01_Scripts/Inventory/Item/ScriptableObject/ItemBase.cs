using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class ItemBase : ScriptableObject
{
    [Header("Item Basic Information")]
    public int id; //아이템 고유 ID
    public string name; //아이템 이름
    [TextArea]
    public string description; //아이템 설명
    public int currentItemCount; //현재 아이템 갯수
    public int itemClass; //아이템 종류
    public float price; //아이템 가격 / 퀘스트 아이템 = -1
    public int dropPercent; //아이템 드랍률
    
    [Header("bool")]
    public bool isHave; //아이템 보유 여부 -> 구현에 필요 없으면 삭제 예정
    
    [Header("Icon&Prefab")]
    public Sprite image; //아이템 아이콘
    public List<GameObject> equipPrefab; //아이템 프리팹
    public GameObject dropLightLine; //빛 기둥 프리팹
    

    private void Awake()
    {
        isHave = false;
    }
}
