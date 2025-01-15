using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryPanel : MonoBehaviour
{
    public CanvasGroup invenCanvasGroup;
    public GameObject panel;
    
    public Button invenCloseButton;
    public Text currentGoldText;

    private TestSY _testsy;
    private void Awake()
    {
        _testsy = FindObjectOfType<TestSY>();
        ButtonOnClick();
    }

    private void Update()
    {
        InteractableController();
        SetCurrentGold();
    }

    private void ButtonOnClick()
    {
        invenCloseButton.onClick.AddListener(InvenCloseButtonClick);
    }
    
    private void InvenCloseButtonClick()
    {
        UIManager.Instance.CloseInventoryPanel();
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
                UIManager.Instance.CloseItemInfoPanel();
            }
        }
    }

    private void InteractableController()
    {
        if (UIManager.Instance.itemInfo.itemInfoPanel.activeSelf)
        {
            invenCanvasGroup.alpha = 0.5f;
            invenCanvasGroup.interactable = false;
        }
        else if (!UIManager.Instance.itemInfo.itemInfoPanel.activeSelf)
        {
            invenCanvasGroup.alpha = 1;
            invenCanvasGroup.interactable = true;
        }
    }

    private void SetCurrentGold()
    {
        currentGoldText.text = "보유 골드: " + _testsy.myGold.ToString();
    }

}
