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
    public List<OtherItem> otherItems;
    public List<ItemBase> items;

}
