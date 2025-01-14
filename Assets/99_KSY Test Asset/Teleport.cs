using UnityEngine;
using UnityEngine.SceneManagement;

// 목적: 플레이어의 장소간 이동 
public class Teleport : MonoBehaviour
{
    [Header("전환할 씬 이름")]
    public string nextSceneName;

    [Header("씬 로드 지연시간")]
    public float loadSceneDelay = 1f; 

    [Header("플레이어 레이어")]
    public LayerMask playerLayer;

    private bool sceneLoaded = false;
    public PlayerPosition playerPosition;

    // 콜라이더 충돌 시 호출
    private void OnTriggerEnter(Collider collider)
    {
        // 충돌한 객체의 레이어가 플레이어라면 실행
        if (!sceneLoaded && ((1 << collider.gameObject.layer) & playerLayer) != 0)
        {
            // 씬 전환 중복 호출 방지
            sceneLoaded = true;

            // 현재 씬에서 플레이어 위치 저장
            if (playerPosition != null)
            {
                playerPosition.SavePosition();
            }
            else
            {
                Debug.LogError("PlayerPosition 스크립트가 인스펙터에 할당되지 않았습니다.");
            }

            // Teleport 오브젝트 마테리얼 색상 변경 (빨간색)
            Renderer teleportRenderer = GetComponent<Renderer>();
            if (teleportRenderer != null)
            {
                teleportRenderer.material.color = Color.red;
            }

            // 일정 시간 후 씬 전환
            Invoke("LoadNextScene", loadSceneDelay);
        }
    }

    // 다음 씬으로 전환
    public void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            Debug.Log("씬 전환: " + nextSceneName);
            SceneManager.LoadSceneAsync(nextSceneName);
        }
        else
        {
            Debug.LogError("씬 이름을 설정하세요.");
        }
    }
}

// 완성
