using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkillFsm : MonoBehaviour
{
    private Animator animator;

    [System.Serializable]
    public class Skill
    {
        public string name;           // 스킬 이름
        public string triggerName;    // Animator 트리거 이름
        public float cooldown;        // 쿨다운 시간
        public float speedBoost = 1f; // 이동 속도 증가 배율
        public float duration = 0f;   // 이동 속도 증가 지속 시간
        public GameObject particleEffect; // 비활성화된 파티클 오브젝트 (프리팹 대신)

        [HideInInspector] public bool isOnCooldown = false; // 쿨다운 상태
    }

    [Header("스킬 설정")]
    public List<Skill> skills;

    private bool isSpeedBoostActive = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError("Animator 컴포넌트가 없습니다.");
            enabled = false;
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

        // 파티클 효과 활성화
        if (skill.particleEffect != null)
        {
            ActivateSkillParticle(skill.particleEffect, transform.position);
        }

        // Rush 스킬의 경우 이동 속도 증가 효과 적용
        if (skillName == "Rush")
        {
            ApplySpeedBoost(skill.speedBoost, skill.duration);
        }

        StartCoroutine(CooldownCoroutine(skill));
    }

    private void ActivateSkillParticle(GameObject particleEffect, Vector3 position)
    {
        // 비활성화된 파티클 오브젝트를 활성화
        particleEffect.transform.position = position;
        particleEffect.SetActive(true);

        // 파티클 재생 시간 이후 비활성화
        ParticleSystem ps = particleEffect.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            StartCoroutine(DeactivateParticleAfterDuration(particleEffect, ps.main.duration + ps.main.startLifetime.constantMax));
        }
    }

    private IEnumerator DeactivateParticleAfterDuration(GameObject particleEffect, float duration)
    {
        yield return new WaitForSeconds(duration);
        particleEffect.SetActive(false); // 다시 비활성화
    }

    private void ApplySpeedBoost(float speedBoost, float duration)
    {
        if (isSpeedBoostActive) return;

        Debug.Log("Rush 이동 속도 상승 효과 시작");
        StartCoroutine(SpeedBoostCoroutine(speedBoost, duration));
    }

    private IEnumerator SpeedBoostCoroutine(float speedBoost, float duration)
    {
        isSpeedBoostActive = true;

        float originalSpeed = PlayerData.Instance.moveSpeed;
        PlayerData.Instance.moveSpeed *= speedBoost;

        yield return new WaitForSeconds(duration);

        PlayerData.Instance.moveSpeed = originalSpeed;
        isSpeedBoostActive = false;
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

//
