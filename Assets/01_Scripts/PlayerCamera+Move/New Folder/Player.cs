using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    [Header("전환할 씬 이름")]
    public string nextSceneName;

    [Header("씬 로드 지연시간")]
    public int loadSceneDelay = 1;

    // 흡혈 처리
    public void SuckBlood()
    {
        if (PlayerData.Instance.currentHp >= PlayerData.Instance.maxHp) return;

        float suckBloodPercentage = PlayerData.Instance.suckBlood / 100f;
        float healAmount = PlayerData.Instance.maxHp * suckBloodPercentage;

        PlayerData.Instance.currentHp += healAmount;
        PlayerData.Instance.currentHp = Mathf.Clamp(PlayerData.Instance.currentHp, 0, PlayerData.Instance.maxHp);

        Debug.Log($"흡혈 회복량: {healAmount}/현재 체력: {PlayerData.Instance.currentHp}/{PlayerData.Instance.maxHp}");
    }

    // 데미지 처리
    public void TakeDamage(float damage)
    {
        if (PlayerData.Instance.currentHp <= 0) return;

        PlayerData.Instance.currentHp -= damage;
        PlayerData.Instance.currentHp = Mathf.Clamp(PlayerData.Instance.currentHp, 0, PlayerData.Instance.maxHp);

        Debug.Log($"플레이어 체력: {PlayerData.Instance.currentHp}/{PlayerData.Instance.maxHp} (받은 데미지: {damage})");

        if (PlayerData.Instance.currentHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // 사운드 재생
        SoundManager.Instance.PlaySound("monster_potbellied_battle_1");

        // 디버그 메시지 출력
        Debug.Log("Player Die");

        // PlayerMove 스크립트 참조
        PlayerFsm playerMove = GetComponent<PlayerFsm>();
        if (playerMove != null)
        {
            // Die 상태 애니 구현
            playerMove.Die();
        }
        else
        {
            Debug.LogWarning("PlayerMove 스크립트를 찾을 수 없습니다.");
        }

        // 체력을 최대값으로 복원
        PlayerData.Instance.currentHp = PlayerData.Instance.maxHp;

        // 플레이어의 위치 값을 여기다가 넣으면 될 듯
        // 없으면 초기씬 초기값으로 다음씬에서 스폰

        // 지정된 딜레이 후 다음 씬으로 로드
        Invoke("LoadNextScene", loadSceneDelay);
    }


    // 다음 씬으로 전환
    private void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadSceneAsync(nextSceneName);
        }
        else
        {
            Debug.LogError("씬 이름을 설정하세요.");
        }
    }
}
