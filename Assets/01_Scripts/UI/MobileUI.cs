using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MobileUI : MonoBehaviour
{
    public Button invenButton;
    public Button optionButton;
    
    public Button interactButton; //플레이어 인풋 매니저 E키 그대로 사용

    private void Awake()
    {
        MobileUIOnClick();
    }

    private void MobileUIOnClick()
    {
        invenButton.onClick.AddListener(InvenButtonClicked);
        optionButton.onClick.AddListener(OptionButtonClicked);
    }
    
    private void InvenButtonClicked()
    {
        UIManager.Instance.OpenInventoryPanel();
    }

    private void OptionButtonClicked()
    {
        UIManager.Instance.OpenOptionPanel();
    }
}
