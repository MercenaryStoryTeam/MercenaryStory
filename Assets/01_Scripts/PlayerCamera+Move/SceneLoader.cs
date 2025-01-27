using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // 인스펙터에서 씬 이름 할당
    public string sceneName;

    // 버튼 클릭 시 호출될 메서드
    public void LoadScene()
    {
        // 지정된 씬으로 이동
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}
