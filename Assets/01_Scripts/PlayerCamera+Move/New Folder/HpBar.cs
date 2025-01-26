using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; 

public class HpBar : MonoBehaviour
{
    [Header("HpBarPanel 참조")]
    public Image HpBarPanel;

    public Player player;

    private void Awake()
    {
        FindPlayer();
    }

    private void OnEnable()
    {
        // 씬 로드 이벤트에 메서드 등록
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // 씬 로드 이벤트에서 메서드 제거
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 새로운 씬이 로드될 때마다 플레이어를 다시 찾음
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

        if (playerLayer == -1)
        {
            Debug.LogError("[HpBar] 'Player' 레이어가 존재하지 않습니다.");
            return;
        }

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
                    Debug.Log($"[HpBar] Player를 '{obj.name}' 오브젝트에서 찾았습니다.");
                    break;
                }
            }
        }

        if (player == null)
        {
            Debug.LogWarning("[HpBar] Player를 찾지 못했습니다.");
        }
    }
}

// 멀티에서 구분: 레이어가 플레이어이면서 오브젝트면이 Clone이 아닌 오브젝트의 ooo 스크립트를 참조하도록 설정
// 필요한 스크립트 자동 참조하도록,그리고 미씽나거나 참조를 못하면 계속 참조하도록 설정
