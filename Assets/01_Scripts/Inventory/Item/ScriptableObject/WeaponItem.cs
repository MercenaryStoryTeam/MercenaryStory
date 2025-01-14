using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon Item", menuName = "Items/Weapon Item")]
[Serializable]
public class WeaponItem : ItemBase
{
    public int Rank; // 아이템 등급
    public float Damage; // 공격력
    public bool isEquiped; // 장착 여부
    private void Awake()
    {
        isEquiped = false;
    }
    
    
}
