using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberPanel : MonoBehaviour
{
	public Text nameText;
	public Button cancelButton;
	public Button exitButton;
	public PartyMemberEntry PartyMemberEntryPrefab;

	public List<UserData> partyMembers = new List<UserData>();

	public List<PartyMemberEntry> memberEntries = new List<PartyMemberEntry>();

	public RectTransform content;

	private void Awake()
	{
		cancelButton.onClick.AddListener(CancelButtonClick);
		exitButton.onClick.AddListener(ExitButtonClick);
	}

	private void OnEnable()
	{
		RefreshPartyMembers();
		nameText.text = FirebaseManager.Instance.CurrentPartyData.party_Name;
	}

	private void RefreshPartyMembers()
	{
		foreach (var member in memberEntries)
		{
			Destroy(member.gameObject);
		}

		memberEntries.Clear();
		FirebaseManager.Instance.UpdatePartyAndList();
		partyMembers = FirebaseManager.Instance.GetCurrentPartyMembers();
		if (partyMembers != null)
		{
			foreach (var member in partyMembers)
			{
				PartyMemberEntry memberEntry =
					Instantiate(PartyMemberEntryPrefab, content);
				if (member.user_Id ==
				    FirebaseManager.Instance.CurrentPartyData.party_Owner.user_Id)
				{
					memberEntry.roleText.text = "파티장";
				}
				else
				{
					memberEntry.roleText.text = "파티원";
				}

				memberEntry.nameText.text = member.user_Name;
				memberEntries.Add(memberEntry);
			}
		}
	}

	private void CancelButtonClick()
	{
		UIManager.Instance.ClosePartyPanel();
	}

	private void ExitButtonClick()
	{
		FirebaseManager.Instance.ExitParty();
		UIManager.Instance.ClosePartyPanel();
	}
}