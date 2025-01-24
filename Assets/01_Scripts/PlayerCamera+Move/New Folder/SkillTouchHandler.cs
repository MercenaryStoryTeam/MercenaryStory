using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SkillTouchHandlerNamespace
{
    // 스킬 타입 열거형
    public enum SkillType
    {
        Rush,
        Parry,
        Skill1,
        Skill2
    }
}

// 모바일에서 터치 입력으로 스킬 처리
public class SkillTouchHandler : MonoBehaviour
{
    [System.Serializable]
    public class SkillButton
    {
        // 버튼 오브젝트
        public GameObject button;

        // 해당 버튼과 연결된 스킬 타입
        public SkillType skillType;

        // 쿨타임 UI (이미지)
        public Image cooldownImage;
    }

    // 스킬 버튼 리스트
    [Header("스킬 버튼 리스트")]
    public List<SkillButton> skillButtons = new List<SkillButton>();

    // 버튼과 스킬 타입 매핑
    private Dictionary<GameObject, SkillType> buttonSkillMap;

    // SkillFsm 참조
    [Header("SkillFsm 참조")]
    public SkillFsm skillFsm;

    private void Awake()
    {
        // 버튼과 스킬 타입 매핑 초기화
        buttonSkillMap = new Dictionary<GameObject, SkillType>();
        foreach (var skillButton in skillButtons)
        {
            if (skillButton.button != null)
            {
                buttonSkillMap[skillButton.button] = skillButton.skillType;

                // 각 버튼에 EventTrigger 추가
                AddEventTrigger(skillButton.button);
            }
        }
    }

    private void Update()
    {
        // SkillFsm 참조 확인
        if (skillFsm == null)
        {
            skillFsm = GameObject.Find($"{FirebaseManager.Instance.CurrentUserData.user_Name}").GetComponent<SkillFsm>();
            if (skillFsm == null)
            {
                Debug.LogError("SkillFsm을 찾을 수 없습니다.");
                enabled = false;
                return;
            }
        }
        UpdateCooldownUI();
    }

    private void AddEventTrigger(GameObject button)
    {
        var eventTrigger = button.GetComponent<EventTrigger>();
        if (eventTrigger == null)
        {
            eventTrigger = button.AddComponent<EventTrigger>();
        }

        // Pointer Down 이벤트 추가
        var pointerDownEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerDown
        };
        pointerDownEntry.callback.AddListener((data) => { OnPointerDown((PointerEventData)data, button); });
        eventTrigger.triggers.Add(pointerDownEntry);

        // Pointer Up 이벤트 추가
        var pointerUpEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerUp
        };
        pointerUpEntry.callback.AddListener((data) => { OnPointerUp((PointerEventData)data, button); });
        eventTrigger.triggers.Add(pointerUpEntry);
    }

    // 버튼을 터치했을 때 호출 (터치 시작)
    public void OnPointerDown(PointerEventData eventData, GameObject button)
    {
        if (!buttonSkillMap.ContainsKey(button)) return;

        SkillType skillType = buttonSkillMap[button];

        // 스킬이 쿨타임 중인지 확인
        Skill skill = skillFsm.GetSkill(skillType);
        if (skill == null)
        {
            Debug.LogWarning($"스킬 {skillType}을 찾을 수 없습니다.");
            return;
        }

        if (skill.IsOnCooldown)
        {
            Debug.Log($"스킬 {skillType}이(가) 쿨타임 중입니다.");
            return;
        }

        Debug.Log($"버튼 터치 시작: {skillType}");
        TriggerSkill(skillType);
    }

    // 버튼에서 손을 뗐을 때 호출 (터치 종료)
    public void OnPointerUp(PointerEventData eventData, GameObject button)
    {
        if (!buttonSkillMap.ContainsKey(button)) return;

        SkillType skillType = buttonSkillMap[button];
        Debug.Log($"버튼 터치 종료: {skillType}");
    }

    // 스킬 트리거 호출 
    private void TriggerSkill(SkillType skillType)
    {
        switch (skillType)
        {
            case SkillType.Rush:
                PlayerInputManager.OnSkillInput?.Invoke();
                break;
            case SkillType.Parry:
                PlayerInputManager.OnRightClickInput?.Invoke();
                break;
            case SkillType.Skill1:
                PlayerInputManager.OnShiftLeftClickInput?.Invoke();
                break;
            case SkillType.Skill2:
                PlayerInputManager.OnShiftRightClickInput?.Invoke();
                break;
            default:
                Debug.LogWarning("알 수 없는 SkillType이 할당되었습니다.");
                break;
        }
    }

    // 쿨타임 UI 업데이트
    private void UpdateCooldownUI()
    {
        foreach (var skillButton in skillButtons)
        {
            if (skillButton.cooldownImage != null)
            {
                Skill skill = skillFsm.GetSkill(skillButton.skillType);
                if (skill != null)
                {
                    if (skill.IsOnCooldown)
                    {
                        float remaining = skill.RemainingCooldown / skill.CachedCooldown;
                        skillButton.cooldownImage.fillAmount = remaining;
                    }
                    else
                    {
                        skillButton.cooldownImage.fillAmount = 0f;
                    }
                }
            }
        }
    }
}
