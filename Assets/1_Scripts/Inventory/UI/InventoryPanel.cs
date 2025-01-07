using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class InventoryPanel : MonoBehaviour
{
    
    public GameObject panel;
    
    public Button invenCloseButton;
    
    private void Awake()
    {
        panel.SetActive(false);
        ButtonOnClick();
    }

    private void ButtonOnClick()
    {
        invenCloseButton.onClick.AddListener(InvenCloseButtonClick);
    }
    
    private void InvenCloseButtonClick()
    {
        panel.SetActive(false);
        UIManager.Instance.isInventoryActive = false;
    }

    public void TryOpenInventory()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (!UIManager.Instance.isInventoryActive)
            {
                UIManager.Instance.OpenInventoryPanel();
            }
            else
            {
                UIManager.Instance.CloseInventoryPanel();
            }
        }
    }
}
