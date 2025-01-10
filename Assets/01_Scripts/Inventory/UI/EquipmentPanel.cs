using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentPanel : MonoBehaviour
{
    
    public Image currentEquipImage;
    private TestSY _testSy;
    private void Awake()
    {
        _testSy = FindObjectOfType<TestSY>();
    }

    private void Update()
    {
       UpdateUI();
    }

    private void UpdateUI()
    {
        if (_testSy.isEquipped == false)
        {
            currentEquipImage.enabled = false;
        }
        
        else
        {
            currentEquipImage.enabled = true;
        }
    }
}
