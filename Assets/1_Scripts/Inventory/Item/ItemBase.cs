using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ItemBase : ScriptableObject
{
    public int id;
    public string name;
    public string description;
    public bool isSellable;
    public Sprite image;
    public List<GameObject> prefab;
    public int currentItemCount;
}
