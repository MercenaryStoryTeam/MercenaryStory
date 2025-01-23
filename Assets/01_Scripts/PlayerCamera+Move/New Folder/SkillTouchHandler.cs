using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// SkillFsm 스크립트에서 선언된 SkillType 명칭과 겹침 문제 해결
namespace GameNamespace
{
    // 스킬 타입 열거형
    public enum SkillType
    {
        None,
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
        public GameNamespace.SkillType skillType = GameNamespace.SkillType.None;
    }

    // 스킬 버튼 리스트
    [Header("스킬 버튼 리스트")]
    public List<SkillButton> skillButtons = new List<SkillButton>();

    // 버튼과 스킬 타입 매핑
    private Dictionary<GameObject, GameNamespace.SkillType> buttonSkillMap;

    private void Awake()
    {
        // 버튼과 스킬 타입 매핑 초기화
        buttonSkillMap = new Dictionary<GameObject, GameNamespace.SkillType>();
        foreach (var skillButton in skillButtons)
        {
            if (skillButton.button != null)
            {
                buttonSkillMap[skillButton.button] = skillButton.skillType;

                // 각 버튼에 EventTrigger 추가 -> 트리거 처리 핵심
                AddEventTrigger(skillButton.button);
            }
        }
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

        // Drag 이벤트 추가 제거
        // 드래그 기능이 필요 없으므로 관련 코드를 제거합니다.
    }

    // 버튼을 터치했을 때 호출 (터치 시작)
    public void OnPointerDown(PointerEventData eventData, GameObject button)
    {
        if (!buttonSkillMap.ContainsKey(button)) return;

        GameNamespace.SkillType skillType = buttonSkillMap[button];
        Debug.Log($"버튼 터치 시작: {skillType}");
        TriggerSkill(skillType);
    }

    // 버튼에서 손을 뗐을 때 호출 (터치 종료)
    public void OnPointerUp(PointerEventData eventData, GameObject button)
    {
        if (!buttonSkillMap.ContainsKey(button)) return;

        GameNamespace.SkillType skillType = buttonSkillMap[button];
        Debug.Log($"버튼 터치 종료: {skillType}");
    }

    // 스킬 트리거 호출 
    private void TriggerSkill(GameNamespace.SkillType skillType)
    {
        switch (skillType)
        {
            case GameNamespace.SkillType.Rush:
                PlayerInputManager.OnSkillInput?.Invoke();
                break;
            case GameNamespace.SkillType.Parry:
                PlayerInputManager.OnRightClickInput?.Invoke();
                break;
            case GameNamespace.SkillType.Skill1:
                PlayerInputManager.OnShiftLeftClickInput?.Invoke();
                break;
            case GameNamespace.SkillType.Skill2:
                PlayerInputManager.OnShiftRightClickInput?.Invoke();
                break;
            case GameNamespace.SkillType.None:
                Debug.LogWarning("스킬 타입이 None으로 설정되어 있습니다.");
                break;
            default:
                Debug.LogWarning("알 수 없는 SkillType이 할당되었습니다.");
                break;
        }
    }
}
