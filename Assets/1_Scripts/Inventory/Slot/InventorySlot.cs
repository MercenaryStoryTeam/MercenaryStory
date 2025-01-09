using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IPointerClickHandler
{
    public ItemBase item;
    public Image itemImage;
    public Button itemButton;
    public Text itemCountText;
    
    private float clikcTime = 0;
    private void Awake()
    {
        itemButton.onClick.AddListener(itemButtonClick);
    }

    private void itemButtonClick()
    {
        UIManager.Instance.OpenItemInfoPanel();
        UIManager.Instance.SetItemInfoScreen(item);
    }

    private void Update()
    {
        UpdateUI();
    }

    public void AddItem(ItemBase newItem)
    {
        item = newItem;
    }

    public void UpdateUI()
    {
        if (item != null)
        {
            itemImage.sprite = item.image;
            itemImage.enabled = true;
            itemCountText.enabled = false;
            if (item.isStackable == true)
            {
                itemCountText.text = item.currentItemCount.ToString();
                itemCountText.enabled = true;
            }
        }
        else
        {
            itemImage.enabled = false;
            itemCountText.enabled = false;
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (Time.time - clikcTime < 0.3f)
        {
            clikcTime = -1;
        }
        else
        {
            clikcTime = Time.time;
        }
    }
}
