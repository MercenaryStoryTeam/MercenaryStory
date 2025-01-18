using UnityEngine;
using UnityEngine.UI; // UI 관련 네임스페이스 추가
using System.Collections;
using System.Collections.Generic;

// 스킬 유형을 정의하는 Enum
public enum SkillType
{
    Rush,
    Parry,
    Skill1,
    Skill2
}

public class SkillFsm : MonoBehaviour
{
    private Animator animator;

    [System.Serializable]
    public class Skill
    {
        public SkillType skillType; // 드롭다운으로 선택할 스킬 유형

        public float cooldown;
        public float speedBoost = 1f;
        public float duration = 0f;
        public GameObject particleEffect;

        [Header("쿨타임바")]
        public Image cooldownImage; // 쿨타임바 이미지 추가

        [HideInInspector] public bool isOnCooldown = false;

        // 선택된 SkillType에 따라 스킬 이름 반환
        public string Name
        {
            get { return skillType.ToString(); }
        }

        // 선택된 SkillType에 따라 트리거 이름 반환
        public string TriggerName
        {
            get
            {
                switch (skillType)
                {
                    case SkillType.Rush:
                        return "Rush";
                    case SkillType.Parry:
                        return "Parry";
                    case SkillType.Skill1:
                        return "Skill1";
                    case SkillType.Skill2:
                        return "Skill2";
                    default:
                        return "";
                }
            }
        }
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

        // 초기 쿨타임바 설정
        foreach (var skill in skills)
        {
            if (skill.cooldownImage != null)
            {
                skill.cooldownImage.fillAmount = 0f; // 스킬 사용 전 쿨타임바 비움
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
        TriggerSkill(SkillType.Rush);
    }

    private void OnRightClickInput()
    {
        TriggerSkill(SkillType.Parry);
    }

    private void OnShiftLeftClickInput()
    {
        TriggerSkill(SkillType.Skill1);
    }

    private void OnShiftRightClickInput()
    {
        TriggerSkill(SkillType.Skill2);
    }

    public void TriggerSkill(SkillType skillType)
    {
        // Animator null 체크
        if (animator == null)
        {
            Debug.LogWarning("[SkillFsm] Animator가 null이므로 스킬을 사용할 수 없습니다.");
            return;
        }

        Skill skill = skills.Find(s => s.skillType == skillType);
        if (skill == null)
        {
            Debug.LogWarning($"[SkillFsm] {skillType} 스킬이 등록되어 있지 않습니다.");
            return;
        }

        if (skill.isOnCooldown)
        {
            Debug.Log($"[SkillFsm] {skill.Name} 스킬은 쿨다운 중입니다.");
            return;
        }

        // 스킬 실행
        animator.SetTrigger(skill.TriggerName);

        // 파티클 이펙트 활성화
        if (skill.particleEffect != null)
        {
            ActivateSkillParticle(skill.particleEffect);
        }

        // Rush 스킬이면 이동 속도 증가
        if (skillType == SkillType.Rush)
        {
            ApplySpeedBoost(skill.speedBoost, skill.duration);
        }

        // 쿨다운 시작
        StartCoroutine(CooldownCoroutine(skill));
    }

    private void ActivateSkillParticle(GameObject particleEffect)
    {
        // 1) 파티클 오브젝트가 가진 localPosition 값(오프셋)을 부모 트랜스폼 기준의 월드 좌표로 변환
        Vector3 localOffset = particleEffect.transform.localPosition;
        Vector3 finalPosition = transform.TransformPoint(localOffset);

        // 2) 최종 위치 적용
        particleEffect.transform.position = finalPosition;

        // 3) 파티클 오브젝트 활성화
        particleEffect.SetActive(true);

        // 4) 파티클 재생 시간 뒤 비활성화
        ParticleSystem ps = particleEffect.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            float duration = ps.main.duration + ps.main.startLifetime.constantMax;
            StartCoroutine(DeactivateParticleAfterDuration(particleEffect, duration));
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

        // 쿨타임바 활성화 및 초기화
        if (skill.cooldownImage != null)
        {
            skill.cooldownImage.fillAmount = 1f; // 쿨타임 시작 시 채움
        }

        float elapsed = 0f;
        float totalCooldown = skill.cooldown;

        while (elapsed < totalCooldown)
        {
            elapsed += Time.deltaTime;
            if (skill.cooldownImage != null)
            {
                skill.cooldownImage.fillAmount = 1f - (elapsed / totalCooldown);
            }
            yield return null;
        }

        // 쿨타임 종료
        skill.isOnCooldown = false;
        Debug.Log($"[SkillFsm] {skill.Name} 스킬 쿨다운 종료");

        // 쿨타임바 비활성화
        if (skill.cooldownImage != null)
        {
            skill.cooldownImage.fillAmount = 0f;
        }
    }

    // 플레이어 객체가 재활성화되었을 때 새로운 Animator를 받아올 수 있도록 하는 메서드
    public void SetAnimator(Animator newAnimator)
    {
        if (newAnimator == null)
        {
            Debug.LogError("Animator가 null입니다. 설정할 수 없습니다.");
            return;
        }

        animator = newAnimator;
    }
}
