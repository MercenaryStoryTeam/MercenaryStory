using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class InvenUI : MonoBehaviour
{
    public GameObject invenPanel;
    public GameObject itemInfoPanel;
    
    public Image image;
    public Text itemName;
    public Text itemInfo;
    
    public Button equipItemButton;
    public Button removeItemButton;
    public Button invenCloseButton;
    public Button iteminfoCloseButton;

    public static bool isInventoryActive = false;
    
    private Inventory inven;
    private void Awake()
    {
        inven = FindObjectOfType<Inventory>();
        invenPanel.SetActive(false);
        itemInfoPanel.SetActive(false);
        ButtonOnClick();
    }

    private void ButtonOnClick()
    {
        equipItemButton.onClick.AddListener(EquipItemButtonClick);
        removeItemButton.onClick.AddListener(RemoveItemButtonClick);
        invenCloseButton.onClick.AddListener(InvenCloseButtonClick);
        iteminfoCloseButton.onClick.AddListener(ItemInfoCloseButtonClick);
    }
    private void ItemInfoCloseButtonClick()
    {
        itemInfoPanel.SetActive(false);
    }

    private void InvenCloseButtonClick()
    {
        invenPanel.SetActive(false);
        isInventoryActive = false;
    }

    private void RemoveItemButtonClick()
    {
        //inven.items.Remove();
    }

    private void EquipItemButtonClick()
    {
        image.sprite = Resources.Load<Sprite>("InvenUI");
    }

    public void OpenInventory()
    {
        invenPanel.SetActive(true);
        isInventoryActive = true;
    }

    public void CloseInventory()
    {
        invenPanel.SetActive(false);
        isInventoryActive = false;
    }

    public void TryOpenInventory()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (!isInventoryActive)
            {
                OpenInventory();
            }
            else
            {
                CloseInventory();
            }
        }
    }
}
