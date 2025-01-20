using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SkillUpgradeUI : MonoBehaviour
{
    [Header("Skill FSM 스크립트")]
    public SkillFsm skillFsm;

    [Header("스킬 선택 버튼들")]
    public List<Button> skillSelectionButtons; // 스킬 선택 버튼들을 리스트로 관리
    public List<SkillType> skillTypes; // 각 버튼에 해당하는 스킬 타입 리스트

    [Header("레벨 업 버튼")]
    public Button levelUpButton;

    [Header("스킬 이름 표시")]
    public Text skillNameText;

    [Header("스킬 설명 표시")]
    public Text skillDescriptionText;

    [Header("스킬 레벨 표시")]
    public Text selectedSkillLevelText;

    [Header("스킬 이미지 표시")]
    public Image skillImage;

    [Header("Cooldown Text")]
    public Text cooldownText;

    [Header("Exit Button")]
    public Button exitButton;

    [Header("Open Button")]
    public Button openButton;

    [Header("Skill Upgrade Panel")]
    public GameObject skillUpgradePanel;

    // 기본 버튼 색상 저장
    private Dictionary<Button, Color> buttonDefaultColors = new Dictionary<Button, Color>();
    // 선택된 버튼 색상
    private Color selectedButtonColor = Color.yellow;

    private Skill selectedSkill;

    private void Start()
    {
        // SkillFsm 참조가 설정되지 않았을 경우 에러 출력
        if (skillFsm == null)
        {
            Debug.LogError("[SkillUpgradeUI] SkillFsm 참조가 설정되지 않았습니다.");
            return;
        }

        // 기본 버튼 색상 저장 및 리스너 할당
        for (int i = 0; i < skillSelectionButtons.Count; i++)
        {
            var button = skillSelectionButtons[i];
            if (button != null)
            {
                buttonDefaultColors[button] = button.image.color;

                // 스킬 타입이 유효한지 확인
                if (i < skillTypes.Count)
                {
                    SkillType skillType = skillTypes[i];
                    // 람다 캡처 문제를 방지하기 위해 지역 변수 사용
                    button.onClick.AddListener(() => SelectSkill(skillType));
                }
                else
                {
                    Debug.LogWarning($"[SkillUpgradeUI] 스킬 타입이 버튼 수보다 적습니다. 버튼: {button.name}");
                }
            }
        }

        // 다른 버튼들에 리스너 할당
        if (levelUpButton != null)
            levelUpButton.onClick.AddListener(UpgradeSelectedSkill);

        if (exitButton != null)
            exitButton.onClick.AddListener(CloseSkillUpgradeUI);

        if (openButton != null)
            openButton.onClick.AddListener(OpenSkillUpgradeUI);

        // 초기에는 Skill Upgrade Panel을 숨김
        if (skillUpgradePanel != null)
            skillUpgradePanel.SetActive(false);

        // 선택된 스킬이 없으므로 'M' 표시 초기화
        if (selectedSkillLevelText != null)
        {
            selectedSkillLevelText.text = "";
        }

        // PlayerInputManager의 OnKInput 이벤트에 리스너 추가
        PlayerInputManager.OnKInput += ToggleSkillUpgradeUI;
    }

    private void OnDestroy()
    {
        // 스킬 선택 버튼들의 모든 리스너 해제
        foreach (var button in skillSelectionButtons)
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
            }
        }

        // Upgrade, Exit, Open 버튼의 리스너 해제
        if (levelUpButton != null)
            levelUpButton.onClick.RemoveListener(UpgradeSelectedSkill);

        if (exitButton != null)
            exitButton.onClick.RemoveListener(CloseSkillUpgradeUI);

        if (openButton != null)
            openButton.onClick.RemoveListener(OpenSkillUpgradeUI);

        // OnKInput 이벤트에서 리스너 해제
        PlayerInputManager.OnKInput -= ToggleSkillUpgradeUI;
    }

    // 특정 스킬을 선택하는 메서드
    private void SelectSkill(SkillType skillType)
    {
        // 선택된 스킬 찾기 및 설정
        selectedSkill = skillFsm.GetSkill(skillType);
        if (selectedSkill == null)
        {
            Debug.LogWarning($"[SkillUpgradeUI] {skillType} 스킬을 찾을 수 없습니다.");
            return;
        }

        // UI 요소 업데이트
        UpdateSkillName();
        UpdateSkillDescription();
        UpdateSelectedSkillLevelText();
        UpdateSkillImage();
        UpdateCooldownText();
        UpdateButtonColors(skillType);
        UpdateUpgradeButtonState();
    }

    // 선택된 스킬을 업그레이드하는 메서드
    public void UpgradeSelectedSkill()
    {
        if (selectedSkill == null)
        {
            Debug.LogWarning("[SkillUpgradeUI] 업그레이드할 스킬이 선택되지 않았습니다.");
            return;
        }

        // 선택된 스킬 업그레이드 시도
        bool success = skillFsm.LevelUpSkill(selectedSkill.skillType);
        if (success)
        {
            Debug.Log($"[SkillUpgradeUI] {selectedSkill.skillType} 스킬을 업그레이드했습니다. 현재 레벨: {selectedSkill.Level}");

            // 스킬 레벨 텍스트 업데이트
            UpdateSelectedSkillLevelText();

            // 업그레이드 버튼 상태 업데이트
            UpdateUpgradeButtonState();

            // 쿨타임 텍스트 업데이트
            UpdateCooldownText();
        }
        else
        {
            Debug.LogWarning($"[SkillUpgradeUI] {selectedSkill.skillType} 스킬 업그레이드에 실패했습니다.");
        }
    }

    // 스킬 이름 업데이트
    private void UpdateSkillName()
    {
        if (skillNameText == null || selectedSkill == null)
            return;

        skillNameText.text = $"스킬: {selectedSkill.Name}";
    }

    // 스킬 설명 업데이트
    private void UpdateSkillDescription()
    {
        if (skillDescriptionText == null || selectedSkill == null)
            return;

        skillDescriptionText.text = $"설명: {selectedSkill.Description}";
    }

    // 선택된 스킬 레벨 텍스트 업데이트
    private void UpdateSelectedSkillLevelText()
    {
        if (selectedSkillLevelText == null || selectedSkill == null)
            return;

        if (selectedSkill.Level >= selectedSkill.MaxLevel)
        {
            // 최대 레벨 도달 시 'M'을 빨간색으로 표시
            selectedSkillLevelText.text = $"<color=red>M</color>";
        }
        else
        {
            // 레벨 표시
            selectedSkillLevelText.text = $"레벨: {selectedSkill.Level}";
        }
    }

    // 스킬 이미지 업데이트
    private void UpdateSkillImage()
    {
        if (skillImage == null || selectedSkill == null)
            return;

        skillImage.sprite = selectedSkill.Icon;
        skillImage.enabled = skillImage.sprite != null; // 이미지가 있을 때만 활성화
    }

    // 쿨타임 텍스트 업데이트
    private void UpdateCooldownText()
    {
        if (cooldownText == null || selectedSkill == null)
            return;

        // 소수점 두 자리까지 표시
        cooldownText.text = $"쿨타임: {selectedSkill.CachedCooldown:F2} 초";
    }

    // 버튼 색상 업데이트
    private void UpdateButtonColors(SkillType selectedSkillType)
    {
        // 모든 버튼 색상을 기본 색상으로 초기화
        ResetAllButtonColors();

        // 선택된 버튼의 색상을 노란색으로 변경
        for (int i = 0; i < skillSelectionButtons.Count; i++)
        {
            var button = skillSelectionButtons[i];
            if (button != null && i < skillTypes.Count)
            {
                if (skillTypes[i] == selectedSkillType)
                {
                    button.image.color = selectedButtonColor;
                }
            }
        }
    }

    // 모든 스킬 선택 버튼의 색상을 기본 색상으로 복원
    private void ResetAllButtonColors()
    {
        foreach (var button in skillSelectionButtons)
        {
            if (button != null && buttonDefaultColors.ContainsKey(button))
            {
                button.image.color = buttonDefaultColors[button];
            }
        }
    }

    // 업그레이드 버튼의 활성화 상태를 업데이트
    public void UpdateUpgradeButtonState()
    {
        if (selectedSkill == null || levelUpButton == null)
            return;

        if (selectedSkill.Level >= selectedSkill.MaxLevel)
        {
            // 최대 레벨에 도달한 경우 업그레이드 버튼 비활성화
            levelUpButton.interactable = false;
            Debug.Log($"[SkillUpgradeUI] {selectedSkill.skillType} 스킬은 이미 최대 레벨({selectedSkill.MaxLevel})에 도달했습니다.");
        }
        else
        {
            // 최대 레벨에 도달하지 않은 경우 업그레이드 버튼 활성화
            levelUpButton.interactable = true;
        }
    }

    // Skill Upgrade UI 패널을 닫는 메서드
    public void CloseSkillUpgradeUI()
    {
        if (skillUpgradePanel != null)
            skillUpgradePanel.SetActive(false);
    }

    // Skill Upgrade UI 패널을 여는 메서드
    public void OpenSkillUpgradeUI()
    {
        if (skillUpgradePanel != null)
            skillUpgradePanel.SetActive(true);

        // 필요 시 기본 스킬 선택 (옵션)
        // SelectSkill(SkillType.Rush);
    }

    // Skill Upgrade UI 패널의 활성화 상태를 토글하는 메서드
    private void ToggleSkillUpgradeUI()
    {
        if (skillUpgradePanel == null)
            return;

        bool isActive = skillUpgradePanel.activeSelf;
        skillUpgradePanel.SetActive(!isActive);
    }
}
