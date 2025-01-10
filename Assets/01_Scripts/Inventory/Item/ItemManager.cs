using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using Input = UnityEngine.Input;
using Random = UnityEngine.Random;

public class ItemManager : SingletonManager<ItemManager>
{
    public List<WeaponItem> weaponItems;
    public List<QuestItem> questItems;
    public List<OtherItem> otherItems;
    public List<ItemBase> items;
    
    protected override void Awake()
    {
        base.Awake();
        for (int i = 0; i < items.Count; i++)
        {
            items[i].currentItemCount = 0;
        }
    }
    
    public void SetCurrentEquipImage(WeaponItem weapon)
    {
        Color equipColor = UIManager.Instance.equipment.currentEquipImage.color;
        //게임매니저 - 플레이어 - 현재 장착 무기에 따라 장착무기 로직 추가
        //GameManager.Instance.Player.currentEquip = weapon;
        UIManager.Instance.equipment.currentEquipImage.sprite = weapon.image;
        equipColor.a = 1;
        //if(GameManager.Instance.Player.currentEquip == null) 관련 로직 추가
        //{equipColor.a = 0f;}
    }
    
    public string SerializeItems()
    {
        var itemDataContainer = new ItemDataContainer
        {
            weapons = weaponItems,
            quests = questItems,
            others = otherItems
        };
        
        return JsonUtility.ToJson(itemDataContainer);
    }
    
}
