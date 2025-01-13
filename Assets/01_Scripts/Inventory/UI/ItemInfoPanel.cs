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
    
    public Button firstOptionButton;
    public Button secondOptionButton;
    public Button closeButton;

    public GameObject secondOption;
    
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
        firstOptionButton.onClick.AddListener(EquipItemButtonClick);
        secondOptionButton.onClick.AddListener(RemoveItemButtonClick);
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
        EquipmentPanel equipment = FindObjectOfType<EquipmentPanel>();
        print("장착");
        equipment.SetEquipImage(currentSelectedSlot);
        ItemManager.Instance.SetCurrentEquip(currentSelectedSlot.item);
        UIManager.Instance.CloseItemInfoPanel();
    }

    public void SetCurrentSlot(InventorySlot slot)
    {
        currentSelectedSlot = slot;
    }
}
