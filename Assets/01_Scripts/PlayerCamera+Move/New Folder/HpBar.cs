using UnityEngine;
using UnityEngine.UI;

public class HpBar : MonoBehaviour
{
    [Header("HpBarPanel 참조")]
    public Image HpBarPanel;

    public Player player;

    private void Awake()
    {
        FindPlayer();
    }

    private void Update()
    {
        // 플레이어를 찾지 못했거나 참조가 끊어진 경우 다시 찾음
        if (player == null)
        {
            FindPlayer();
        }

        // 플레이어 스크립트를 참조 받으면서, HpBarPanel이 할당된 상황이면 실행
        if (player != null && HpBarPanel != null)
        {
            // Fill Amount를 현재 체력 비율로 설정
            HpBarPanel.fillAmount = player.currentHp / player.maxHp;
        }
    }

    private void FindPlayer()
    {
        // 플레이어 레이어 설정
        int playerLayer = LayerMask.NameToLayer("Player");

        // 모든 오브젝트를 검색
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            // 이름에 "Clone"이 없고, 레이어가 플레이어인 오브젝트만 찾음
            if (!obj.name.Contains("Clone") && obj.layer == playerLayer)
            {
                player = obj.GetComponent<Player>();
                if (player != null)
                {
                    // 첫 번째로 찾은 플레이어 오브젝트를 할당하고 종료
                    break; 
                }
            }
        }
    }
}

//
