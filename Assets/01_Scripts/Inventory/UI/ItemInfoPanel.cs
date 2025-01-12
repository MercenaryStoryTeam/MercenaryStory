using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ItemInfoPanel : MonoBehaviour
{
    public GameObject itemInfoPanel;

    public Image itemImage;
    private Image currentItemImage;
    public Text itemName;
    public Text itemDescription;
    public Text firstOptionText;
    
    public Button firstButton;
    public Button secondButton;
    public Button closeButton;

    public GameObject secondOption;
    
    private TestSY _testSy;
    private InventorySlot currentSelectedSlot;

    private void Awake()
    {
        itemInfoPanel.SetActive(false);
        InfoButtonOnClick();
    }

    private void Update()
    {
        currentItemImage = itemImage;
    }
    
    private void InfoButtonOnClick()
    {
        firstButton.onClick.AddListener(EquipItemButtonClick);
        secondButton.onClick.AddListener(RemoveItemButtonClick);
        closeButton.onClick.AddListener(CloseButtonClick);
    }
    
    private void CloseButtonClick()
    {
        UIManager.Instance.CloseItemInfoPanel();
    }

    private void RemoveItemButtonClick()
    {
        Inventory inventory = FindObjectOfType<Inventory>();
        inventory.DeleteItem(currentSelectedSlot);
        UIManager.Instance.CloseItemInfoPanel();
    }

    private InventorySlot SelectedSlot()
    {
        Inventory inventory = FindObjectOfType<Inventory>();
        foreach (InventorySlot slot in inventory.slots)
        {
            if (slot.item != null && slot.item.name == itemName.text)
            {
                return slot;
            }
        }
        return null;
    }
    
    private void EquipItemButtonClick()
    {
        print("장착");
        
        //테스트용임 나중에 고칠 예정
        // _testSy.isEquipped = true;
        // UIManager.Instance.equipment.currentEquipImage.sprite = currentItemImage.sprite;
        
        UIManager.Instance.CloseItemInfoPanel();
    }

    public void SetCurrentSlot(InventorySlot slot)
    {
        currentSelectedSlot = slot;
    }
}
