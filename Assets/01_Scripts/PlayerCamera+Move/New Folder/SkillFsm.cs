using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;

public enum SkillType
{
    Rush,
    Parry,
    Skill1,
    Skill2
}

// 스킬 이펙트
[System.Serializable]
public class SkillEffect
{
    [Header("레벨에 따른 파티클 이펙트")] public GameObject ParticleEffect;

    [Header("레벨에 따른 파티클 스폰 포인트")] public Transform ParticleSpawnPoint;
}

// 스킬을 정의
[System.Serializable]
public class Skill
{
    [Header("스킬 유형")] public SkillType skillType;

    [Header("초기 레벨")] public int level = 1;

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
                if (skillFsm != null && skillFsm.enableDebugLogs)
                {
                    LogWarning($"[Skill] {Name} 스킬은 이미 최대 레벨({MaxLevel})에 도달했습니다.");
                }
            }
        }
    }

    [Header("최대 스킬 레벨")]
    public int MaxLevel = 4;

    [Header("스킬 레벨별 업그레이드 비용")]
    public List<float> UpgradeCosts;

    [Header("기본 쿨타임 (초 단위)")]
    public float BaseCooldown = 2f;

    [Header("레벨당 쿨타임 감소 비율 (백분율)")]
    public int CooldownReductionPerLevel = 5;

    [Header("Rush 스킬만 해당 (이동 속도 배수 처리)")]
    public float SpeedBoost = 0f;

    [Header("Rush 스킬만 해당 (지속 시간)")]
    public float Duration = 0.5f;

    [Header("스킬 설명")]
    [TextArea] public string Description;

    [Header("스킬 이미지")]
    public Sprite Icon;

    [Header("레벨에 따른 이펙트 및 스폰 포인트 리스트")]
    public List<SkillEffect> SkillEffects;

    // 쿨타임 작동 여부 체크를 위한 변수
    [HideInInspector] public bool IsOnCooldown = false;

    // 현재 쿨타임 남은 시간
    [HideInInspector] public float RemainingCooldown = 0f;

    // 스킬 이름 반환 -> 내부적으로 처리
    public string Name => skillType.ToString();

    // 애니메이터 트리거 이름 반환
    public string TriggerName
    {
        get { return skillType.ToString(); }
    }

    // 쿨타임을 미리 계산하여 저장
    [HideInInspector] public float CachedCooldown;

    // SkillFsm 참조
    [System.NonSerialized] public SkillFsm skillFsm;

    // SkillFsm 설정 
    public void SetSkillFsm(SkillFsm fsm)
    {
        skillFsm = fsm;
    }

    // 반복적으로 사용되는 로그 출력을 간소화
    public void Log(string message)
    {
        if (skillFsm != null)
            skillFsm.Log(message);
    }

    public void LogWarning(string message)
    {
        if (skillFsm != null)
            skillFsm.LogWarning(message);
    }

    public void LogError(string message)
    {
        if (skillFsm != null)
            skillFsm.LogError(message);
    }

    // 현재 레벨에 따른 파티클 이펙트 반환
    public SkillEffect GetCurrentSkillEffect()
    {
        if (SkillEffects == null || SkillEffects.Count == 0)
            return null;

        // 레벨 인덱스가 리스트 범위를 넘지 않도록 조정
        int index = Mathf.Clamp(Level - 1, 0, SkillEffects.Count - 1);
        return SkillEffects[index];
    }

    // 스킬 레벨업
    public bool LevelUp()
    {
        if (Level >= MaxLevel)
        {
            LogWarning($"[Skill] {Name} 스킬은 이미 최대 레벨({MaxLevel})에 도달했습니다.");
            return false;
        }

        Level++;
        Log($"[Skill] {Name} 스킬이 레벨 {Level}로 상승했습니다.");

        // 쿨타임 업데이트
        UpdateCooldown();

        return true;
    }

    // 현재 레벨에 따른 쿨타임 계산
    public void UpdateCooldown()
    {
        float reduction = CooldownReductionPerLevel / 100f;
        CachedCooldown = BaseCooldown * Mathf.Pow(1 - reduction, Level - 1);
        Log(
            $"[Skill] {Name} 레벨: {Level}, 기존 쿨타임: {BaseCooldown:F2}s, 쿨타임 감소율: {CooldownReductionPerLevel}%, 조정 쿨타임: {CachedCooldown:F2}s");
    }

    // 유효성 검사
    public void OnValidate()
    {
        if (UpgradeCosts != null && UpgradeCosts.Count < MaxLevel)
        {
            Debug.LogWarning(
                $"[Skill] {Name}의 UpgradeCosts 리스트가 MaxLevel보다 작습니다. 자동으로 초기화합니다.");
            UpgradeCosts = new List<float>(new float[MaxLevel - 1]);
        }
    }
}

// ========================================================================================================================================

[RequireComponent(typeof(Animator), typeof(Player), typeof(PlayerFsm))]
public class SkillFsm : MonoBehaviour
{
    private Animator animator;

    // 스킬 설정
    public List<Skill> Skills;

    private bool isSpeedBoostActive = false;

    [Header("디버그 설정 (출력 유/무)")]
    public bool enableDebugLogs = true;

    // Player 및 PlayerFsm 컴포넌트 참조
    private Player player;
    private PlayerFsm playerFsm;

    // Rush 스킬 활성화 상태를 나타내는 프로퍼티
    public bool IsRushActive { get; private set; } = false;

    // 스킬 트리거 요청 이벤트
    public event Action<SkillType> OnSkillTriggerRequested;

    private void Awake()
    {
        InitializeComponents();
        InitializeSkillFsm();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        // 입력 이벤트 등록 (예시)
        PlayerInputManager.OnSkillInput += TriggerRushSkill;
        PlayerInputManager.OnRightClickInput += TriggerParrySkill;
        PlayerInputManager.OnShiftLeftClickInput += TriggerSkill1;
        PlayerInputManager.OnShiftRightClickInput += TriggerSkill2;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // 입력 이벤트 해제 (예시)
        PlayerInputManager.OnSkillInput -= TriggerRushSkill;
        PlayerInputManager.OnRightClickInput -= TriggerParrySkill;
        PlayerInputManager.OnShiftLeftClickInput -= TriggerSkill1;
        PlayerInputManager.OnShiftRightClickInput -= TriggerSkill2;
    }

    /// <summary>
    /// 컴포넌트 초기화 메서드
    /// </summary>
    private void InitializeComponents()
    {
        // Animator 컴포넌트 가져오기
        animator = GetComponent<Animator>();
        if (animator != null)
        {
            Log("[SkillFsm] Animator 컴포넌트를 성공적으로 가져왔습니다.");
        }
        else
        {
            LogError("[SkillFsm] Animator 컴포넌트를 찾을 수 없습니다. SkillFsm 기능이 제한될 수 있습니다.");
        }

        // Player 컴포넌트 가져오기
        player = GetComponent<Player>();
        if (player != null)
        {
            Log("[SkillFsm] Player 컴포넌트를 성공적으로 가져왔습니다.");
        }
        else
        {
            LogError("[SkillFsm] Player 컴포넌트를 찾을 수 없습니다. SkillFsm 기능이 제한될 수 있습니다.");
        }

        // PlayerFsm 컴포넌트 가져오기
        playerFsm = GetComponent<PlayerFsm>();
        if (playerFsm != null)
        {
            Log("[SkillFsm] PlayerFsm 컴포넌트를 성공적으로 가져왔습니다.");
        }
        else
        {
            LogError("[SkillFsm] PlayerFsm 컴포넌트를 찾을 수 없습니다. SkillFsm 기능이 제한될 수 있습니다.");
        }
    }

    /// <summary>
    /// SkillFsm 초기화 메서드
    /// </summary>
    private void InitializeSkillFsm()
    {
        // SkillFsm 참조 설정
        foreach (var skill in Skills)
        {
            skill.SetSkillFsm(this);
        }

        // FirebaseManager가 초기화되었는지 확인
        if (FirebaseManager.Instance != null && FirebaseManager.Instance.CurrentUserData != null)
        {
            for (int i = 0; i < Skills.Count; i++)
            {
                if (i < FirebaseManager.Instance.CurrentUserData.user_Skills.Count)
                {
                    Skills[i].level = FirebaseManager.Instance.CurrentUserData.user_Skills[i].skill_Level;
                }
                else
                {
                    LogWarning($"[SkillFsm] user_Skills 리스트에 인덱스 {i}가 존재하지 않습니다.");
                }
            }
        }
        else
        {
            LogError("[SkillFsm] FirebaseManager 또는 CurrentUserData가 초기화되지 않았습니다.");
        }

        InitializeSkillEffects();
        InitializeCooldowns();
    }

    /// <summary>
    /// 씬 로딩 시 호출되는 메서드
    /// </summary>
    /// <param name="scene">로드된 씬</param>
    /// <param name="mode">로드 모드</param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Log($"[SkillFsm] 씬 '{scene.name}'이(가) 로드되었습니다. 로드 모드: {mode}");

        // 컴포넌트 재초기화
        InitializeComponents();
        InitializeSkillFsm();
    }

    /// <summary>
    /// 스킬 이펙트 초기화
    /// </summary>
    private void InitializeSkillEffects()
    {
        foreach (var skill in Skills)
        {
            if (skill.SkillEffects == null || skill.SkillEffects.Count == 0)
            {
                List<SkillEffect> foundSkillEffects = new List<SkillEffect>();
                for (int lvl = 1; lvl <= skill.MaxLevel; lvl++)
                {
                    string spawnPointName = $"{skill.skillType}Level{lvl}ParticleSpawnPoint";
                    Transform spawnPoint = player != null ? player.transform.Find(spawnPointName) : null;
                    if (spawnPoint != null)
                    {
                        // 스폰 포인트가 발견되면 기본 이펙트 프리팹을 지정
                        SkillEffect skillEffect = new SkillEffect
                        {
                            ParticleEffect = (skill.SkillEffects != null &&
                                              lvl - 1 < skill.SkillEffects.Count)
                                ? skill.SkillEffects[lvl - 1].ParticleEffect
                                : null,
                            ParticleSpawnPoint = spawnPoint
                        };
                        foundSkillEffects.Add(skillEffect);
                        Log(
                            $"[SkillFsm] {skill.Name} 스킬 레벨 {lvl}에 대한 ParticleSpawnPoint '{spawnPointName}'가 설정되었습니다.");
                    }
                    else
                    {
                        LogWarning(
                            $"[SkillFsm] {skill.Name} 스킬 레벨 {lvl}에 대한 ParticleSpawnPoint '{spawnPointName}'를 찾을 수 없습니다.");
                        // 스폰 포인트가 없을 경우 null 추가
                        foundSkillEffects.Add(null);
                    }
                }

                skill.SkillEffects = foundSkillEffects;
            }

            // SkillFsm 참조 설정(중복)
            skill.SetSkillFsm(this);
        }
    }

    /// <summary>
    /// 쿨타임 초기화
    /// </summary>
    private void InitializeCooldowns()
    {
        foreach (var skill in Skills)
        {
            skill.UpdateCooldown();
        }
    }

    // 입력 이벤트 등록 (예시)
    private void TriggerRushSkill()
    {
        ExternalTriggerSkill(SkillType.Rush);
    }

    private void TriggerParrySkill()
    {
        ExternalTriggerSkill(SkillType.Parry);
    }

    private void TriggerSkill1()
    {
        ExternalTriggerSkill(SkillType.Skill1);
    }

    private void TriggerSkill2()
    {
        ExternalTriggerSkill(SkillType.Skill2);
    }

    // 스킬 사용을 외부에서 요청할 수 있도록 하는 메서드
    public void ExternalTriggerSkill(SkillType skillType)
    {
        OnSkillTriggerRequested?.Invoke(skillType);
    }

    // FSMManager에서 호출하는 실제 스킬 트리거 메서드
    public void TriggerSkill(SkillType skillType)
    {
        if (animator == null)
        {
            LogWarning("[SkillFsm] Animator가 null이므로 스킬을 사용할 수 없습니다.");
            return;
        }

        Skill skill = GetSkill(skillType);
        if (skill == null)
        {
            LogWarning($"[SkillFsm] {skillType} 스킬이 등록되어 있지 않습니다.");
            return;
        }

        if (skill.IsOnCooldown)
        {
            Log(
                $"[SkillFsm] {skill.Name} 스킬은 쿨다운 중입니다. 남은 시간: {skill.RemainingCooldown:F2}초");
            return;
        }

        // 스킬 실행
        animator.SetTrigger(skill.TriggerName);
        Log($"[SkillFsm] {skill.Name} 스킬이 트리거되었습니다.");

        // 스킬 유형별 사운드 재생 (예시)
        switch (skillType)
        {
            case SkillType.Rush:
                SoundManager.Instance?.PlaySFX("Dash", gameObject);
                break;
            case SkillType.Parry:
            case SkillType.Skill1:
                SoundManager.Instance?.PlaySFX("rush_skill_sound", gameObject);
                break;
            case SkillType.Skill2:
                StartCoroutine(PlayDelayedSound("sound_player_Twohandskill4", 0.6f));
                break;
            default:
                break;
        }

        // 레벨에 따른 파티클 이펙트 활성화
        SkillEffect currentSkillEffect = skill.GetCurrentSkillEffect();
        if (currentSkillEffect != null && currentSkillEffect.ParticleEffect != null)
        {
            ActivateSkillParticle(currentSkillEffect, skill);
            Log($"[SkillFsm] {skill.Name} 스킬의 파티클 이펙트가 활성화되었습니다.");
        }
        else
        {
            LogWarning($"[SkillFsm] {skill.Name} 스킬의 파티클 이펙트가 설정되지 않았습니다.");
        }

        // Rush 스킬이면 이동 속도 증가 및 Rush 활성화 상태 설정
        if (skillType == SkillType.Rush && player != null)
        {
            IsRushActive = true; // Rush 활성화
            Log("Rush 스킬이 활성화되었습니다.");

            // 플레이어의 무적 상태를 스킬의 지속 시간과 동일하게 설정
            player.SetInvincible(true, skill.Duration);

            // 이동 속도 증가 적용
            ApplySpeedBoost(skill.SpeedBoost, skill.Duration);

            // Rush 스킬의 지속 시간이 끝난 후 Rush 활성화 상태 해제
            StartCoroutine(RushCooldownCoroutine(skill.Duration));
        }

        // 쿨다운 시작
        StartCoroutine(CooldownCoroutine(skill));
    }

    // 지연된 사운드 재생 메서드
    private IEnumerator PlayDelayedSound(string soundName, float delay)
    {
        yield return new WaitForSeconds(delay);
        SoundManager.Instance?.PlaySFX(soundName, gameObject);
    }

    // Rush 스킬의 지속 시간 동안 Rush 활성화 상태를 유지하는 코루틴
    private IEnumerator RushCooldownCoroutine(float duration)
    {
        Log($"Rush 스킬 지속 시간 시작: {duration}초");
        yield return new WaitForSeconds(duration);
        IsRushActive = false;
        Log("Rush 스킬의 지속 시간이 끝나서 Rush 활성화 상태가 해제되었습니다.");
    }

    // 파티클 이펙트를 활성화
    private void ActivateSkillParticle(SkillEffect skillEffect, Skill skill)
    {
        if (skillEffect.ParticleEffect == null)
        {
            LogWarning($"[SkillFsm] {skill.Name} 스킬의 파티클 이펙트 프리팹이 null입니다.");
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
            // 기본 위치 사용
            spawnPosition = player != null ? player.transform.position + player.transform.forward : Vector3.zero;
            spawnRotation = Quaternion.identity;
            LogWarning($"[SkillFsm] {skill.Name} 스킬의 파티클이 기본 위치에서 생성됩니다.");
        }

        GameObject particleInstance = Instantiate(skillEffect.ParticleEffect,
            spawnPosition, spawnRotation, transform);
        particleInstance.SetActive(true);

        ParticleSystem ps = particleInstance.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            float duration = ps.main.duration + ps.main.startLifetime.constantMax;
            StartCoroutine(
                DeactivateParticleAfterDuration(particleInstance, duration));
            Log(
                $"[SkillFsm] {particleInstance.name} 파티클 이펙트가 {duration}초 후에 비활성화됩니다.");
        }
        else
        {
            // ParticleSystem이 없으면 즉시 비활성화
            Destroy(particleInstance);
            LogWarning(
                $"[SkillFsm] {particleInstance.name}에 ParticleSystem이 없습니다. 즉시 비활성화됩니다.");
        }
    }

    // 파티클 이펙트를 일정 시간 후 비활성화하는 코루틴
    private IEnumerator DeactivateParticleAfterDuration(GameObject particleEffect,
        float duration)
    {
        yield return new WaitForSeconds(duration);
        Destroy(particleEffect);
        Log($"[SkillFsm] {particleEffect.name} 파티클 이펙트가 비활성화되었습니다.");
    }

    // 이동 속도를 증가
    private void ApplySpeedBoost(float speedBoost, float duration)
    {
        if (isSpeedBoostActive || speedBoost <= 0f)
            return;

        StartCoroutine(SpeedBoostCoroutine(speedBoost, duration));
    }

    // Rush 스킬의 이동 속도 증가 코루틴
    private IEnumerator SpeedBoostCoroutine(float speedBoost, float duration)
    {
        isSpeedBoostActive = true;

        float originalSpeed = player != null ? player.moveSpeed : 0f;
        if (player != null)
        {
            player.moveSpeed *= speedBoost;
            Log(
                $"[SkillFsm] Rush 이동 속도 상승 시작 ({speedBoost}배, {duration}초). 원래 속도: {originalSpeed}, 새로운 속도: {player.moveSpeed}");
        }
        else
        {
            LogWarning("[SkillFsm] Player 참조가 없으므로 이동 속도 증가를 적용할 수 없습니다.");
        }

        yield return new WaitForSeconds(duration);

        if (player != null)
        {
            player.moveSpeed = originalSpeed;
            Log($"[SkillFsm] Rush 이동 속도 상승 종료. 속도 복원: {player.moveSpeed}");
        }
        else
        {
            LogWarning("[SkillFsm] Player 참조가 없으므로 이동 속도 복원을 할 수 없습니다.");
        }

        isSpeedBoostActive = false;
    }

    // 쿨타임을 처리하는 코루틴
    private IEnumerator CooldownCoroutine(Skill skill)
    {
        skill.IsOnCooldown = true;
        skill.RemainingCooldown = skill.CachedCooldown;

        float elapsed = 0f;
        float totalCooldown = skill.CachedCooldown;
        float logInterval = 1f;
        float nextLogTime = logInterval;

        Log($"[CooldownCoroutine] {skill.Name} 스킬 쿨타임 시작: {totalCooldown:F2}초");

        // 남은 시간 디버그 출력
        while (elapsed < totalCooldown)
        {
            elapsed += Time.deltaTime;
            skill.RemainingCooldown = Mathf.Max(totalCooldown - elapsed, 0f);

            if (enableDebugLogs && elapsed >= nextLogTime)
            {
                Log(
                    $"[CooldownCoroutine] {skill.Name} remainingCooldown: {skill.RemainingCooldown:F2} sec");
                nextLogTime += logInterval;
            }

            yield return null;
        }

        // 쿨타임 종료
        skill.IsOnCooldown = false;
        skill.RemainingCooldown = 0f;
        Log($"[SkillFsm] {skill.Name} 스킬 쿨다운 종료");
    }

    // 코드 내에서 반복적으로 사용되는 로그 출력
    public void Log(string message)
    {
        if (enableDebugLogs)
            Debug.Log(message);
    }

    public void LogWarning(string message)
    {
        if (enableDebugLogs)
            Debug.LogWarning(message);
    }

    public void LogError(string message)
    {
        if (enableDebugLogs)
            Debug.LogError(message);
    }

    // 스킬 가져오기
    public Skill GetSkill(SkillType skillType)
    {
        Skill skill = Skills.Find(s => s.skillType == skillType);

        if (skill != null)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[SkillFsm] {skillType} 스킬이 검색되었습니다.");
            }
        }
        else
        {
            if (enableDebugLogs)
            {
                Debug.LogWarning($"[SkillFsm] {skillType} 스킬이 존재하지 않습니다.");
            }
        }

        return skill;
    }

    // 스킬 레벨업
    public bool LevelUpSkill(SkillType skillType)
    {
        Skill skill = GetSkill(skillType);
        if (skill == null)
        {
            LogWarning($"[SkillFsm] {skillType} 스킬이 등록되어 있지 않습니다.");
            return false;
        }

        bool leveledUp = skill.LevelUp();

        if (FirebaseManager.Instance != null && FirebaseManager.Instance.CurrentUserData != null)
        {
            if ((int)skillType < FirebaseManager.Instance.CurrentUserData.user_Skills.Count)
            {
                FirebaseManager.Instance.CurrentUserData.user_Skills[(int)skillType] =
                    new SkillData((int)skillType, skill.Level);
            }
            else
            {
                LogWarning($"[SkillFsm] user_Skills 리스트에 인덱스 {(int)skillType}가 존재하지 않습니다.");
            }
        }
        else
        {
            LogError("[SkillFsm] FirebaseManager 또는 CurrentUserData가 초기화되지 않았습니다.");
        }

        if (leveledUp)
        {
            // 레벨업에 따른 스킬 속성 조정
            AdjustSkillAttributes(skill);
            Log($"[SkillFsm] {skill.Name} 스킬의 속성이 레벨업에 따라 조정되었습니다.");
            return true;
        }

        return false;
    }

    // 스킬 레벨업에 따른 속성 조정
    private void AdjustSkillAttributes(Skill skill)
    {
        switch (skill.skillType)
        {
            case SkillType.Rush:
                // Rush 스킬의 레벨업 로직 필요 시 추가
                break;
            case SkillType.Parry:
            case SkillType.Skill1:
            case SkillType.Skill2:
                Log($"[SkillFsm] {skill.Name} 스킬의 레벨업에 따른 추가 로직이 구현되었습니다.");
                break;
            default:
                LogWarning($"[SkillFsm] {skill.Name} 스킬의 레벨업에 대한 로직이 정의되지 않았습니다.");
                break;
        }

        // 추가적인 속성 조정이 필요할 경우 여기서 구현
    }
}
