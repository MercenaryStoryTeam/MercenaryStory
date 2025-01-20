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

[RequireComponent(typeof(Animator))]
public class SkillFsm : MonoBehaviour
{
    private Animator animator;

    [System.Serializable]
    public class Skill
    {
        [Header("스킬 유형")]
        public SkillType skillType;

        [Header("현재 스킬 레벨")]
        [SerializeField]
        private int level = 1;
        public int Level
        {
            get => level;
            private set
            {
                if (value <= MaxLevel)
                {
                    level = value;
                }
                else
                {
                    level = MaxLevel;
                    if (SkillFsm.Instance != null && SkillFsm.Instance.enableDebugLogs)
                    {
                        Debug.LogWarning($"[Skill] {Name} 스킬은 이미 최대 레벨({MaxLevel})에 도달했습니다.");
                    }
                }
            }
        }

        [Header("최대 스킬 레벨")]
        public int MaxLevel = 4;

        [Header("기본 쿨타임 (초 단위)")]
        public float BaseCooldown = 2f;

        [Header("레벨당 쿨타임 감소 비율 (백분율)")]
        public int CooldownReductionPerLevel = 5;

        [Header("Rush 스킬만 해당 (이동 속도 배수 처리)")]
        public float SpeedBoost = 0f;

        [Header("Rush 스킬만 해당 (지속 시간)")]
        public float Duration = 0f;

        [Header("스킬 설명")]
        [TextArea]
        public string Description;

        [Header("스킬 이미지")]
        public Sprite Icon; // 스킬 이미지 필드 추가

        [Header("레벨에 따른 이펙트 리스트")]
        public List<GameObject> ParticleEffects;

        [Header("쿨타임바")]
        public Image CooldownImage;

        // 쿨타임 작동 여부 체크를 위한 변수
        [HideInInspector]
        public bool IsOnCooldown = false;

        // 현재 쿨타임 남은 시간
        [HideInInspector]
        public float RemainingCooldown = 0f;

        // 스킬 이름 반환
        public string Name => skillType.ToString();

        // 애니메이터 트리거 이름 반환
        public string TriggerName
        {
            get
            {
                return skillType.ToString();
            }
        }

        // 캐싱된 쿨타임
        [HideInInspector]
        public float CachedCooldown;

        // 현재 레벨에 따른 파티클 이펙트 반환
        public GameObject GetCurrentParticleEffect()
        {
            if (ParticleEffects == null || ParticleEffects.Count == 0)
                return null;

            // 레벨 인덱스가 리스트 범위를 넘지 않도록 조정
            int index = Mathf.Clamp(Level - 1, 0, ParticleEffects.Count - 1);
            return ParticleEffects[index];
        }

        // 스킬 레벨업 메서드
        public bool LevelUp()
        {
            if (Level >= MaxLevel)
            {
                if (SkillFsm.Instance != null && SkillFsm.Instance.enableDebugLogs)
                {
                    Debug.LogWarning($"[Skill] {Name} 스킬은 이미 최대 레벨({MaxLevel})에 도달했습니다.");
                }
                return false;
            }

            Level++;
            if (SkillFsm.Instance != null && SkillFsm.Instance.enableDebugLogs)
            {
                Debug.Log($"[Skill] {Name} 스킬이 레벨 {Level}로 상승했습니다.");
            }

            // 쿨타임 업데이트
            UpdateCooldown();

            return true;
        }

        // 현재 레벨에 따른 쿨타임 계산 및 캐싱
        public void UpdateCooldown()
        {
            float reduction = CooldownReductionPerLevel / 100f;
            CachedCooldown = BaseCooldown * Mathf.Pow(1 - reduction, Level - 1);
            if (SkillFsm.Instance != null && SkillFsm.Instance.enableDebugLogs)
            {
                Debug.Log($"[Skill] {Name} Level: {Level}, Base Cooldown: {BaseCooldown}s, " +
                          $"Reduction per Level: {CooldownReductionPerLevel}%, " +
                          $"Calculated Cooldown: {CachedCooldown:F2}s");
            }
        }
    }

    [Header("스킬 설정")]
    public List<Skill> Skills;

    private bool isSpeedBoostActive = false;

    [Header("참조 설정")]
    [SerializeField] private Player player;

    [Header("디버그 설정")]
    public bool enableDebugLogs = true;

    // 싱글톤 인스턴스 (필요 시)
    public static SkillFsm Instance { get; private set; }

    private void Awake()
    {
        // 싱글톤 패턴 설정 (필요 시)
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;

        animator = GetComponent<Animator>();

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

        // 초기 쿨타임 설정
        foreach (var skill in Skills)
        {
            skill.UpdateCooldown();
            if (skill.CooldownImage != null)
            {
                skill.CooldownImage.fillAmount = 0f;
                if (enableDebugLogs)
                {
                    Debug.Log($"[SkillFsm] {skill.Name} 스킬의 쿨타임바가 초기화되었습니다.");
                }
            }
        }
    }

    private void OnEnable()
    {
        // 입력 이벤트 구독 (중복 메서드 제거, 람다 사용)
        PlayerInputManager.OnSkillInput += () => TriggerSkill(SkillType.Rush);
        PlayerInputManager.OnRightClickInput += () => TriggerSkill(SkillType.Parry);
        PlayerInputManager.OnShiftLeftClickInput += () => TriggerSkill(SkillType.Skill1);
        PlayerInputManager.OnShiftRightClickInput += () => TriggerSkill(SkillType.Skill2);
    }

    private void OnDisable()
    {
        // 입력 이벤트 해제
        PlayerInputManager.OnSkillInput -= () => TriggerSkill(SkillType.Rush);
        PlayerInputManager.OnRightClickInput -= () => TriggerSkill(SkillType.Parry);
        PlayerInputManager.OnShiftLeftClickInput -= () => TriggerSkill(SkillType.Skill1);
        PlayerInputManager.OnShiftRightClickInput -= () => TriggerSkill(SkillType.Skill2);
    }

    // 특정 스킬을 트리거하는 메서드
    public void TriggerSkill(SkillType skillType)
    {
        if (animator == null)
        {
            if (enableDebugLogs)
            {
                Debug.LogWarning("[SkillFsm] Animator가 null이므로 스킬을 사용할 수 없습니다.");
            }
            return;
        }

        Skill skill = GetSkill(skillType);
        if (skill == null)
        {
            if (enableDebugLogs)
            {
                Debug.LogWarning($"[SkillFsm] {skillType} 스킬이 등록되어 있지 않습니다.");
            }
            return;
        }

        if (skill.IsOnCooldown)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[SkillFsm] {skill.Name} 스킬은 쿨다운 중입니다. 남은 시간: {skill.RemainingCooldown:F2}초");
            }
            return;
        }

        // 스킬 실행
        animator.SetTrigger(skill.TriggerName);
        if (enableDebugLogs)
        {
            Debug.Log($"[SkillFsm] {skill.Name} 스킬이 트리거되었습니다.");
        }

        // 레벨에 따른 파티클 이펙트 활성화
        GameObject currentEffect = skill.GetCurrentParticleEffect();
        if (currentEffect != null)
        {
            ActivateSkillParticle(currentEffect);
            if (enableDebugLogs)
            {
                Debug.Log($"[SkillFsm] {skill.Name} 스킬의 파티클 이펙트가 활성화되었습니다.");
            }
        }

        // Rush 스킬이면 이동 속도 증가
        if (skillType == SkillType.Rush)
        {
            ApplySpeedBoost(skill.SpeedBoost, skill.Duration);
        }

        // 쿨다운 시작
        StartCoroutine(CooldownCoroutine(skill));
    }

    // 파티클 이펙트 활성화 메서드
    private void ActivateSkillParticle(GameObject particleEffect)
    {
        // 파티클 이펙트를 인스턴스화하여 위치 설정
        Vector3 finalPosition = transform.TransformPoint(particleEffect.transform.localPosition);
        GameObject particleInstance = Instantiate(particleEffect, finalPosition, Quaternion.identity, transform);
        particleInstance.SetActive(true);

        // 파티클 시스템의 재생 시간 계산 후 자동 파괴
        ParticleSystem ps = particleInstance.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            float duration = ps.main.duration + ps.main.startLifetime.constantMax;
            StartCoroutine(DeactivateParticleAfterDuration(particleInstance, duration));
            if (enableDebugLogs)
            {
                Debug.Log($"[SkillFsm] {particleInstance.name} 파티클 이펙트가 {duration}초 후에 비활성화됩니다.");
            }
        }
    }

    // 파티클 이펙트를 일정 시간 후 비활성화하는 코루틴
    private IEnumerator DeactivateParticleAfterDuration(GameObject particleEffect, float duration)
    {
        yield return new WaitForSeconds(duration);
        Destroy(particleEffect);
        if (enableDebugLogs)
        {
            Debug.Log($"[SkillFsm] {particleEffect.name} 파티클 이펙트가 비활성화되었습니다.");
        }
    }

    // 이동 속도 증가 메서드
    private void ApplySpeedBoost(float speedBoost, float duration)
    {
        if (isSpeedBoostActive || speedBoost <= 0f) return;
        StartCoroutine(SpeedBoostCoroutine(speedBoost, duration));
    }

    // Rush 스킬의 이동 속도 증가 코루틴
    private IEnumerator SpeedBoostCoroutine(float speedBoost, float duration)
    {
        isSpeedBoostActive = true;

        float originalSpeed = player.moveSpeed;
        player.moveSpeed *= speedBoost;
        if (enableDebugLogs)
        {
            Debug.Log($"[SkillFsm] Rush 이동 속도 상승 시작 ({speedBoost}배, {duration}초). " +
                      $"원래 속도: {originalSpeed}, 새로운 속도: {player.moveSpeed}");
        }

        yield return new WaitForSeconds(duration);

        player.moveSpeed = originalSpeed;
        isSpeedBoostActive = false;
        if (enableDebugLogs)
        {
            Debug.Log($"[SkillFsm] Rush 이동 속도 상승 종료. 속도 복원: {player.moveSpeed}");
        }
    }

    // 쿨다운 코루틴
    private IEnumerator CooldownCoroutine(Skill skill)
    {
        skill.IsOnCooldown = true;
        skill.RemainingCooldown = skill.CachedCooldown;

        // 쿨타임바 활성화 및 초기화
        if (skill.CooldownImage != null)
        {
            skill.CooldownImage.fillAmount = 1f;
            if (enableDebugLogs)
            {
                Debug.Log($"[CooldownCoroutine] {skill.Name} 스킬의 쿨타임바가 활성화되었습니다.");
            }
        }

        float elapsed = 0f;
        float totalCooldown = skill.CachedCooldown;
        float logInterval = 1f; // 1초마다 로그 출력
        float nextLogTime = logInterval;

        if (enableDebugLogs)
        {
            Debug.Log($"[CooldownCoroutine] {skill.Name} 스킬 쿨타임 시작: {totalCooldown:F2}초");
        }

        while (elapsed < totalCooldown)
        {
            elapsed += Time.deltaTime;
            skill.RemainingCooldown = Mathf.Max(totalCooldown - elapsed, 0f);

            if (skill.CooldownImage != null)
            {
                skill.CooldownImage.fillAmount = 1f - (elapsed / totalCooldown);
            }

            if (enableDebugLogs && elapsed >= nextLogTime)
            {
                Debug.Log($"[CooldownCoroutine] {skill.Name} remainingCooldown: {skill.RemainingCooldown:F2} sec");
                nextLogTime += logInterval;
            }

            yield return null;
        }

        // 쿨타임 종료
        skill.IsOnCooldown = false;
        skill.RemainingCooldown = 0f;
        if (enableDebugLogs)
        {
            Debug.Log($"[SkillFsm] {skill.Name} 스킬 쿨다운 종료");
        }

        // 쿨타임바 비활성화
        if (skill.CooldownImage != null)
        {
            skill.CooldownImage.fillAmount = 0f;
            if (enableDebugLogs)
            {
                Debug.Log($"[CooldownCoroutine] {skill.Name} 스킬의 쿨타임바가 비활성화되었습니다.");
            }
        }
    }

    // Animator 설정 메서드
    public void SetAnimator(Animator newAnimator)
    {
        if (newAnimator == null)
        {
            if (enableDebugLogs)
            {
                Debug.LogError("Animator가 null입니다. 설정할 수 없습니다.");
            }
            return;
        }

        animator = newAnimator;
        if (enableDebugLogs)
        {
            Debug.Log("[SkillFsm] 새로운 Animator가 설정되었습니다.");
        }
    }

    // 특정 SkillType에 해당하는 Skill 객체 반환
    public Skill GetSkill(SkillType skillType)
    {
        Skill skill = Skills.Find(s => s.skillType == skillType);
        if (skill != null && enableDebugLogs)
        {
            Debug.Log($"[SkillFsm] {skillType} 스킬이 검색되었습니다.");
        }
        else if (enableDebugLogs)
        {
            Debug.LogWarning($"[SkillFsm] {skillType} 스킬이 존재하지 않습니다.");
        }
        return skill;
    }

    // 스킬 레벨업 메서드
    public bool LevelUpSkill(SkillType skillType)
    {
        Skill skill = GetSkill(skillType);
        if (skill == null)
        {
            if (enableDebugLogs)
            {
                Debug.LogWarning($"[SkillFsm] {skillType} 스킬이 등록되어 있지 않습니다.");
            }
            return false;
        }

        bool leveledUp = skill.LevelUp();
        if (leveledUp)
        {
            // 레벨업에 따른 스킬 속성 조정
            AdjustSkillAttributes(skill);
            if (enableDebugLogs)
            {
                Debug.Log($"[SkillFsm] {skill.Name} 스킬의 속성이 레벨업에 따라 조정되었습니다.");
            }
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
                // 예: 레벨업 시 speedBoost 10% 증가
                skill.SpeedBoost += 0.1f;
                if (enableDebugLogs)
                {
                    Debug.Log($"[SkillFsm] {skill.Name} 스킬의 SpeedBoost가 {skill.SpeedBoost:F2}로 증가되었습니다.");
                }
                break;
            case SkillType.Parry:
                // 예: 레벨업 시 쿨타임 감소 비율 추가
                skill.CooldownReductionPerLevel += 2;
                skill.UpdateCooldown(); // 쿨타임 재계산
                if (enableDebugLogs)
                {
                    Debug.Log($"[SkillFsm] {skill.Name} 스킬의 CooldownReductionPerLevel이 {skill.CooldownReductionPerLevel}%로 증가되었습니다.");
                }
                break;
            case SkillType.Skill1:
                // Skill1 스킬의 레벨업 로직 추가 (예: 데미지 증가)
                // 여기에 구체적인 로직을 추가하세요
                if (enableDebugLogs)
                {
                    Debug.Log($"[SkillFsm] {skill.Name} 스킬의 레벨업에 따른 추가 로직이 구현되었습니다.");
                }
                break;
            case SkillType.Skill2:
                // Skill2 스킬의 레벨업 로직 추가
                // 여기에 구체적인 로직을 추가하세요
                if (enableDebugLogs)
                {
                    Debug.Log($"[SkillFsm] {skill.Name} 스킬의 레벨업에 따른 추가 로직이 구현되었습니다.");
                }
                break;
            default:
                if (enableDebugLogs)
                {
                    Debug.LogWarning($"[SkillFsm] {skill.Name} 스킬의 레벨업에 대한 로직이 정의되지 않았습니다.");
                }
                break;
        }

        // 추가적인 속성 조정이 필요할 경우 여기에 구현
    }
}
