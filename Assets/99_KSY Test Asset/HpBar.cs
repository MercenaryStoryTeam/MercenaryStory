using UnityEngine;
using UnityEngine.UI;

public class HpBar : MonoBehaviour
{
    [Header("필드에 있는 플레이어의 스크립트 참조")]
    public Player player; 

    [Header("HpBarPanel 참조")]
    public Image HpBarPanel; 

    private void Update()
    {
        if (player != null && HpBarPanel != null)
        {
            // Fill Amount를 현재 체력 비율로 설정
            HpBarPanel.fillAmount = PlayerData.Instance.currentHp / PlayerData.Instance.maxHp;
        }
    }
}

// 완성
