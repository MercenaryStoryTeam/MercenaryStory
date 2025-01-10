using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ItemBase : ScriptableObject
{
    public int id; //아이템 고유 ID
    public string name; //아이템 이름
    [TextArea]
    public string description; //아이템 설명
    public int dropPercent; //아이템 드랍률
    public bool isSellable; //팔 수 있는 물품인지
    public bool isStackable; //슬롯에 중복 추가가 가능한지
    public bool isHave; //아이템 보유 여부
    public Sprite image; //아이템 아이콘
    public List<GameObject> prefab; //아이템 프리팹
    public int currentItemCount; //현재 아이템 갯수
    public int itemClass; //아이템 종류

    private void Awake()
    {
        isHave = false;
    }
}
