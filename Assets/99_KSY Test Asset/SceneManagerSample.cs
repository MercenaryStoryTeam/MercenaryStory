using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneManagerSample : MonoBehaviour
{
    [Header("전환할 씬 이름")]
    [SerializeField] private string nextSceneName; // 전환할 씬 이름
    [Header("버튼")]
    [SerializeField] private Button sceneChangeButton; // 씬 전환 버튼

    private void Awake()
    {
        if (sceneChangeButton != null)
        {
            sceneChangeButton.onClick.AddListener(LoadNextScene);
        }
    }

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
