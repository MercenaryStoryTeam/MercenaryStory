using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkillFsm : MonoBehaviour
{
    private Animator animator;

    // System.Serializable -> 인스펙터에서 Skill 내부 데이터 확인 가능 
    [System.Serializable]
    public class Skill
    {
        public string name;           // 스킬 이름
        public string triggerName;    // Animator 트리거 이름
        public float cooldown;        // 쿨다운 시간
        public float speedBoost = 1f; // 이동 속도 증가 배율 (예: 1.5f는 1.5배 속도)
        public float duration = 0f;   // 이동 속도 증가 지속 시간

        [HideInInspector] public bool isOnCooldown = false; // 쿨다운 상태
    }

    [Header("스킬 설정")]
    public List<Skill> skills;

    private bool isSpeedBoostActive = false; // 이동 속도 중첩 방지 플래그

    private void Awake()
    {
        animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError("Animator 컴포넌트가 없습니다.");
            enabled = false; // 스크립트 비활성화
        }
    }

    private void OnEnable()
    {
        PlayerInputManager.OnSkillInput += () => TriggerSkill("Rush");
        PlayerInputManager.OnRightClickInput += () => TriggerSkill("Parry");
        PlayerInputManager.OnShiftLeftClickInput += () => TriggerSkill("Skill1");
        PlayerInputManager.OnShiftRightClickInput += () => TriggerSkill("Skill2");
    }

    private void OnDisable()
    {
        PlayerInputManager.OnSkillInput -= () => TriggerSkill("Rush");
        PlayerInputManager.OnRightClickInput -= () => TriggerSkill("Parry");
        PlayerInputManager.OnShiftLeftClickInput -= () => TriggerSkill("Skill1");
        PlayerInputManager.OnShiftRightClickInput -= () => TriggerSkill("Skill2");
    }

    private void TriggerSkill(string skillName)
    {
        Skill skill = skills.Find(s => s.name == skillName);

        if (skill == null)
        {
            Debug.LogWarning($"{skillName} 스킬이 SkillManager에 등록되어 있지 않습니다.");
            return;
        }

        if (skill.isOnCooldown)
        {
            Debug.Log($"{skillName} 스킬은 쿨다운 중입니다.");
            return;
        }

        animator.SetTrigger(skill.triggerName);

        // Rush 스킬의 경우 이동 속도 증가 효과 적용
        if (skillName == "Rush")
        {
            ApplySpeedBoost(skill.speedBoost, skill.duration);
        }

        StartCoroutine(CooldownCoroutine(skill));
    }

    private void ApplySpeedBoost(float speedBoost, float duration)
    {
        if (isSpeedBoostActive) return; // 이미 이동 속도 증가가 활성화된 경우 무시

        Debug.Log("Rush 이동 속도 상승 효과 시작");

        // 코루틴 실행으로 이동 속도 복원 처리
        StartCoroutine(SpeedBoostCoroutine(speedBoost, duration));
    }

    private IEnumerator SpeedBoostCoroutine(float speedBoost, float duration)
    {
        isSpeedBoostActive = true; // 이동 속도 증가 활성화

        // 기존 속도 저장 및 증가 적용
        float originalSpeed = PlayerData.Instance.moveSpeed;
        PlayerData.Instance.moveSpeed *= speedBoost;

        // 지속 시간 대기
        yield return new WaitForSeconds(duration);

        // 이동 속도 복원
        PlayerData.Instance.moveSpeed = originalSpeed;
        isSpeedBoostActive = false; // 이동 속도 증가 비활성화
        Debug.Log("Rush 이동 속도 상승 효과 종료");
    }

    private IEnumerator CooldownCoroutine(Skill skill)
    {
        skill.isOnCooldown = true;
        yield return new WaitForSeconds(skill.cooldown);
        skill.isOnCooldown = false;
        Debug.Log($"{skill.name} 스킬 쿨다운 종료");
    }
}
