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

    // 플레이어 die 처리
    private void Die()
    {
        SoundManager.Instance.PlaySound("monster_potbellied_battle_1");
        Debug.Log("Player Die");

        // FSMManager를 통해 Die 상태 처리
        FSMManager.Instance.TriggerDie();

        PlayerData.Instance.currentHp = PlayerData.Instance.maxHp;

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
