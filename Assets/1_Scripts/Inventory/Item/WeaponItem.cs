using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon Item", menuName = "Items/Weapon Item")]
[Serializable]
public class WeaponItem : ItemBase
{
    public int Rank;
    public float Damage;
    public float sellPrice;
    private void Awake()
    {
        currentItemCount = 0;
        isSellable = true;
    }
    
    
}
