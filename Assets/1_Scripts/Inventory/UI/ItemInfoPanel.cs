using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ItemInfoPanel : MonoBehaviour
{
    public GameObject itemInfoPanel;

    public Image itemImage;
    public Text itemName;
    public Text itemDescription;
    public Text firstOptionText;
    public Text secondOptionText;
    
    public Button firstButton;
    public Button secondButton;
    public Button closeButton;

    public GameObject secondOption;
    private void Awake()
    {
        itemInfoPanel.SetActive(false);
        InfoButtonOnClick();
    }

    private void Update()
    {
        
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
        //inven.items.Remove();
    }
    
    private void EquipItemButtonClick()
    {
    }

}
