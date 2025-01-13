using UnityEngine;

// 가장 최근에 플레이어에게 데미지를 받은 몬스터의 HP 바만 표시하도록 관리하는 매니저
public class MonsterHpBarManager : MonoBehaviour
{
    // Singleton 패턴
    public static MonsterHpBarManager Instance { get; private set; }

    // 현재 활성화된 HP 바
    private MonsterHpBar monsterHpBar;

    // 씬 전환 시 오브젝트가 중복 생성을 방지하는 것이 목적
    private void Awake()
    {
        // Instance가 비어있을 경우 현재 인스턴스를 Singleton 인스턴스로 설정
        if (Instance == null)
        {
            Instance = this;
        }
        // Instance가 이미 존재할 경우 중복 인스턴스를 제거
        else
        {
            Destroy(gameObject);
        }
    }

    // 새로운 monsterHpBar를 활성화하고 기존 monsterHpBar를 비활성화 처리
    public void ShowNewHpBar(MonsterHpBar newHpBar) // 현재 활성화된 monsterHpBar를 매개변수로 전달받아 기존 monsterHpBar으로 처리
                                                    // 여러 몬스터 중 특정 몬스터 객체의 monsterHpBar를 표시할지 지정하는 역할
    {
        // 기존 monsterHpBar가 존재하고, 새로운 monsterHpBar와 다를 경우
        if (monsterHpBar != null && monsterHpBar != newHpBar)
        {
            // 기존 monsterHpBar를 비활성화 처리
            monsterHpBar.HideHpBarImmediately();
        }

        // 기존 monsterHpBar를 새로운 monsterHpBar로 업데이트
        monsterHpBar = newHpBar;

        // 새로운 monsterHpBar를 표시
        monsterHpBar.ShowHpBar();
    }

}

// 완성
