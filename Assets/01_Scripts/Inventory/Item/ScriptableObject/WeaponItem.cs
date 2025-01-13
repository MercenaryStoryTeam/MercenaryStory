using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Weapon Item", menuName = "Items/Weapon Item")]
[Serializable]
public class WeaponItem : ItemBase
{
    public int Rank; // 아이템 등급
    public float Damage; // 공격력
    public float sellPrice; // 물품 가격
    public bool isEquiped; // 장착 여부
    private void Awake()
    {
        isSellable = true;
        isStackable = false;
        isEquiped = false;
    }
    
    
}
