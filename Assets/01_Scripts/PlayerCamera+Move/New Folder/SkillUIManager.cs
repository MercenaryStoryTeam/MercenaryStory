using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// 스킬창을 위한 스크립트
public class SkillUIManager : MonoBehaviour
{
    [Header("Skill FSM 스크립트")] public SkillFsm skillFsm;
    [Header("SkillPanel")] public GameObject skillPanel;

    // 버튼, 타입 리스트 추후에 묶어서 관리
    [Header("스킬 선택 버튼")] public List<Button> skillButtons;
    [Header("스킬 유형")] public List<SkillType> skillTypes;
    [Header("업그레이드 버튼")] public Button upgradeButton;
    [Header("업그레이드 비용 표시 텍스트")] public Text upgradeCostText;
    [Header("스킬 이름 표시")] public Text skillNameText;
    [Header("스킬 설명 표시")] public Text skillDescriptionText;
    [Header("스킬 레벨 표시")] public Text skillLevelText;
    [Header("스킬 이미지 표시")] public Image skillImage;
    [Header("쿨타임 표시")] public Text cooldownText;
    [Header("닫기 버튼")] public Button exitButton;
    [Header("열기 버튼")] public Button openButton;
    [Header("골드 표시 텍스트")] public Text goldText;

    // GoldManager 참조 추가
    [Header("GoldManager")] public GoldManager goldManager;

    // 초기 skillImage 스프라이트를 저장해둘 변수
    private Sprite defaultSkillSprite;

    // 기본 버튼 색상 저장
    private Dictionary<Button, Color> buttonDefaultColors = new Dictionary<Button, Color>();

    // 선택된 버튼 색상
    private Color selectedButtonColor = Color.yellow;

    // 현재 선택된 스킬을 저장하는 변수
    private Skill selectedSkill;

    private void OnEnable()
    {
        // 씬 로드 이벤트 구독
        SceneManager.sceneLoaded += OnSceneLoaded;

        // 현재 씬이 이미 로드된 상태라면 참조 시도
        StartCoroutine(FindReferencesRoutine());
    }

    private void OnDisable()
    {
        // 씬 로드 이벤트 해제
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // 코루틴 정지
        StopAllCoroutines();

        // 이벤트 해제
        if (goldManager != null)
        {
            goldManager.OnGoldChanged -= UpdateGoldDisplay;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(FindReferencesRoutine());
    }

    private IEnumerator FindReferencesRoutine()
    {
        while (true)
        {
            // 모든 "Player" 태그가 붙은 오브젝트 찾기
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            GameObject player = null;

            foreach (var go in players)
            {
                if (!go.name.Contains("Clone"))
                {
                    player = go;
                    break;
                }
            }

            if (player != null)
            {
                // SkillFsm 참조 설정
                if (skillFsm == null)
                {
                    skillFsm = player.GetComponent<SkillFsm>();
                    if (skillFsm == null)
                    {
                        Debug.LogWarning("[SkillUIManager] SkillFsm 컴포넌트를 찾을 수 없습니다. 1초 후 다시 시도합니다.");
                    }
                }

                // GoldManager 참조 설정
                if (goldManager == null)
                {
                    goldManager = player.GetComponent<GoldManager>();
                    if (goldManager == null)
                    {
                        Debug.LogWarning("[SkillUIManager] GoldManager 컴포넌트를 찾을 수 없습니다. 1초 후 다시 시도합니다.");
                    }
                    else
                    {
                        // 이벤트 중복 방지 후 구독
                        goldManager.OnGoldChanged -= UpdateGoldDisplay;
                        goldManager.OnGoldChanged += UpdateGoldDisplay;
                        Debug.Log("[SkillUIManager] GoldManager 이벤트 구독 완료.");
                    }
                }

                // 두 참조가 모두 설정되었는지 확인
                if (skillFsm != null && goldManager != null)
                {
                    InitializeUI();
                    yield break; // 코루틴 종료
                }
            }
            else
            {
                Debug.LogWarning("[SkillUIManager] 이름에 'Clone'이 포함되지 않은 Player 오브젝트를 찾을 수 없습니다. 1초 후 다시 시도합니다.");
            }

            // 1초 대기 후 다시 시도
            yield return new WaitForSeconds(1f);
        }
    }

    private void InitializeUI()
    {
        // 현재 skillImage가 가지고 있는 기본 스프라이트를 저장
        if (skillImage != null)
            defaultSkillSprite = skillImage.sprite;

        // 스킬 선택 버튼의 리스너를 등록하기 전에 모두 제거
        if (skillButtons != null)
        {
            for (int i = 0; i < skillButtons.Count; i++)
            {
                var button = skillButtons[i];
                if (button != null)
                {
                    button.onClick.RemoveAllListeners(); 
                    buttonDefaultColors[button] = button.image.color;

                    if (i < skillTypes.Count)
                    {
                        SkillType skillType = skillTypes[i];
                        int index = i;
                        button.onClick.AddListener(() => SelectSkill(skillTypes[index]));
                    }
                    else
                    {
                        Debug.LogWarning("[SkillUIManager] 스킬 타입이 버튼 수보다 적습니다. 버튼: " + button.name);
                    }
                }
            }
        }

        // 업그레이드 버튼 리스너도 동일하게 중복 등록 방지
        if (upgradeButton != null)
        {
            upgradeButton.onClick.RemoveAllListeners();
            upgradeButton.onClick.AddListener(UpgradeSelectedSkill);
        }

        // 닫기 버튼 리스너
        if (exitButton != null)
        {
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(CloseSkillUpgradeUI);
        }

        // 열기 버튼 리스너
        if (openButton != null)
        {
            openButton.onClick.RemoveAllListeners();
            openButton.onClick.AddListener(OpenSkillUpgradeUI);
        }

        // 초기에는 Skill Upgrade Panel을 숨김
        if (skillPanel != null)
            skillPanel.SetActive(false);

        // 선택된 스킬이 없으므로 'M' 표시 초기화
        if (skillLevelText != null)
            skillLevelText.text = "";

        // 골드 표시 업데이트
        if (goldManager != null)
        {
            UpdateGoldDisplay(goldManager.GetCurrentGold());
        }
    }

    private void OnDestroy()
    {
        // 스킬 선택 버튼들의 모든 리스너 해제
        foreach (var button in skillButtons)
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
            }
        }

        // Upgrade, Exit, Open 버튼의 리스너 해제
        if (upgradeButton != null)
            upgradeButton.onClick.RemoveListener(UpgradeSelectedSkill);

        if (exitButton != null)
            exitButton.onClick.RemoveListener(CloseSkillUpgradeUI);

        if (openButton != null)
            openButton.onClick.RemoveListener(OpenSkillUpgradeUI);

        // Player의 골드 변경 이벤트 해제
        if (goldManager != null)
        {
            goldManager.OnGoldChanged -= UpdateGoldDisplay;
        }
    }

    private void SelectSkill(SkillType skillType)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX("Fantasy Click 2", gameObject);
        }

        selectedSkill = skillFsm.GetSkill(skillType);
        if (selectedSkill == null)
        {
            Debug.LogWarning("[SkillUIManager] " + skillType + " 스킬을 찾을 수 없습니다.");
            return;
        }

        UpdateSkillName();
        UpdateSkillDescription();
        UpdateSelectedSkillLevelText();
        UpdateSkillImage();
        UpdateCooldownText();
        UpdateButtonColors(skillType);
        UpdateUpgradeButtonState();
        UpdateUpgradeCostDisplay();
    }

    public void UpgradeSelectedSkill()
    {
        if (selectedSkill == null)
        {
            Debug.LogWarning("[SkillUIManager] 업그레이드할 스킬이 선택되지 않았습니다.");
            return;
        }

        if (selectedSkill.Level >= selectedSkill.MaxLevel)
        {
            Debug.LogWarning("[SkillUIManager] " + selectedSkill.skillType + " 스킬은 이미 최대 레벨(" + selectedSkill.MaxLevel + ")에 도달했습니다.");
            return;
        }

        float upgradeCost = selectedSkill.UpgradeCosts[selectedSkill.Level - 1];

        if (goldManager.SpendGold(upgradeCost))
        {
            bool success = skillFsm.LevelUpSkill(selectedSkill.skillType);
            if (success)
            {
                Debug.Log("[SkillUIManager] " + selectedSkill.skillType + " 스킬 업그레이드 완료. 현재 레벨: " + selectedSkill.Level);
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlaySFX("RankUP", gameObject);
                }
                UpdateSelectedSkillLevelText();
                UpdateUpgradeButtonState();
                UpdateCooldownText();
                UpdateUpgradeCostDisplay();
            }
            else
            {
                Debug.LogWarning("[SkillUIManager] " + selectedSkill.skillType + " 스킬 업그레이드 실패.");
            }
        }
        else
        {
            Debug.LogWarning("[SkillUIManager] 골드가 부족하여 스킬 업그레이드 불가.");
        }
    }

    private void UpdateSkillName()
    {
        if (skillNameText == null || selectedSkill == null) return;
        skillNameText.text = "스킬: " + selectedSkill.Name;
    }

    private void UpdateSkillDescription()
    {
        if (skillDescriptionText == null || selectedSkill == null) return;
        skillDescriptionText.text = "설명: " + selectedSkill.Description;
    }

    private void UpdateSelectedSkillLevelText()
    {
        if (skillLevelText == null || selectedSkill == null) return;

        if (selectedSkill.Level >= selectedSkill.MaxLevel)
        {
            skillLevelText.text = "<color=red>M</color>";
        }
        else
        {
            skillLevelText.text = "레벨: " + selectedSkill.Level;
        }
    }

    private void UpdateSkillImage()
    {
        if (skillImage == null || selectedSkill == null) return;
        skillImage.sprite = selectedSkill.Icon;
        skillImage.enabled = (skillImage.sprite != null);
    }

    private void UpdateCooldownText()
    {
        if (cooldownText == null || selectedSkill == null) return;
        cooldownText.text = "쿨타임: " + selectedSkill.CachedCooldown.ToString("F2") + " 초";
    }

    private void UpdateButtonColors(SkillType selectedSkillType)
    {
        ResetAllButtonColors();

        for (int i = 0; i < skillButtons.Count; i++)
        {
            var button = skillButtons[i];
            if (button != null && i < skillTypes.Count)
            {
                if (skillTypes[i] == selectedSkillType)
                {
                    button.image.color = selectedButtonColor;
                }
            }
        }
    }

    private void ResetAllButtonColors()
    {
        foreach (var button in skillButtons)
        {
            if (button != null && buttonDefaultColors.ContainsKey(button))
            {
                button.image.color = buttonDefaultColors[button];
            }
        }
    }

    public void UpdateUpgradeButtonState()
    {
        if (selectedSkill == null || upgradeButton == null || goldManager == null) return;

        if (selectedSkill.Level >= selectedSkill.MaxLevel)
        {
            upgradeButton.interactable = false;
            Debug.Log("[SkillUIManager] " + selectedSkill.skillType + " 스킬은 이미 최대 레벨(" + selectedSkill.MaxLevel + ")입니다.");
        }
        else
        {
            float nextUpgradeCost = selectedSkill.UpgradeCosts[selectedSkill.Level - 1];
            upgradeButton.interactable = goldManager.GetCurrentGold() >= nextUpgradeCost;
        }
    }

    private void UpdateUpgradeCostDisplay()
    {
        if (selectedSkill == null || upgradeCostText == null) return;

        if (selectedSkill.Level >= selectedSkill.MaxLevel)
        {
            upgradeCostText.text = "<color=#FF0000>Max</color> 레벨";
        }
        else
        {
            float nextUpgradeCost = selectedSkill.UpgradeCosts[selectedSkill.Level - 1];
            upgradeCostText.text = "비용: <color=#FFFF00>" + nextUpgradeCost + "</color> 골드";
        }
    }

    public void UpdateGoldDisplay(float newGold)
    {
        if (goldText != null)
        {
            goldText.text = newGold.ToString();
            Debug.Log($"[SkillUIManager] 골드 텍스트 업데이트: {newGold}");
        }
        else
        {
            Debug.LogWarning("[SkillUIManager] goldText 참조가 null입니다.");
        }
        UpdateUpgradeButtonState();
    }

    public void CloseSkillUpgradeUI()
    {
        if (skillPanel != null)
        {
            UIManager.Instance.CloseSkillPanel();
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX("ui_off", gameObject);
            }

            selectedSkill = null;
            ResetAllButtonColors();

            skillNameText.text = "스킬 상태창";
            skillDescriptionText.text = "스킬 설명";
            skillLevelText.text = "";
            skillImage.sprite = defaultSkillSprite;
            cooldownText.text = "";
            upgradeCostText.text = "업그레이드";
        }
    }

    public void OpenSkillUpgradeUI()
    {
        if (UIManager.Instance.IsAnyPanelOpen() || UIManager.Instance.IsPopUpOpen())
            return;

        if (skillPanel != null)
        {
            UIManager.Instance.OpenSkillPanel();
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX("ui_on", gameObject);
            }
            RefreshSelectedSkillUI();
        }
    }

    private void RefreshSelectedSkillUI()
    {
        if (selectedSkill == null) return;

        UpdateSkillName();
        UpdateSkillDescription();
        UpdateSelectedSkillLevelText();
        UpdateSkillImage();
        UpdateCooldownText();
        UpdateButtonColors(selectedSkill.skillType);
        UpdateUpgradeButtonState();
        UpdateUpgradeCostDisplay();
    }
}
