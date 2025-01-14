using UnityEngine;
using UnityEngine.SceneManagement;


// 목적: 플레이어의 장소간 이동 
public class Teleport : MonoBehaviour
{
    [Header("전환할 씬 이름")]
    [SerializeField] private string nextSceneName;

    [Header("플레이어 레이어")]
    [SerializeField] private LayerMask playerLayer;

    private bool sceneLoaded = false;

    // 콜라이더 충돌 시 호출
    private void OnTriggerEnter(Collider collider)
    {
        // 충돌한 객체의 레이어가 플레이어라면 실행
        if (!sceneLoaded && (playerLayer.value & (1 << collider.gameObject.layer)) != 0)
        {
            // 씬 전환 중복 호출 방지
            sceneLoaded = true;

            // 일정 시간 후 씬 전환
            Invoke("LoadNextScene", 0f);
        }
    }

    // 다음 씬으로 전환 처리
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
