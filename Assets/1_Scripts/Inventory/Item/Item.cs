using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class Item
{
    public int item_Id;
    public string item_Name;
    public int item_Class;
    public bool item_Unique;
    public bool item_Stackposible;
    public int item_Stackingamount;
    public int item_Rank;
    public int item_MaxRank;
    public float item_Cellprice;
}
