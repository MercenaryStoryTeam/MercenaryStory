using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MonsterHpBar : MonoBehaviour
{
    [Header("몬스터 스크립트 참조")]
    public Monster monster;

    [Header("HpBarPanel 참조")]
    public Image HpBarPanel;

    private Coroutine hideCoroutine;

    private void Start()
    {
        // 처음에 몬스터 HP 바를 비활성화
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (monster != null && HpBarPanel != null)
        {
            // Fill Amount를 현재 체력 비율로 설정
            HpBarPanel.fillAmount = monster.Hp / monster.MaxHp;
        }
    }

    // 5초 동안 유지되는 몬스터 hp바 활성화 처리
    public void ShowHpBar()
    {
        // 몬스터 hp바가 비활성 상태일 때 활성화함
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        // 기존 코루틴이 실행 중이라면 중지
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }

        // 5초 후에 HP 바를 비활성화하는 코루틴 시작
        hideCoroutine = StartCoroutine(HideAfterDelay(5f));

        // 현재 활성화된 monsterHpBar가 MonsterHpBarManager 스크립트 ShowNewHpBar 메서드에 매개변수로 전달
        if (MonsterHpBarManager.Instance != null)
        {
            MonsterHpBarManager.Instance.ShowNewHpBar(this);
        }
    }

    // 즉시 몬스터 HP 바를 비활성화 처리
    public void HideHpBarImmediately()
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }
        gameObject.SetActive(false);
    }

    // 지정된 시간 후에 몬스터 HP 바를 비활성화하는 코루틴
    private IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }
}

// 완성
