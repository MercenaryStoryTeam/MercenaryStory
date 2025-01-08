using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;
using System.IO;
using Newtonsoft.Json;
using File = System.IO.File;

public class ItemManager : SingletonManager<ItemManager>
{
    private ItemBase _itemBase;

    protected override void Awake()
    {
        base.Awake();

    }
    
}
