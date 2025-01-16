using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using static SkillManager;

public class SkillManager : MonoBehaviour
{
    private Animator animator;

    //System.Serializable -> 인스펙터에서 Skill 내부 데이터 확인 가능 
    // name, triggerName, cooldown 등
    [System.Serializable]
    public class Skill
    {
        // 스킬 이름
        public string name;          

        // Animator 트리거 이름
        public string triggerName;  
        
        // 쿨다운
        public float cooldown;

        // HideInInspector -> 인스펙터에서 숨김 처리
        // 스킬이 쿨다운 상태인지 여부
        [HideInInspector] public bool isOnCooldown = false; 
    }

    // 스킬 리스트 (Inspector에서 설정 가능)
    [Header("스킬 설정")]
    public List<Skill> skills;

    private void Awake()
    {
        // Animator 컴포넌트 초기화
        animator = GetComponent<Animator>();

        // Animator가 존재하지 않으면 오류 출력
        if (animator == null)
        {
            Debug.LogError("Animator 컴포넌트가 없습니다.");
            enabled = false; // 스크립트 비활성화
        }
    }

    // PlayerInputManager에서 설정된 입력 이벤트에 
    // 트리거 연결
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
        // skills 리스트에서 name이 skillName인 스킬 찾기
        Skill skill = skills.Find(s => s.name == skillName);

        // 해당 스킬이 리스트에 등록되어 있지 않은 경우
        if (skill == null)
        {
            Debug.LogWarning($"{skillName} 스킬이 SkillManager에 등록되어 있지 않습니다.");
            return; // 실행 중단
        }

        // 스킬이 쿨다운 상태인지 확인
        if (skill.isOnCooldown)
        {
            Debug.Log($"{skillName} 스킬은 쿨다운 중입니다."); // 쿨다운 중 경고 출력
            return; // 실행 중단
        }

        // 스킬 발동: Animator에 설정된 트리거 활성화
        animator.SetTrigger(skill.triggerName);

        // 쿨다운 처리 코루틴 실행
        StartCoroutine(CooldownCoroutine(skill));
    }

    private IEnumerator CooldownCoroutine(Skill skill)
    {
        skill.isOnCooldown = true; // 스킬을 쿨다운 상태로 설정
        yield return new WaitForSeconds(skill.cooldown); // 쿨다운 시간만큼 대기
        skill.isOnCooldown = false; // 쿨다운 상태 해제
        Debug.Log($"{skill.name} 스킬 쿨다운 종료"); // 쿨다운 종료 로그 출력
    }
}
