using UnityEngine;
using System;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(PlayerFsm), typeof(SkillFsm))]
public class FSMManager : MonoBehaviour
{
    public enum PlayerState
    {
        Idle,
        Moving,
        Attacking,    // 일반 공격 중
        Attack1,      // Attack1 상태 추가
        Attack2,      // Attack2 상태 추가
        UsingSkill,   // 스킬 사용 중
        Hit,
        Die,
        Rush,
        Parry,
        Skill1,
        Skill2
    }

    private PlayerFsm playerFsm;
    private SkillFsm skillFsm;

    // FSMManager가 현재 상태를 단일 관리합니다.
    public PlayerState currentState = PlayerState.Idle;

    private void Awake()
    {
        InitializeComponents();
        InitializeFSMManager();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        if (playerFsm != null)
            // playerFsm의 OnStateChanged 이벤트 대신 HandlePlayerStateChanged()를 직접 호출받습니다.
            playerFsm.OnAnimationEnd(""); // 임의 호출 X – 상태 전환은 PlayerFsm 내부에서 직접 HandlePlayerStateChanged()를 호출합니다.

        if (skillFsm != null)
            skillFsm.OnSkillTriggerRequested += HandleSkillTrigger;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (playerFsm != null)
            skillFsm.OnSkillTriggerRequested -= HandleSkillTrigger;
    }

    // 컴포넌트 초기화 메서드
    private void InitializeComponents()
    {
        playerFsm = GetComponent<PlayerFsm>();
        if (playerFsm == null)
        {
            Debug.LogError("FSMManager는 PlayerFsm을 참조해야 합니다.");
        }

        skillFsm = GetComponent<SkillFsm>();
        if (skillFsm == null)
        {
            Debug.LogError("FSMManager는 SkillFsm을 참조해야 합니다.");
        }
    }

    // FSMManager 초기화 메서드
    private void InitializeFSMManager()
    {
        if (playerFsm == null || skillFsm == null)
        {
            enabled = false;
            return;
        }
    }

    // 씬 로딩 시 호출되는 메서드
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InitializeComponents();
        InitializeFSMManager();

        // PlayerFsm 내부에서 상태 전환 시 직접 HandlePlayerStateChanged() 호출하므로 별도의 작업은 필요하지 않습니다.

        if (skillFsm != null)
            skillFsm.OnSkillTriggerRequested += HandleSkillTrigger;
    }

    // PlayerFsm의 상태 변경을 처리하는 메서드 (중앙 관리)
    public void HandlePlayerStateChanged(PlayerState newState)
    {
        currentState = newState;
        Debug.Log($"[FSMManager] 상태가 {newState}로 변경되었습니다.");
        // 필요 시 추가적인 상태 변경 처리 로직 구현
    }

    // SkillFsm에서 스킬 트리거 요청을 처리하는 메서드
    private void HandleSkillTrigger(SkillType skillType)
    {
        // 상태에 따른 스킬 사용 허용 여부 판단
        switch (skillType)
        {
            case SkillType.Rush:
                if (currentState == PlayerState.Hit || currentState == PlayerState.Die || currentState == PlayerState.UsingSkill)
                {
                    // 현재 Hit, Die, UsingSkill 상태이므로 Rush 스킬을 사용할 수 없습니다.
                    Debug.LogWarning("[FSMManager] 현재 상태에서는 Rush 스킬을 사용할 수 없습니다.");
                    return;
                }
                break;

            case SkillType.Parry:
            case SkillType.Skill1:
            case SkillType.Skill2:
                if (currentState != PlayerState.Idle)
                {
                    // 현재 Idle 상태가 아니므로 스킬을 사용할 수 없습니다.
                    Debug.LogWarning($"[FSMManager] 현재 상태({currentState})에서는 {skillType} 스킬을 사용할 수 없습니다.");
                    return;
                }
                break;
        }

        // Rush 스킬은 Moving 상태일 때만 사용 가능
        if (skillType == SkillType.Rush && currentState != PlayerState.Moving)
        {
            Debug.LogWarning("[FSMManager] Rush 스킬은 이동 중일 때만 사용할 수 있습니다.");
            return;
        }

        // Parry, Skill1, Skill2는 Idle 상태에서만 사용 가능
        if ((skillType == SkillType.Parry || skillType == SkillType.Skill1 || skillType == SkillType.Skill2) &&
            currentState != PlayerState.Idle)
        {
            Debug.LogWarning($"[FSMManager] {skillType} 스킬은 Idle 상태에서만 사용할 수 있습니다.");
            return;
        }

        // 조건을 만족하면 스킬 트리거
        skillFsm.TriggerSkill(skillType);
    }
}
