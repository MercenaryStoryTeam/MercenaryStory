using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;


public class ItemManager : SingletonManager<ItemManager>
{
    public List<WeaponItem> weaponItems;
    public List<QuestItem> questItems;
    public List<OtherItem> otherItems;
    public List<ItemBase> items;
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
