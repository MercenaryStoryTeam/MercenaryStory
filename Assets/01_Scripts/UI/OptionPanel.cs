using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionPanel : MonoBehaviour
{
	public Slider masterSlider;
	public TextMeshProUGUI masterText;
	public Slider bgmSlider;
	public TextMeshProUGUI bgmText;
	public Slider sfxSlider;
	public TextMeshProUGUI sfxText;
	public Button exitButton;
	public Button returnToTownButton;
	public Button gameQuitButton;

	private void Awake()
	{
		// 슬라이더 값 범위 설정
		masterSlider.minValue = 0f;
		masterSlider.maxValue = 100f;
		bgmSlider.minValue = 0f;
		bgmSlider.maxValue = 100f;
		sfxSlider.minValue = 0f;
		sfxSlider.maxValue = 100f;

		// 저장된 값 불러오기 (0~1 범위를 0~100으로 변환)
		float masterValue = PlayerPrefs.GetFloat("Master", 1f) * 100f;
		float bgmValue = PlayerPrefs.GetFloat("BGM", 1f) * 100f;
		float sfxValue = PlayerPrefs.GetFloat("SFX", 1f) * 100f;

		// 슬라이더와 텍스트 초기값 설정
		masterSlider.value = masterValue;
		masterText.text = $"{(int)masterValue}%";
		SoundManager.Instance.SetMasterVolume(masterValue / 100f);

		bgmSlider.value = bgmValue;
		bgmText.text = $"{(int)bgmValue}%";
		SoundManager.Instance.SetBGMVolume(bgmValue / 100f);

		sfxSlider.value = sfxValue;
		sfxText.text = $"{(int)sfxValue}%";
		SoundManager.Instance.SetSFXVolume(sfxValue / 100f);

		masterSlider.onValueChanged.AddListener(OnMasterSliderChanged);
		bgmSlider.onValueChanged.AddListener(OnBGMSliderChanged);
		sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);

		gameQuitButton.onClick.AddListener(gameQuitButtonClick);
		returnToTownButton.onClick.AddListener(returnToTownButtonClick);
		exitButton.onClick.AddListener(exitButtonClick);
	}

	private void OnEnable()
	{
		if (GameManager.Instance.CurrentScene > 1)
		{
			gameQuitButton.gameObject.SetActive(false);
			returnToTownButton.gameObject.SetActive(true);
		}
		else
		{
			gameQuitButton.gameObject.SetActive(true);
			returnToTownButton.gameObject.SetActive(false);
		}

		// 서버에 저장하기
		FirebaseManager.Instance.UploadCurrentUserData();
	}

	private void exitButtonClick()
	{
		UIManager.Instance.CloseOptionPanel();
	}

	private void returnToTownButtonClick()
	{
		GameManager.Instance.currentPlayerFsm.ReturnToTown();
		UIManager.Instance.CloseAllPanels();
	}

	private void gameQuitButtonClick()
	{
		Application.Quit();
	}

	private void OnMasterSliderChanged(float value)
	{
		masterText.text = $"{(int)value}%";
		SoundManager.Instance.SetMasterVolume(value / 100f);
	}

	private void OnBGMSliderChanged(float value)
	{
		bgmText.text = $"{(int)value}%";
		SoundManager.Instance.SetBGMVolume(value / 100f);
	}

	private void OnSFXSliderChanged(float value)
	{
		sfxText.text = $"{(int)value}%";
		SoundManager.Instance.SetSFXVolume(value / 100f);
	}
}