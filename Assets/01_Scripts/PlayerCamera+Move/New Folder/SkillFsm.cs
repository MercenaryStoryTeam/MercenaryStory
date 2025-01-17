using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkillFsm : MonoBehaviour
{
    private Animator animator;

    [System.Serializable]
    public class Skill
    {
        public string name;          
        public string triggerName;  
        public float cooldown;       
        public float speedBoost = 1f;
        public float duration = 0f;
        public GameObject particleEffect;

        [HideInInspector] public bool isOnCooldown = false;
    }

    [Header("스킬 설정")]
    public List<Skill> skills;

    private bool isSpeedBoostActive = false;

    [Header("참조 설정")]
    [SerializeField] private Player player;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("[SkillFsm] Animator가 없습니다.");
            enabled = false;
            return;
        }

        if (player == null)
        {
            player = FindObjectOfType<Player>();
            if (player == null)
            {
                Debug.LogError("[SkillFsm] Player 오브젝트를 찾을 수 없습니다.");
                enabled = false;
                return;
            }
        }
    }

    private void OnEnable()
    {
        // 반드시 메서드 형식으로 구독
        PlayerInputManager.OnSkillInput += OnSkillInput;
        PlayerInputManager.OnRightClickInput += OnRightClickInput;
        PlayerInputManager.OnShiftLeftClickInput += OnShiftLeftClickInput;
        PlayerInputManager.OnShiftRightClickInput += OnShiftRightClickInput;
    }

    private void OnDisable()
    {
        // 반드시 구독 해제
        PlayerInputManager.OnSkillInput -= OnSkillInput;
        PlayerInputManager.OnRightClickInput -= OnRightClickInput;
        PlayerInputManager.OnShiftLeftClickInput -= OnShiftLeftClickInput;
        PlayerInputManager.OnShiftRightClickInput -= OnShiftRightClickInput;
    }

    private void OnSkillInput()
    {
        TriggerSkill("Rush");
    }

    private void OnRightClickInput()
    {
        TriggerSkill("Parry");
    }

    private void OnShiftLeftClickInput()
    {
        TriggerSkill("Skill1");
    }

    private void OnShiftRightClickInput()
    {
        TriggerSkill("Skill2");
    }

    public void TriggerSkill(string skillName)
    {
        // 이미 Destroy된 Animator가 아니라면 null 체크
        if (animator == null)
        {
            Debug.LogWarning("[SkillFsm] Animator가 null이므로 스킬을 사용할 수 없습니다.");
            return;
        }

        Skill skill = skills.Find(s => s.name == skillName);
        if (skill == null)
        {
            Debug.LogWarning($"[SkillFsm] {skillName} 스킬이 등록되어 있지 않습니다.");
            return;
        }

        if (skill.isOnCooldown)
        {
            Debug.Log($"[SkillFsm] {skillName} 스킬은 쿨다운 중입니다.");
            return;
        }

        // 스킬 실행
        animator.SetTrigger(skill.triggerName);

        // 파티클 이펙트 활성화
        if (skill.particleEffect != null)
        {
            ActivateSkillParticle(skill.particleEffect, transform.position);
        }

        // Rush 스킬이면 이동 속도 증가
        if (skillName == "Rush")
        {
            ApplySpeedBoost(skill.speedBoost, skill.duration);
        }

        // 쿨다운
        StartCoroutine(CooldownCoroutine(skill));
    }

    private void ActivateSkillParticle(GameObject particleEffect, Vector3 position)
    {
        particleEffect.transform.position = position;
        particleEffect.SetActive(true);

        ParticleSystem ps = particleEffect.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            StartCoroutine(DeactivateParticleAfterDuration(particleEffect, ps.main.duration + ps.main.startLifetime.constantMax));
        }
    }

    private IEnumerator DeactivateParticleAfterDuration(GameObject particleEffect, float duration)
    {
        yield return new WaitForSeconds(duration);
        particleEffect.SetActive(false);
    }

    private void ApplySpeedBoost(float speedBoost, float duration)
    {
        if (isSpeedBoostActive) return;
        StartCoroutine(SpeedBoostCoroutine(speedBoost, duration));
    }

    private IEnumerator SpeedBoostCoroutine(float speedBoost, float duration)
    {
        isSpeedBoostActive = true;

        float originalSpeed = player.moveSpeed;
        player.moveSpeed *= speedBoost;
        Debug.Log($"[SkillFsm] Rush 이동 속도 상승 시작 ({speedBoost}배, {duration}초)");

        yield return new WaitForSeconds(duration);

        player.moveSpeed = originalSpeed;
        isSpeedBoostActive = false;
        Debug.Log("[SkillFsm] Rush 이동 속도 상승 종료");
    }

    private IEnumerator CooldownCoroutine(Skill skill)
    {
        skill.isOnCooldown = true;
        yield return new WaitForSeconds(skill.cooldown);
        skill.isOnCooldown = false;
        Debug.Log($"[SkillFsm] {skill.name} 스킬 쿨다운 종료");
    }

    /// <summary>
    /// 부활 또는 새 씬에서 새 Animator를 받아올 수 있도록 하는 메서드 (필요시 사용)
    /// </summary>
    public void SetAnimator(Animator newAnimator)
    {
        animator = newAnimator;
    }
}
