using UnityEngine;
using UnityEngine.UI;

public class PartyMemberPanel : MonoBehaviour
{
	public Text nameText;
	public Button cancelButton;
	public Button exitButton;
	public PartyMemberEntry PartyMemberEntryPrefab;

	private void Awake()
	{
		cancelButton.onClick.AddListener(CancelButtonClick);
		exitButton.onClick.AddListener(ExitButtonClick);
	}

	private void OnEnable()
	{
		print(
			$"FirebaseManager.Instance.CurrentPartyData: {FirebaseManager.Instance.CurrentPartyData}");
		print(
			$"FirebaseManager.Instance.CurrentPartyData.party_Name: {FirebaseManager.Instance.CurrentPartyData.party_Name}");
		nameText.text = FirebaseManager.Instance.CurrentPartyData.party_Name;
	}

	private void CancelButtonClick()
	{
		gameObject.SetActive(false);
	}

	private void ExitButtonClick()
	{
		FirebaseManager.Instance.ExitParty();
	}
}