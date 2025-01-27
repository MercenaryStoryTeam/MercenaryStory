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
        Attack1,
        Attack2,
        Hit,
        Die,
        Rush,
        Parry,
        Skill1,
        Skill2
    }

    private PlayerFsm playerFsm;
    private SkillFsm skillFsm;

    private PlayerState currentState = PlayerState.Idle;

    private void Awake()
    {
        InitializeComponents();
        InitializeFSMManager();
    }

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;

        if (playerFsm != null)
            playerFsm.OnStateChanged += HandlePlayerStateChanged;

        if (skillFsm != null)
            skillFsm.OnSkillTriggerRequested += HandleSkillTrigger;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;

        if (playerFsm != null)
            playerFsm.OnStateChanged -= HandlePlayerStateChanged;

        if (skillFsm != null)
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
        // 추가 초기화 로직이 필요하면 여기서 수행
    }

    // 씬 로딩 시 호출되는 메서드
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InitializeComponents();
        InitializeFSMManager();

        if (playerFsm != null)
            playerFsm.OnStateChanged += HandlePlayerStateChanged;

        if (skillFsm != null)
            skillFsm.OnSkillTriggerRequested += HandleSkillTrigger;
    }

    // PlayerFsm의 상태 변경을 처리하는 메서드
    private void HandlePlayerStateChanged(FSMManager.PlayerState newState)
    {
        currentState = newState;
    }

    // SkillFsm에서 스킬 트리거 요청을 처리하는 메서드
    private void HandleSkillTrigger(SkillType skillType)
    {
        // 상태에 따른 스킬 사용 허용 여부 판단
        switch (skillType)
        {
            case SkillType.Rush:
                if (currentState == PlayerState.Hit)
                {
                    // 현재 Hit 상태이므로 Rush 스킬을 사용할 수 없습니다.
                    return;
                }
                break;

            case SkillType.Parry:
            case SkillType.Skill1:
            case SkillType.Skill2:
                if (currentState != PlayerState.Idle)
                {
                    // 현재 Idle 상태가 아니므로 스킬을 사용할 수 없습니다.
                    return;
                }
                break;
        }

        // 조건을 만족하면 스킬 트리거
        skillFsm.TriggerSkill(skillType);
    }
}
