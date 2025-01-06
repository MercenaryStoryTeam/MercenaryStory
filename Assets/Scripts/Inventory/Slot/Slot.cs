using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slot : MonoBehaviour
{
    public List<Slot> slots;

    private void AddItem()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            slots.Add(new Slot());
        }
    }
    
}
