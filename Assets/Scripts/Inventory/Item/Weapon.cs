using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Weapon : Item
{
    private List<GameObject> weapons;
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            Random.Range(0, weapons.Count);
        }
    }
}
