using System;
using UnityEngine;

public class GoldManager : MonoBehaviour
{
    // 현재 골드 변수
    private float currentGold;

    // 골드 변경 시 호출되는 이벤트
    public event Action<float> OnGoldChanged;

    private void Start()
    {
        // Firebase에서 초기 골드 값을 불러옵니다.
        if (FirebaseManager.Instance != null)
        {
            currentGold = FirebaseManager.Instance.CurrentUserData.user_Gold;
            OnGoldChanged?.Invoke(currentGold); 
        }
        else
        {
            Debug.LogWarning("[GoldManager] FirebaseManager가 존재하지 않습니다.");
        }
    }

    // 골드를 사용
    public bool SpendGold(float amount)
    {
        if (currentGold >= amount)
        {
            currentGold -= amount;
            Debug.Log($"[GoldManager] 골드 {amount}을 사용했습니다. 남은 골드: {currentGold}");

            // Firebase에 골드 값 반영
            UpdateFirebaseGold();

            // 골드 변경 이벤트 호출
            OnGoldChanged?.Invoke(currentGold);

            return true;
        }
        else
        {
            Debug.LogWarning($"[GoldManager] 골드가 부족합니다. 필요: {amount}, 현재: {currentGold}");
            return false;
        }
    }

    // 골드를 획득
    public void AddGold(float amount)
    {
        currentGold += amount;
        Debug.Log($"[GoldManager] 골드 {amount}을 획득했습니다. 현재 골드: {currentGold}");

        // Firebase에 골드 값 반영
        UpdateFirebaseGold();

        // 골드 변경 이벤트 호출
        OnGoldChanged?.Invoke(currentGold);
    }

    // 현재 골드 가져오기
    public float GetCurrentGold()
    {
        return currentGold;
    }

    // Firebase에 골드 값 업데이트
    private void UpdateFirebaseGold()
    {
        if (FirebaseManager.Instance != null)
        {
            FirebaseManager.Instance.CurrentUserData.user_Gold = currentGold;
        }
        else
        {
            Debug.LogWarning("[GoldManager] FirebaseManager가 존재하지 않습니다.");
        }
    }
}
