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

[System.Serializable]
public class SkillEffect
{
    [Header("레벨에 따른 파티클 이펙트")]
    public GameObject ParticleEffect; // 파티클 이펙트 프리팹

    [Header("레벨에 따른 파티클 스폰 포인트")]
    public Transform ParticleSpawnPoint; // 파티클 스폰 포인트
}

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
    public Sprite Icon; 

    [Header("레벨에 따른 이펙트 및 스폰 포인트 리스트")]
    public List<SkillEffect> SkillEffects; 

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
    public SkillEffect GetCurrentSkillEffect()
    {
        if (SkillEffects == null || SkillEffects.Count == 0)
            return null;

        // 레벨 인덱스가 리스트 범위를 넘지 않도록 조정
        int index = Mathf.Clamp(Level - 1, 0, SkillEffects.Count - 1);
        return SkillEffects[index];
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

    // 현재 레벨에 따른 쿨타임 계산 
    public void UpdateCooldown()
    {
        float reduction = CooldownReductionPerLevel / 100f;
        CachedCooldown = BaseCooldown * Mathf.Pow(1 - reduction, Level - 1);
        if (SkillFsm.Instance != null && SkillFsm.Instance.enableDebugLogs)
        {
            Debug.Log($"[Skill] {Name} Level: {Level}, Base Cooldown: {BaseCooldown:F2}s, " +
                      $"Reduction per Level: {CooldownReductionPerLevel}%, " +
                      $"Calculated Cooldown: {CachedCooldown:F2}s");
        }
    }
}

[RequireComponent(typeof(Animator))]
public class SkillFsm : MonoBehaviour
{
    private Animator animator;

    [Header("스킬 설정")]
    public List<Skill> Skills;

    private bool isSpeedBoostActive = false;

    [Header("참조 설정")]
    [SerializeField] private Player player;

    [Header("디버그 설정 (출력 유,무)")]
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

        // 각 스킬의 SkillEffects 설정 확인
        foreach (var skill in Skills)
        {
            if (skill.SkillEffects == null || skill.SkillEffects.Count == 0)
            {
                // 플레이어의 자식 중 스킬 이름과 레벨을 포함하는 파티클 스폰 포인트를 찾음
                // 예: "RushLevel1ParticleSpawnPoint", "RushLevel2ParticleSpawnPoint", etc.
                List<SkillEffect> foundSkillEffects = new List<SkillEffect>();
                for (int lvl = 1; lvl <= skill.MaxLevel; lvl++)
                {
                    string spawnPointName = $"{skill.skillType}Level{lvl}ParticleSpawnPoint";
                    Transform spawnPoint = player.transform.Find(spawnPointName);
                    if (spawnPoint != null)
                    {
                        // 스폰 포인트가 발견되면 기본 이펙트 프리팹을 지정 (필요 시 수정)
                        SkillEffect skillEffect = new SkillEffect
                        {
                            ParticleEffect = skill.SkillEffects != null && lvl - 1 < skill.SkillEffects.Count ? skill.SkillEffects[lvl - 1].ParticleEffect : null,
                            ParticleSpawnPoint = spawnPoint
                        };
                        foundSkillEffects.Add(skillEffect);
                        if (enableDebugLogs)
                        {
                            Debug.Log($"[SkillFsm] {skill.Name} 스킬 레벨 {lvl}에 대한 ParticleSpawnPoint '{spawnPointName}'가 설정되었습니다.");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[SkillFsm] {skill.Name} 스킬 레벨 {lvl}에 대한 ParticleSpawnPoint '{spawnPointName}'를 찾을 수 없습니다.");
                        foundSkillEffects.Add(null); // 스폰 포인트가 없을 경우 null 추가
                        foundSkillEffects.Add(null); 
                    }
                }

                skill.SkillEffects = foundSkillEffects;
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
        // 입력 이벤트 구독 (명확한 메서드 사용)
        PlayerInputManager.OnSkillInput += TriggerRushSkill;
        PlayerInputManager.OnRightClickInput += TriggerParrySkill;
        PlayerInputManager.OnShiftLeftClickInput += TriggerSkill1;
        PlayerInputManager.OnShiftRightClickInput += TriggerSkill2;
    }

    private void OnDisable()
    {
        // 입력 이벤트 해제
        PlayerInputManager.OnSkillInput -= TriggerRushSkill;
        PlayerInputManager.OnRightClickInput -= TriggerParrySkill;
        PlayerInputManager.OnShiftLeftClickInput -= TriggerSkill1;
        PlayerInputManager.OnShiftRightClickInput -= TriggerSkill2;
    }

    // 각 스킬에 대한 트리거 메서드
    private void TriggerRushSkill()
    {
        TriggerSkill(SkillType.Rush);
    }

    private void TriggerParrySkill()
    {
        TriggerSkill(SkillType.Parry);
    }

    private void TriggerSkill1()
    {
        TriggerSkill(SkillType.Skill1);
    }

    private void TriggerSkill2()
    {
        TriggerSkill(SkillType.Skill2);
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
        SkillEffect currentSkillEffect = skill.GetCurrentSkillEffect();
        if (currentSkillEffect != null && currentSkillEffect.ParticleEffect != null)
        {
            ActivateSkillParticle(currentSkillEffect, skill);
            if (enableDebugLogs)
            {
                Debug.Log($"[SkillFsm] {skill.Name} 스킬의 파티클 이펙트가 활성화되었습니다.");
            }
        }
        else
        {
            if (enableDebugLogs)
            {
                Debug.LogWarning($"[SkillFsm] {skill.Name} 스킬의 파티클 이펙트가 설정되지 않았습니다.");
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
    private void ActivateSkillParticle(SkillEffect skillEffect, Skill skill)
    {
        if (skillEffect.ParticleEffect == null)
        {
            if (enableDebugLogs)
            {
                Debug.LogWarning($"[SkillFsm] {skill.Name} 스킬의 파티클 이펙트 프리팹이 null입니다.");
            }
            return;
        }

        Vector3 spawnPosition;
        Quaternion spawnRotation;

        if (skillEffect.ParticleSpawnPoint != null)
        {
            spawnPosition = skillEffect.ParticleSpawnPoint.position;
            spawnRotation = skillEffect.ParticleSpawnPoint.rotation;
        }
        else
        {
            // 스폰 포인트가 설정되지 않았으면 기본 위치 사용
            spawnPosition = player.transform.position + player.transform.forward;
            spawnRotation = Quaternion.identity;
            if (enableDebugLogs)
            {
                Debug.LogWarning($"[SkillFsm] {skill.Name} 스킬의 ParticleSpawnPoint가 설정되지 않았습니다. 기본 위치에서 파티클이 생성됩니다.");
            }
        }

        GameObject particleInstance = Instantiate(skillEffect.ParticleEffect, spawnPosition, spawnRotation, transform);
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
        else
        {
            // 파티클 시스템이 없으면 즉시 비활성화
            Destroy(particleInstance);
            if (enableDebugLogs)
            {
                Debug.LogWarning($"[SkillFsm] {particleInstance.name}에 ParticleSystem이 없습니다. 즉시 비활성화됩니다.");
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
        float logInterval = 1f;
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
