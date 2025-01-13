using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;
using System.IO;
using System.Linq;
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
    public List<GameObject> weapons;
    public Transform rightHandParent;
    private TestSY _testSY;
    protected override void Awake()
    {
        base.Awake();
        
        //아이템 장착 테스트
        _testSY = FindObjectOfType<TestSY>();
        for (int i = 0; i < weapons.Count; i++)
        {
            weapons[i].SetActive(false);
        }
    }
    
    public void SetCurrentEquip(ItemBase item)
    {
        item = _testSY.currentWeapon;
        for (int i = 0; i < item.prefab.Count; i++)
        {
            item.prefab[i].gameObject.SetActive(true);
        }
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
