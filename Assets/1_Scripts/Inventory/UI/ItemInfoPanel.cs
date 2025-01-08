using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ItemInfoPanel : MonoBehaviour
{
    public GameObject itemInfoPanel;

    public Image itemImage;
    public Text itemName;
    public Text itemInfo;
    public Text itemCount;
    
    public Button equipItemButton;
    public Button removeItemButton;
    public Button closeButton;

    private void Awake()
    {
        itemImage = GetComponent<Image>();
        itemInfoPanel.SetActive(false);
        InfoButtonOnClick();
    }

    private void Update()
    {
        
    }
    
    private void InfoButtonOnClick()
    {
        equipItemButton?.onClick.AddListener(EquipItemButtonClick);
        removeItemButton?.onClick.AddListener(RemoveItemButtonClick);
        closeButton.onClick.AddListener(CloseButtonClick);
    }
    
    private void CloseButtonClick()
    {
        itemInfoPanel.SetActive(false);
    }

    private void RemoveItemButtonClick()
    {
        //inven.items.Remove();
    }
    
    private void EquipItemButtonClick()
    {
    }

}
