using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// 스킬 업그레이드 UI 관리
public class SkillUIManager : MonoBehaviour
{
    [Header("Skill FSM 스크립트")]
    public SkillFsm skillFsm;

    [Header("SkillPanel")]
    public GameObject skillPanel;

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

    // 기본 버튼 색상 저장
    private Dictionary<Button, Color> buttonDefaultColors = new Dictionary<Button, Color>();
    // 선택된 버튼 색상
    private Color selectedButtonColor = Color.yellow;
    // 현재 선택된 스킬을 저장하는 변수
    private Skill selectedSkill;
    // Player 스크립트 참조
    private Player player;

    private void OnEnable()
    {
        // 씬 로드 이벤트 구독
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;

        // 현재 씬이 이미 로드된 상태라면 참조 시도
        StartCoroutine(FindReferencesRoutine());
    }

    private void OnDisable()
    {
        // 씬 로드 이벤트 해제
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;

        // 코루틴 정지
        StopAllCoroutines();
    }

    /// <summary>
    /// 씬이 로드될 때 호출되는 메서드
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(FindReferencesRoutine());
    }

    /// <summary>
    /// 스크립트에서 필요한 Player, SkillFsm 등을 레이어/태그 조건에 따라 찾아서 할당하는 코루틴
    /// </summary>
    private IEnumerator FindReferencesRoutine()
    {
        while (true)
        {
            if (skillFsm == null || player == null)
            {
                // 씬 내 모든 오브젝트를 탐색
                GameObject[] allGameObjects = FindObjectsOfType<GameObject>();
                foreach (var go in allGameObjects)
                {
                    // 태그가 "Player"이고, 이름에 "Clone"이 포함되지 않은 오브젝트 찾기
                    if (go.CompareTag("Player") && !go.name.Contains("Clone"))
                    {
                        if (skillFsm == null)
                            skillFsm = go.GetComponent<SkillFsm>();

                        if (player == null)
                            player = go.GetComponent<Player>();
                    }
                }

                // Player 스크립트를 찾지 못했다면 경고 로그 출력
                if (player == null)
                {
                    Debug.LogWarning("[SkillUpgradeUI] Player 인스턴스를 찾지 못했습니다. 1초 후 다시 시도합니다.");
                }
                else
                {
                    // 플레이어의 골드 변경 이벤트 구독
                    player.OnGoldChanged += UpdateGoldDisplay;
                }

                // SkillFsm을 찾지 못했다면 경고 로그 출력
                if (skillFsm == null)
                {
                    Debug.LogWarning("[SkillUpgradeUI] SkillFsm 참조를 찾지 못했습니다. 1초 후 다시 시도합니다.");
                }
            }
            else
            {
                // 필요한 참조를 모두 얻었다면 초기화 메서드 호출
                InitializeUI();
                yield break; // 코루틴 종료
            }

            yield return new WaitForSeconds(1f);
        }
    }

    /// <summary>
    /// UI 초기화 및 리스너 할당
    /// </summary>
    private void InitializeUI()
    {
        // 스킬 선택 버튼의 기본 색상 저장 및 리스너 할당
        if (skillButtons != null)
        {
            for (int i = 0; i < skillButtons.Count; i++)
            {
                var button = skillButtons[i];
                if (button != null)
                {
                    buttonDefaultColors[button] = button.image.color;
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
        }

        // 다른 버튼들에 리스너 할당
        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(UpgradeSelectedSkill);

        if (exitButton != null)
            exitButton.onClick.AddListener(CloseSkillUpgradeUI);

        if (openButton != null)
            openButton.onClick.AddListener(OpenSkillUpgradeUI);

        // 초기에는 Skill Upgrade Panel을 숨김
        if (skillPanel != null)
            skillPanel.SetActive(false);

        // 선택된 스킬이 없으므로 'M' 표시 초기화
        if (skillLevelText != null)
        {
            skillLevelText.text = "";
        }

        // PlayerInputManager의 OnKInput 이벤트에 리스너 추가
        PlayerInputManager.OnKInput += ToggleSkillUpgradeUI;

        // 골드 표시 업데이트 (현재 골드 값 전달)
        UpdateGoldDisplay(FirebaseManager.Instance.CurrentUserData.user_Gold);
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

        // OnKInput 이벤트에서 리스너 해제
        PlayerInputManager.OnKInput -= ToggleSkillUpgradeUI;

        // Player의 골드 변경 이벤트 해제
        if (player != null)
        {
            player.OnGoldChanged -= UpdateGoldDisplay;
        }
    }

    // 특정 스킬을 선택하는 메서드
    private void SelectSkill(SkillType skillType)
    {
        SoundManager.Instance.PlaySFX("Fantasy Click 2", gameObject);

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
        UpdateUpgradeCostDisplay();
    }

    // 선택된 스킬을 업그레이드하는 메서드
    public void UpgradeSelectedSkill()
    {
        if (selectedSkill == null)
        {
            Debug.LogWarning("[SkillUpgradeUI] 업그레이드할 스킬이 선택되지 않았습니다.");
            return;
        }

        if (selectedSkill.Level >= selectedSkill.MaxLevel)
        {
            Debug.LogWarning($"[SkillUpgradeUI] {selectedSkill.skillType} 스킬은 이미 최대 레벨({selectedSkill.MaxLevel})에 도달했습니다.");
            return;
        }

        // 다음 레벨 업그레이드 비용 가져오기
        float upgradeCost = selectedSkill.UpgradeCosts[selectedSkill.Level - 1]; // 레벨은 1부터 시작

        // 플레이어가 충분한 골드를 가지고 있는지 확인
        if (player.SpendGold(upgradeCost))
        {
            // 스킬 레벨업 시도
            bool success = skillFsm.LevelUpSkill(selectedSkill.skillType);
            if (success)
            {
                Debug.Log($"[SkillUpgradeUI] {selectedSkill.skillType} 스킬을 업그레이드했습니다. 현재 레벨: {selectedSkill.Level}");

                // 레벨업 사운드 재생
                SoundManager.Instance.PlaySFX("RankUP", gameObject);

                // UI 요소 업데이트
                UpdateSelectedSkillLevelText();
                UpdateUpgradeButtonState();
                UpdateCooldownText();
                UpdateUpgradeCostDisplay();
            }
            else
            {
                Debug.LogWarning($"[SkillUpgradeUI] {selectedSkill.skillType} 스킬 업그레이드에 실패했습니다.");
            }
        }
        else
        {
            Debug.LogWarning("[SkillUpgradeUI] 골드가 부족하여 스킬을 업그레이드할 수 없습니다.");
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
        if (skillLevelText == null || selectedSkill == null)
            return;

        if (selectedSkill.Level >= selectedSkill.MaxLevel)
        {
            // 최대 레벨 도달 시 'M'을 빨간색으로 표시
            skillLevelText.text = $"<color=red>M</color>";
        }
        else
        {
            // 레벨 표시
            skillLevelText.text = $"레벨: {selectedSkill.Level}";
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

    // 모든 스킬 선택 버튼의 색상을 기본 색상으로 복원
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

    // 업그레이드 버튼의 활성화 상태를 업데이트
    public void UpdateUpgradeButtonState()
    {
        if (selectedSkill == null || upgradeButton == null)
            return;

        if (selectedSkill.Level >= selectedSkill.MaxLevel)
        {
            // 최대 레벨에 도달한 경우 업그레이드 버튼 비활성화
            upgradeButton.interactable = false;
            Debug.Log($"[SkillUpgradeUI] {selectedSkill.skillType} 스킬은 이미 최대 레벨({selectedSkill.MaxLevel})에 도달했습니다.");
        }
        else
        {
            // 다음 업그레이드 비용 가져오기
            float nextUpgradeCost = selectedSkill.UpgradeCosts[selectedSkill.Level - 1];
            // 플레이어가 충분한 골드를 가지고 있는지 확인하여 버튼 활성화 여부 결정
            upgradeButton.interactable = FirebaseManager.Instance.CurrentUserData.user_Gold >= nextUpgradeCost;
        }
    }

    // 스킬 업그레이드 비용 표시 업데이트
    private void UpdateUpgradeCostDisplay()
    {
        if (selectedSkill == null || upgradeCostText == null)
            return;

        if (selectedSkill.Level >= selectedSkill.MaxLevel)
        {
            // "Max 레벨"을 빨간색으로 표시
            upgradeCostText.text = "<color=#FF0000>Max</color> 레벨";
        }
        else
        {
            float nextUpgradeCost = selectedSkill.UpgradeCosts[selectedSkill.Level - 1];
            // 비용을 노란색으로 표시
            upgradeCostText.text = $"비용: <color=#FFFF00>{nextUpgradeCost}</color> 골드";
        }
    }

    // 골드 표시 업데이트
    private void UpdateGoldDisplay(float newGold)
    {
        if (goldText != null)
        {
            goldText.text = $"{newGold}";
        }

        // 업그레이드 버튼 상태도 함께 업데이트
        UpdateUpgradeButtonState();
    }

    // Skill Upgrade UI 패널을 닫는 메서드
    public void CloseSkillUpgradeUI()
    {
        if (skillPanel != null)
            skillPanel.SetActive(false);

        SoundManager.Instance.PlaySFX("ui_off", gameObject);
    }

    // Skill Upgrade UI 패널을 여는 메서드
    public void OpenSkillUpgradeUI()
    {
        if (skillPanel != null)
            skillPanel.SetActive(true);

        SoundManager.Instance.PlaySFX("ui_on", gameObject);
    }

    // Skill Upgrade UI 패널의 활성화 상태를 토글하는 메서드
    private void ToggleSkillUpgradeUI()
    {
        if (skillPanel == null)
            return;

        bool isActive = skillPanel.activeSelf;
        skillPanel.SetActive(!isActive);
    }
}
