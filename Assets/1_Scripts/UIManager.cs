using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class UIManager : SingletonManager<UIManager>
{
    public ItemInfoPanel itemInfo;
    public InventoryPanel inventory;
    public ShopPanel shop;
    public EquipmentPanel equipment;
    [HideInInspector]public bool isInventoryActive = false;
    [HideInInspector]public bool isShopActive = false;

    protected override void Awake()
    {
        base.Awake();
        itemInfo = FindObjectOfType<ItemInfoPanel>();
        inventory = FindObjectOfType<InventoryPanel>();
        shop = FindObjectOfType<ShopPanel>();
        equipment = FindObjectOfType<EquipmentPanel>();
    }

    #region Inventory
    
    public void OpenInventoryPanel()
    {
        isInventoryActive = true;
        inventory.panel.SetActive(true);
    }

    public void CloseInventoryPanel()
    {
        isInventoryActive = false;
        inventory.panel.SetActive(false);
    }

    #endregion
    
    #region Shop

    public void OpenShopPanel()
    {
        isShopActive = true;
        shop.shopPanel.SetActive(true);
    }

    public void CloseShopPanel()
    {
        isShopActive = false;
        shop.shopPanel.SetActive(false);
    }
    
    #endregion

    #region ItemInfo

    public void OpenItemInfoPanel()
    {
        itemInfo.itemInfoPanel.SetActive(true);
    }

    public void CloseItemInfoPanel()
    {
        itemInfo.itemInfoPanel.SetActive(false);
    }
    
    public void SetItemInfoScreen(ItemBase item)
    {
        if (item.itemClass == 1)
        {
            itemInfo.firstButton.gameObject.SetActive(true);
            itemInfo.secondButton.gameObject.SetActive(true);
            itemInfo.secondOption.SetActive(true);
            itemInfo.firstOptionText.text = "장착";
        }

        if (item.itemClass == 2)
        {
            itemInfo.firstButton.gameObject.SetActive(false);
            itemInfo.secondButton.gameObject.SetActive(true);
            itemInfo.secondOption.SetActive(true);
            itemInfo.firstOptionText.text = item.currentItemCount + "개";
        }

        if (item.itemClass == 3)
        {
            itemInfo.firstButton.gameObject.SetActive(false);
            itemInfo.secondOption.SetActive(false);
            itemInfo.firstOptionText.text = item.currentItemCount + "개";
        }  
        
        itemInfo.itemName.text = item.name.ToString();
        itemInfo.itemDescription.text = item.description.ToString();
        itemInfo.itemImage.sprite = item.image;
        
    }
    
    #endregion

}
