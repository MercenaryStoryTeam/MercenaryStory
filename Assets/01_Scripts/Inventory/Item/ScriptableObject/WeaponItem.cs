using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Weapon Item", menuName = "Items/Weapon Item")]
[Serializable]
public class WeaponItem : ItemBase
{
    public int rank; // 아이템 등급
    public float damage; // 공격력

    private void Awake()
    {
    }
    
    
}
