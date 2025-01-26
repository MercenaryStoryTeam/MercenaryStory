using UnityEngine;
using UnityEngine.UI;

public class PartyCreatePanel : MonoBehaviour
{
	public InputField partyName;
	public Toggle size1;
	public Toggle size2;
	public Toggle size3;
	public Toggle size4;
	public Button createButton;
	public Button cancelButton;

	public Sprite checkSprite;
	public Sprite defaultSprite;

	private int size = 0;

	private void Awake()
	{
		createButton.onClick.AddListener(CreateButtonClick);
		cancelButton.onClick.AddListener(CancelButtonClick);
		size1.onValueChanged.AddListener(ToggleSize1);
		size2.onValueChanged.AddListener(ToggleSize2);
		size3.onValueChanged.AddListener(ToggleSize3);
		size4.onValueChanged.AddListener(ToggleSize4);
	}

	private void ToggleSize1(bool value)
	{
		size1.GetComponent<Image>().sprite = value ? checkSprite : defaultSprite;
	}

	private void ToggleSize2(bool value)
	{
		size2.GetComponent<Image>().sprite = value ? checkSprite : defaultSprite;
	}

	private void ToggleSize3(bool value)
	{
		size3.GetComponent<Image>().sprite = value ? checkSprite : defaultSprite;
	}

	private void ToggleSize4(bool value)
	{
		size4.GetComponent<Image>().sprite = value ? checkSprite : defaultSprite;
	}

	private void CreateButtonClick()
	{
		if (size1.isOn) size = 1;
		else if (size2.isOn) size = 2;
		else if (size3.isOn) size = 3;
		else if (size4.isOn) size = 4;

		FirebaseManager.Instance.CreateParty(partyName.text, size);
	}

	private void CancelButtonClick()
	{
		UIManager.Instance.ClosePartyCreatePanel();
	}
}