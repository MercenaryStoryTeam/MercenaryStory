using UnityEngine;

public class FSMManager : MonoBehaviour
{
    public enum PlayerState
    {
        Idle,
        Moving,
        Attack1,
        Attack2,
        Attack3,
        Hit,
        Die,
        Rush,
        Parry,
        Skill1,
        Skill2
    }

    public PlayerFsm playerFsm;
    public SkillFsm skillFsm;

    private PlayerState currentState = PlayerState.Idle;

    private void Awake()
    {
        if (playerFsm == null)
            playerFsm = GetComponent<PlayerFsm>();
        if (skillFsm == null)
            skillFsm = GetComponent<SkillFsm>();

        if (playerFsm == null || skillFsm == null)
        {
            Debug.LogError("FSMManager는 PlayerFsm과 SkillFsm을 참조해야 합니다.");
            enabled = false;
            return;
        }
    }

    private void OnEnable()
    {
        // PlayerFsm 상태 변경 이벤트 구독
        playerFsm.OnStateChanged += HandlePlayerStateChanged;

        // SkillFsm에서 스킬 사용 시 요청 받기
        skillFsm.OnSkillTriggerRequested += HandleSkillTrigger;
    }

    private void OnDisable()
    {
        playerFsm.OnStateChanged -= HandlePlayerStateChanged;
        skillFsm.OnSkillTriggerRequested -= HandleSkillTrigger;
    }

    private void HandlePlayerStateChanged(FSMManager.PlayerState newState)
    {
        currentState = newState;
    }

    private void HandleSkillTrigger(SkillType skillType)
    {
        // 상태에 따른 스킬 사용 허용 여부 판단
        switch (skillType)
        {
            case SkillType.Rush:
                if (currentState == PlayerState.Hit)
                {
                    skillFsm.LogWarning("[FSMManager] 현재 Hit 상태이므로 Rush 스킬을 사용할 수 없습니다.");
                    return;
                }
                break;

            case SkillType.Parry:
            case SkillType.Skill1:
            case SkillType.Skill2:
                if (currentState != PlayerState.Idle)
                {
                    skillFsm.LogWarning("[FSMManager] 현재 Idle 상태가 아니므로 공격 및 스킬을 사용할 수 없습니다.");
                    return;
                }
                break;
        }

        // 조건을 만족하면 스킬 트리거
        skillFsm.TriggerSkill(skillType);
    }
}
