using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

// 스킬 유형 (추가 가능)
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
        [Header("스킬 유형")]
        public SkillType skillType;

        [Header("현재 스킬 레벨")]
        public int level = 1;

        [Header("최대 스킬 레벨")]
        public int maxLevel = 4;

        [Header("기본 쿨타임 (초 단위)")]
        public float baseCooldown = 2f;

        [Header("레벨당 쿨타임 감소 비율 (백분율)")]
        public int cooldownReductionPerLevel = 5;

        [Header("Rush 스킬만 해당 (이동 속도 배수 처리)")]
        public float speedBoost = 0f;

        [Header("Rush 스킬만 해당 (지속 시간)")]
        public float duration = 0f;

        [Header("스킬 설명")]
        [TextArea]
        public string description;

        [Header("스킬 이미지")]
        public Sprite icon; // 스킬 이미지 필드 추가

        [Header("레벨에 따른 이펙트 리스트")]
        public List<GameObject> particleEffects;

        [Header("쿨타임바")]
        public Image cooldownImage;

        // 쿨타임 작동 여부 체크를 위한 변수
        [HideInInspector]
        public bool isOnCooldown = false;

        // 현재 쿨타임 남은 시간
        [HideInInspector]
        public float remainingCooldown = 0f;

        // 스킬 이름 반환
        public string Name
        {
            get { return skillType.ToString(); }
        }

        // 애니메이터 트리거 이름 반환
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

        // 현재 레벨에 따른 쿨타임 계산
        public float CurrentCooldown
        {
            get
            {
                // cooldown = baseCooldown * (1 - (cooldownReductionPerLevel / 100))^(level -1)
                float reduction = cooldownReductionPerLevel / 100f;
                float calculatedCooldown = baseCooldown * Mathf.Pow(1 - reduction, level - 1);

                // Debug log for cooldown calculation
                Debug.Log($"[Skill] {Name} Level: {level}, Base Cooldown: {baseCooldown}s, " +
                          $"Reduction per Level: {cooldownReductionPerLevel}%, " +
                          $"Calculated Cooldown: {calculatedCooldown:F2}s");

                return calculatedCooldown;
            }
        }

        // 현재 레벨에 따른 파티클 이펙트 반환
        public GameObject GetCurrentParticleEffect()
        {
            if (particleEffects == null || particleEffects.Count == 0)
                return null;

            // 레벨 인덱스가 리스트 범위를 넘지 않도록 조정
            int index = Mathf.Clamp(level - 1, 0, particleEffects.Count - 1);
            return particleEffects[index];
        }

        // 스킬 레벨업 메서드
        public bool LevelUp()
        {
            if (level >= maxLevel)
            {
                Debug.Log($"[Skill] {Name} 스킬은 이미 최대 레벨({maxLevel})에 도달했습니다.");
                return false;
            }

            level++;
            Debug.Log($"[Skill] {Name} 스킬이 레벨 {level}로 상승했습니다.");

            // 추가 디버그: 새로운 쿨타임 계산
            float newCooldown = CurrentCooldown;
            Debug.Log($"[Skill] {Name} 스킬의 새로운 쿨타임은 {newCooldown:F2}초입니다.");

            return true;
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
                skill.cooldownImage.fillAmount = 0f;
                Debug.Log($"[SkillFsm] {skill.Name} 스킬의 쿨타임바가 초기화되었습니다.");
            }
        }
    }

    private void OnEnable()
    {
        PlayerInputManager.OnSkillInput += OnSkillInput;
        PlayerInputManager.OnRightClickInput += OnRightClickInput;
        PlayerInputManager.OnShiftLeftClickInput += OnShiftLeftClickInput;
        PlayerInputManager.OnShiftRightClickInput += OnShiftRightClickInput;
    }

    private void OnDisable()
    {
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

    // 특정 스킬을 트리거하는 메서드
    public void TriggerSkill(SkillType skillType)
    {
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
            Debug.Log($"[SkillFsm] {skill.Name} 스킬은 쿨다운 중입니다. 남은 시간: {skill.remainingCooldown:F2}초");
            return;
        }

        // 스킬 실행
        animator.SetTrigger(skill.TriggerName);
        Debug.Log($"[SkillFsm] {skill.Name} 스킬이 트리거되었습니다.");

        // 레벨에 따른 파티클 이펙트 활성화
        GameObject currentEffect = skill.GetCurrentParticleEffect();
        if (currentEffect != null)
        {
            ActivateSkillParticle(currentEffect);
            Debug.Log($"[SkillFsm] {skill.Name} 스킬의 파티클 이펙트가 활성화되었습니다.");
        }

        // Rush 스킬이면 이동 속도 증가
        if (skillType == SkillType.Rush)
        {
            ApplySpeedBoost(skill.speedBoost, skill.duration);
        }

        // 쿨다운 시작
        StartCoroutine(CooldownCoroutine(skill));
    }

    // 파티클 이펙트 활성화 메서드
    private void ActivateSkillParticle(GameObject particleEffect)
    {
        // 1) 파티클 위치 조정
        Vector3 relativePosition = particleEffect.transform.localPosition;
        Vector3 finalPosition = transform.TransformPoint(relativePosition);

        // 2) 최종 위치 적용
        particleEffect.transform.position = finalPosition;

        // 3) 파티클 활성화
        particleEffect.SetActive(true);

        // 4) 파티클 재생 시간 뒤 비활성화
        ParticleSystem ps = particleEffect.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            float duration = ps.main.duration + ps.main.startLifetime.constantMax;
            StartCoroutine(DeactivateParticleAfterDuration(particleEffect, duration));
            Debug.Log($"[SkillFsm] {particleEffect.name} 파티클 이펙트가 {duration}초 후에 비활성화됩니다.");
        }
    }

    private IEnumerator DeactivateParticleAfterDuration(GameObject particleEffect, float duration)
    {
        yield return new WaitForSeconds(duration);
        particleEffect.SetActive(false);
        Debug.Log($"[SkillFsm] {particleEffect.name} 파티클 이펙트가 비활성화되었습니다.");
    }

    // 이동 속도 증가 메서드
    private void ApplySpeedBoost(float speedBoost, float duration)
    {
        if (isSpeedBoostActive) return;
        StartCoroutine(SpeedBoostCoroutine(speedBoost, duration));
    }

    // Rush 스킬의 이동 속도 증가 코루틴
    private IEnumerator SpeedBoostCoroutine(float speedBoost, float duration)
    {
        isSpeedBoostActive = true;

        float originalSpeed = player.moveSpeed;
        player.moveSpeed *= speedBoost;
        Debug.Log($"[SkillFsm] Rush 이동 속도 상승 시작 ({speedBoost}배, {duration}초). " +
                  $"원래 속도: {originalSpeed}, 새로운 속도: {player.moveSpeed}");

        yield return new WaitForSeconds(duration);

        player.moveSpeed = originalSpeed;
        isSpeedBoostActive = false;
        Debug.Log($"[SkillFsm] Rush 이동 속도 상승 종료. 속도 복원: {player.moveSpeed}");
    }

    // 쿨다운 코루틴
    private IEnumerator CooldownCoroutine(Skill skill)
    {
        skill.isOnCooldown = true;
        skill.remainingCooldown = skill.CurrentCooldown;

        // 쿨타임바 활성화 및 초기화
        if (skill.cooldownImage != null)
        {
            skill.cooldownImage.fillAmount = 1f;
            Debug.Log($"[CooldownCoroutine] {skill.Name} 스킬의 쿨타임바가 활성화되었습니다.");
        }

        float elapsed = 0f;
        float totalCooldown = skill.CurrentCooldown;

        Debug.Log($"[CooldownCoroutine] {skill.Name} 스킬 쿨타임 시작: {totalCooldown:F2}초");

        while (elapsed < totalCooldown)
        {
            elapsed += Time.deltaTime;
            skill.remainingCooldown = Mathf.Max(totalCooldown - elapsed, 0f);

            if (skill.cooldownImage != null)
            {
                skill.cooldownImage.fillAmount = 1f - (elapsed / totalCooldown);
            }

            // 디버그 로그 추가
            Debug.Log($"[CooldownCoroutine] {skill.Name} remainingCooldown: {skill.remainingCooldown:F2} sec");

            yield return null;
        }

        // 쿨타임 종료
        skill.isOnCooldown = false;
        skill.remainingCooldown = 0f;
        Debug.Log($"[SkillFsm] {skill.Name} 스킬 쿨다운 종료");

        // 쿨타임바 비활성화
        if (skill.cooldownImage != null)
        {
            skill.cooldownImage.fillAmount = 0f;
            Debug.Log($"[CooldownCoroutine] {skill.Name} 스킬의 쿨타임바가 비활성화되었습니다.");
        }
    }

    // Animator 설정 메서드
    public void SetAnimator(Animator newAnimator)
    {
        if (newAnimator == null)
        {
            Debug.LogError("Animator가 null입니다. 설정할 수 없습니다.");
            return;
        }

        animator = newAnimator;
        Debug.Log("[SkillFsm] 새로운 Animator가 설정되었습니다.");
    }

    // 특정 SkillType에 해당하는 Skill 객체 반환
    public Skill GetSkill(SkillType skillType)
    {
        Skill skill = skills.Find(s => s.skillType == skillType);
        if (skill != null)
        {
            Debug.Log($"[SkillFsm] {skillType} 스킬이 검색되었습니다.");
        }
        else
        {
            Debug.LogWarning($"[SkillFsm] {skillType} 스킬이 존재하지 않습니다.");
        }
        return skill;
    }

    // 스킬 레벨업 메서드
    public bool LevelUpSkill(SkillType skillType)
    {
        Skill skill = skills.Find(s => s.skillType == skillType);
        if (skill == null)
        {
            Debug.LogWarning($"[SkillFsm] {skillType} 스킬이 등록되어 있지 않습니다.");
            return false;
        }

        bool leveledUp = skill.LevelUp();
        if (leveledUp)
        {
            // 레벨업에 따른 스킬 속성 조정
            AdjustSkillAttributes(skill);
            Debug.Log($"[SkillFsm] {skill.Name} 스킬의 속성이 레벨업에 따라 조정되었습니다.");
            return true;
        }

        return false;
    }

    // 스킬 레벨업에 따른 속성 조정 메서드
    private void AdjustSkillAttributes(Skill skill)
    {
        switch (skill.skillType)
        {
            case SkillType.Rush:
                Debug.Log($"[SkillFsm] {skill.Name} 스킬의 speedBoost가 {skill.speedBoost:F2}로 증가되었습니다.");
                break;
            case SkillType.Parry:
                // Parry 스킬의 레벨업 로직 추가 (예: 효과 강화)
                // 현재 예에서는 쿨타임 자동 조정되므로 추가 로직 없음
                Debug.Log($"[SkillFsm] {skill.Name} 스킬의 레벨업에 따른 추가 로직이 없습니다.");
                break;
            case SkillType.Skill1:
                // Skill1 스킬의 레벨업 로직 추가
                Debug.Log($"[SkillFsm] {skill.Name} 스킬의 레벨업에 따른 추가 로직이 없습니다.");
                break;
            case SkillType.Skill2:
                // Skill2 스킬의 레벨업 로직 추가
                Debug.Log($"[SkillFsm] {skill.Name} 스킬의 레벨업에 따른 추가 로직이 없습니다.");
                break;
        }

        // 필요 시 추가적인 속성 조정 가능
    }
}
