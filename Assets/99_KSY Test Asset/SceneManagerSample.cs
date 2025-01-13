using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 씬 전환 예시
// 목적: 플레이어가 A씬에서 die 상태일 때
// B씬에서 idle 상태로 전환 가능한지에 대해 체크
public class SceneManagerSample : MonoBehaviour
{
    // 씬 이름으로 할당
    [Header("전환할 씬 이름")]
    [SerializeField] private string nextSceneName; 
    [Header("버튼")]
    [SerializeField] private Button sceneChangeButton; 

    private void Awake()
    {
        if (sceneChangeButton != null)
        {
            sceneChangeButton.onClick.AddListener(LoadNextScene);
        }
    }

    // 다음씬으로 전환 처리
    public void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            Debug.Log("씬 전환: " + nextSceneName);
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError("씬 이름이 비어 있습니다. 씬 이름을 설정하세요.");
        }
    }
}
