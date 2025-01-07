using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : SingletonManager<UIManager>
{
    public ItemInfoPanel itemInfoPanel;
    public InventoryPanel inventoryPanel;
    [HideInInspector]public bool isInventoryActive = false;

    protected override void Awake()
    {
        base.Awake();
        itemInfoPanel = FindObjectOfType<ItemInfoPanel>();
        inventoryPanel = FindObjectOfType<InventoryPanel>();
    }

    #region Inventory
    
    public void OpenInventoryPanel()
    {
        isInventoryActive = true;
        inventoryPanel.panel.SetActive(true);
    }

    public void CloseInventoryPanel()
    {
        isInventoryActive = false;
        inventoryPanel.panel.SetActive(false);
    }

    #endregion
}
