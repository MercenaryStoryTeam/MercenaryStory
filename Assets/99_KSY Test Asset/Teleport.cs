using UnityEngine;
using UnityEngine.SceneManagement;

// 목적: 플레이어의 장소간 이동 
public class Teleport : MonoBehaviour
{
    [Header("전환할 씬 이름")]
    public string nextSceneName; // 전환할 씬의 이름

    [Header("씬 로드 지연시간")]
    public float loadSceneDelay = 1f; // 씬 전환 지연 시간

    [Header("플레이어 레이어")]
    public LayerMask playerLayer; // 플레이어가 속한 레이어 마스크

    // 다음 씬 전환 가능 여부
    private bool canTeleport = false;

    // 콜라이더 충돌 o 호출
    private void OnTriggerEnter(Collider collider)
    {
        // 플레이어가 트리거 영역 안에 있으면 실행
        if (((1 << collider.gameObject.layer) & playerLayer) != 0)
        {
            // 다음 씬 전환 가능
            canTeleport = true;

            // Teleport 오브젝트 마테리얼 색상 변경 (빨간색)
            SetTeleportColor(Color.red);

            Debug.Log("다음 씬 전환 가능: 'B' 키를 눌러 다음 씬으로 이동하세요.");
        }
    }

    // 콜라이더 충돌 x 호출
    private void OnTriggerExit(Collider collider)
    {
        // 플레이어가 트리거 영역 밖에 있으면 실행
        if (((1 << collider.gameObject.layer) & playerLayer) != 0)
        {
            // 다음 씬 전환 불가
            canTeleport = false;

            // Teleport 오브젝트 마테리얼 색상 원래대로 복구 (흰색)
            SetTeleportColor(Color.white);

            Debug.Log("다음 씬 전환 불가: 트리거 영역을 벗어났습니다.");
        }
    }

    // 매 프레임마다 실행
    private void Update()
    {
        // 다음 씬 전환 가능 상태이고 'B' 키를 입력하면 즉시 씬 전환 코루틴 실행
        if (canTeleport && Input.GetKeyDown(KeyCode.B))
        {
            // 딜레이 후 씬 전환 시작
            StartCoroutine(DelayedSceneLoad());
        }
    }

    // 일정 시간 후 씬 전환을 수행하는 코루틴
    private System.Collections.IEnumerator DelayedSceneLoad()
    {
        Debug.Log($"씬 전환을 {loadSceneDelay}초 후에 시작합니다.");

        // 지연 시간 동안 대기
        yield return new WaitForSeconds(loadSceneDelay);

        // 씬 전환 직전에 플레이어 위치 저장
        PlayerData.Instance.SavePosition();

        // 다음 씬 전환
        LoadNextScene();
    }

    // 다음 씬 전환
    public void LoadNextScene()
    {
        // 씬 이름이 설정되어 있을 때만 씬 전환 수행
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

    // 텔레포트 오브젝트의 마테리얼 색상 설정
    private void SetTeleportColor(Color color)
    {
        Renderer teleportRenderer = GetComponent<Renderer>();
        if (teleportRenderer != null)
        {
            teleportRenderer.material.color = color; 
        }
    }
}

// 완료
