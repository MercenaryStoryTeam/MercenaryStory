using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SkillUpgradeUI : MonoBehaviour
{
    [Header("Skill FSM 스크립트")]
    [SerializeField] private SkillFsm skillFsm;

    [Header("스킬 선택 버튼")]
    [SerializeField] private Button selectRushButton;
    [SerializeField] private Button selectParryButton;
    [SerializeField] private Button selectSkill1Button;
    [SerializeField] private Button selectSkill2Button;

    [Header("레벨 업 버튼")]
    [SerializeField] private Button levelUpButton;

    [Header("Skill Name Text")]
    [SerializeField] private Text skillNameText; // 스킬 이름 표시 텍스트

    [Header("Skill Description Text")]
    [SerializeField] private Text skillDescriptionText;

    [Header("Selected Skill Level Text")]
    [SerializeField] private Text selectedSkillLevelText;

    [Header("Skill Image")]
    [SerializeField] private Image skillImage; // 스킬 이미지 표시 이미지

    [Header("Cooldown Text")]
    [SerializeField] private Text cooldownText; // 쿨타임 표시 텍스트

    [Header("Exit Button")]
    [SerializeField] private Button exitButton;

    [Header("Open Button")]
    [SerializeField] private Button openButton;

    [Header("Skill Upgrade Panel")]
    [SerializeField] private GameObject skillUpgradePanel;

    // 기본 버튼 색상
    private Color defaultButtonColor;
    // 선택된 버튼 색상
    private Color selectedButtonColor = Color.yellow;

    private SkillFsm.Skill selectedSkill;

    private void Start()
    {
        if (skillFsm == null)
        {
            Debug.LogError("[SkillUpgradeUI] SkillFsm 참조가 설정되지 않았습니다.");
            return;
        }

        // 기본 버튼 색상 저장
        if (selectRushButton != null)
            defaultButtonColor = selectRushButton.image.color;

        // Assign listeners to skill selection buttons
        if (selectRushButton != null)
            selectRushButton.onClick.AddListener(() => SelectSkill(SkillType.Rush));

        if (selectParryButton != null)
            selectParryButton.onClick.AddListener(() => SelectSkill(SkillType.Parry));

        if (selectSkill1Button != null)
            selectSkill1Button.onClick.AddListener(() => SelectSkill(SkillType.Skill1));

        if (selectSkill2Button != null)
            selectSkill2Button.onClick.AddListener(() => SelectSkill(SkillType.Skill2));

        // Assign listener to upgrade button
        if (levelUpButton != null)
            levelUpButton.onClick.AddListener(UpgradeSelectedSkill);

        // Assign listener to exit button
        if (exitButton != null)
            exitButton.onClick.AddListener(CloseSkillUpgradeUI);

        // Assign listener to open button
        if (openButton != null)
            openButton.onClick.AddListener(OpenSkillUpgradeUI);

        // Initially hide the Skill Upgrade Panel
        if (skillUpgradePanel != null)
            skillUpgradePanel.SetActive(false);
    }

    // Update 메서드에서 쿨타임 텍스트 업데이트를 제거하거나 수정
    private void Update()
    {
        // 쿨타임 텍스트는 선택 시와 업그레이드 후에만 업데이트하므로, 여기서는 업데이트하지 않습니다.
    }

    private void SelectSkill(SkillType skillType)
    {
        // Find and set the selected skill
        selectedSkill = skillFsm.GetSkill(skillType);
        if (selectedSkill == null)
        {
            Debug.LogWarning($"[SkillUpgradeUI] {skillType} 스킬을 찾을 수 없습니다.");
            return;
        }

        // Update the skill name, description, and level texts
        UpdateSkillName();
        UpdateSkillDescription();
        UpdateSelectedSkillLevelText();

        // Update the skill image
        UpdateSkillImage();

        // Update the cooldown text (전체 쿨타임만 표시)
        UpdateCooldownText();

        // Change button colors to indicate selection
        UpdateButtonColors(skillType);

        // Check if the selected skill has reached max level
        UpdateUpgradeButtonState();
    }

    private void UpgradeSelectedSkill()
    {
        if (selectedSkill == null)
        {
            Debug.LogWarning("[SkillUpgradeUI] 업그레이드할 스킬이 선택되지 않았습니다.");
            return;
        }

        // Upgrade the selected skill
        bool success = skillFsm.LevelUpSkill(selectedSkill.skillType);
        if (success)
        {
            Debug.Log($"[SkillUpgradeUI] {selectedSkill.skillType} 스킬을 업그레이드했습니다. 현재 레벨: {selectedSkill.level}");

            // Update the skill level text
            UpdateSelectedSkillLevelText();

            // Update the upgrade button state based on new level
            UpdateUpgradeButtonState();

            // Update the cooldown display in case cooldown changes
            UpdateCooldownText();
        }
    }

    private void UpdateSkillName()
    {
        if (skillNameText == null || selectedSkill == null)
            return;

        // Display the selected skill's name
        skillNameText.text = $"Skill: {selectedSkill.Name}";
    }

    private void UpdateSkillDescription()
    {
        if (skillDescriptionText == null || selectedSkill == null)
            return;

        // Display the selected skill's description
        skillDescriptionText.text = $"Description: {selectedSkill.description}";
    }

    private void UpdateSelectedSkillLevelText()
    {
        if (selectedSkillLevelText == null || selectedSkill == null)
            return;

        // Display the selected skill's current level
        selectedSkillLevelText.text = $"Level: {selectedSkill.level}";
    }

    private void UpdateSkillImage()
    {
        if (skillImage == null || selectedSkill == null)
            return;

        // Display the selected skill's image
        skillImage.sprite = selectedSkill.icon;
        skillImage.enabled = selectedSkill.icon != null; // 이미지가 있을 때만 활성화
    }

    private void UpdateCooldownText()
    {
        if (cooldownText == null || selectedSkill == null)
            return;

        // 전체 쿨타임만 표시
        // F2: 소수점 자리수 
        cooldownText.text = $"쿨타임: {selectedSkill.CurrentCooldown:F2} 초";
    }

    private void UpdateButtonColors(SkillType selectedSkillType)
    {
        // Reset all buttons to default color
        ResetAllButtonColors();

        // Set the selected button to yellow
        switch (selectedSkillType)
        {
            case SkillType.Rush:
                if (selectRushButton != null)
                    selectRushButton.image.color = selectedButtonColor;
                break;
            case SkillType.Parry:
                if (selectParryButton != null)
                    selectParryButton.image.color = selectedButtonColor;
                break;
            case SkillType.Skill1:
                if (selectSkill1Button != null)
                    selectSkill1Button.image.color = selectedButtonColor;
                break;
            case SkillType.Skill2:
                if (selectSkill2Button != null)
                    selectSkill2Button.image.color = selectedButtonColor;
                break;
        }
    }

    private void ResetAllButtonColors()
    {
        if (selectRushButton != null)
            selectRushButton.image.color = defaultButtonColor;

        if (selectParryButton != null)
            selectParryButton.image.color = defaultButtonColor;

        if (selectSkill1Button != null)
            selectSkill1Button.image.color = defaultButtonColor;

        if (selectSkill2Button != null)
            selectSkill2Button.image.color = defaultButtonColor;
    }

    private void UpdateUpgradeButtonState()
    {
        if (selectedSkill == null || levelUpButton == null)
            return;

        if (selectedSkill.level >= selectedSkill.maxLevel)
        {
            // Disable the upgrade button if max level reached
            levelUpButton.interactable = false;
            Debug.Log($"[SkillUpgradeUI] {selectedSkill.skillType} 스킬은 이미 최대 레벨({selectedSkill.maxLevel})에 도달했습니다.");
        }
        else
        {
            // Enable the upgrade button if not at max level
            levelUpButton.interactable = true;
        }
    }

    private void CloseSkillUpgradeUI()
    {
        if (skillUpgradePanel != null)
            skillUpgradePanel.SetActive(false);
    }

    private void OpenSkillUpgradeUI()
    {
        if (skillUpgradePanel != null)
            skillUpgradePanel.SetActive(true);

        // Optionally, select a default skill when opening the UI
        SelectSkill(SkillType.Rush);
    }
}
