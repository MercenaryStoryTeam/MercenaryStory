using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using File = System.IO.File;
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
    }

    public void RandomWeaponDrop()
    {
        int randomWeapon = Random.Range(0, weaponItems.Count);
        ItemBase weapon = weaponItems[randomWeapon];
        items.Add(weapon);
    }

    public void RandomQuestDrop()
    {
        int randomQuest = Random.Range(0, questItems.Count);
        ItemBase quest = questItems[randomQuest];
        items.Add(quest);
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
    public void SetEquipItemInfo(WeaponItem weapon)
    {
        UIManager.Instance.itemInfo.itemName.text = weapon.name.ToString();
        UIManager.Instance.itemInfo.itemInfo.text = weapon.description.ToString();
        UIManager.Instance.itemInfo.itemImage.sprite = weapon.image;
    }
    
    public void SetQuestItemInfo(QuestItem questItem)
    {
        UIManager.Instance.itemInfo.itemName.text = questItem.name.ToString();
        UIManager.Instance.itemInfo.itemInfo.text = questItem.description.ToString();
        UIManager.Instance.itemInfo.itemImage.sprite = questItem.image;
        UIManager.Instance.itemInfo.itemCount.text = questItem.currentItemCount.ToString();
    }
    
    public void SetOtherItemInfo(OtherItem otherItem)
    {
        UIManager.Instance.itemInfo.itemName.text = otherItem.name.ToString();
        UIManager.Instance.itemInfo.itemInfo.text = otherItem.description.ToString();
        UIManager.Instance.itemInfo.itemImage.sprite = otherItem.image;
        UIManager.Instance.itemInfo.itemCount.text = otherItem.currentItemCount.ToString();
    }
    
    public void RandomOtherDrop()

    {
        int randomOther = Random.Range(0, otherItems.Count);
        ItemBase other = otherItems[randomOther];
        items.Add(other);
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
