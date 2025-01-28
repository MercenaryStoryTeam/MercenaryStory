using Photon.Realtime;
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
    private GameObject shop; // 상점 오브젝트
    private Player player;
    private void Awake()
    {
        MobileUIOnClick();
    }

    private void Start()
    {
        shop = GameObject.Find("Store");
		if (shop == null)
		{
			Debug.LogError("[PlayerInputManager] 씬에 'Store' GameObject가 없습니다.");
		}

        player = GameObject.Find($"{FirebaseManager.Instance.CurrentUserData.user_Name}").GetComponent<Player>();
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

    private void InteractionButtonClick()
    {
        // 기존 E 키 처리 코드 유지...
        if (player.droppedItems.Count > 0)
        {
            for (int i = player.droppedItems.Count - 1; i >= 0; i--)
            {
                if (player.droppedItems[i].droppedItem == null ||
                    player.droppedItems[i].droppedLightLine == null)
                {
                    player.droppedItems.RemoveAt(i);
                    continue;
                }

                if (Vector3.Distance(transform.position,
                        player.droppedItems[i].droppedLightLine.transform.position) <
                    3f)
                {
                    if (player.droppedItems[i].droppedItem != null &&
                        player.droppedItems[i].droppedLightLine != null)
                    {
                        InventoryManger.Instance.AddItemToInventory(player.droppedItems[i]
                            .droppedItem);
                        Destroy(player.droppedItems[i].droppedLightLine);
                    }
                }
            }
        }

        if (Vector3.Distance(transform.position, shop.transform.position) < 7f)
        {
            UIManager.Instance.shop.TryOpenShop();
        }
    }
}
