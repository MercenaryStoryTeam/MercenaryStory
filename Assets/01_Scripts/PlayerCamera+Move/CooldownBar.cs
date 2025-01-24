using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CooldownBar : MonoBehaviour
{
    // 스킬과 관련된 이미지 묶음 클래스
    [System.Serializable]
    public class SkillCooldown
    {
        public SkillType skillType;   // SkillFsm에서 정의된 SkillType 사용
        public Image cooldownImage;   // 쿨타임 이미지
    }

    // 스킬-이미지 리스트
    [SerializeField]
    private List<SkillCooldown> skillCooldowns;

    [Header("SkillFsm 참조")]
    public SkillFsm skillFsm;

    // 참조 시도 간격 (초)
    [SerializeField]
    private float retryInterval = 1f;

    private void Start()
    {
        // 인스펙터에서 참조가 안 되어 있으면 자동으로 찾아보기
        if (skillFsm == null)
        {
            StartCoroutine(FindSkillFsm());
        }

        // 모든 쿨타임 이미지의 fillAmount를 0으로 초기화 (옵션)
        foreach (var skillCooldown in skillCooldowns)
        {
            if (skillCooldown.cooldownImage != null)
            {
                skillCooldown.cooldownImage.fillAmount = 0f;
            }
        }
    }

    private IEnumerator FindSkillFsm()
    {
        // "Player" 레이어의 레이어 번호를 가져옵니다
        int playerLayer = LayerMask.NameToLayer("Player");
        if (playerLayer == -1)
        {
            Debug.LogError("[CooldownBar] 'Player' 레이어가 존재하지 않습니다.");
            yield break;
        }

        while (skillFsm == null)
        {
            // 모든 활성화된 게임 오브젝트를 가져옵니다
            GameObject[] allObjects = FindObjectsOfType<GameObject>();

            foreach (GameObject obj in allObjects)
            {
                // 오브젝트가 플레이어 레이어에 속하고 이름에 "Clone"이 포함되지 않은 경우
                if (obj.layer == playerLayer && !obj.name.Contains("Clone"))
                {
                    SkillFsm foundFsm = obj.GetComponent<SkillFsm>();
                    if (foundFsm != null)
                    {
                        skillFsm = foundFsm;
                        Debug.Log($"[CooldownBar] SkillFsm을 '{obj.name}' 오브젝트에서 찾았습니다.");
                        break;
                    }
                }
            }

            if (skillFsm == null)
            {
                Debug.LogWarning("[CooldownBar] SkillFsm을 찾지 못했습니다. 다음 시도까지 대기합니다.");
                yield return new WaitForSeconds(retryInterval);
            }
            else
            {
                // SkillFsm을 찾았으므로 코루틴을 종료합니다
                yield break;
            }
        }
    }

    private void Update()
    {
        if (skillFsm == null) return;

        // 등록된 스킬 쿨타임 이미지를 순회하며 채우기
        foreach (var skillCooldown in skillCooldowns)
        {
            if (skillCooldown.cooldownImage == null) continue;

            // SkillFsm에서 해당 스킬을 가져온다
            var skill = skillFsm.GetSkill(skillCooldown.skillType);
            if (skill == null) continue;

            // 쿨타임 중이면 남은 시간에 따라 fillAmount 변경
            if (skill.IsOnCooldown && skill.CachedCooldown > 0)
            {
                // fillAmount가 1에서 0으로 감소하도록 설정
                float ratio = Mathf.Clamp01(skill.RemainingCooldown / skill.CachedCooldown);
                skillCooldown.cooldownImage.fillAmount = ratio;
            }
            else
            {
                // 쿨타임이 끝났거나 없는 경우 fillAmount를 0으로 설정
                skillCooldown.cooldownImage.fillAmount = 0f;
            }
        }
    }
}
