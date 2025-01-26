using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

public class PartyPanel : MonoBehaviour
{
	public Text serverText;
	public Button refreshButton;
	public Button createButton;
	public Button requestButton;
	public Button cancelButton;

	public PartyEntry partyEntryPrefab;
	private string currentPartyId;

	private List<PartyData> parties;
	public RectTransform partyContent;

	private Dictionary<string, Toggle> partyToggles =
		new Dictionary<string, Toggle>();

	public ToggleGroup partyEntryToggleGroup;

	private void Awake()
	{
		refreshButton.onClick.AddListener(RefreshPartyList);
		createButton.onClick.AddListener(CreateButtonClick);
		requestButton.onClick.AddListener(RequestButtonClick);
		cancelButton.onClick.AddListener(CancelButtonClick);
		partyEntryToggleGroup = GetComponentInChildren<ToggleGroup>();
	}

	private void OnEnable()
	{
		serverText.text = $"{ServerManager.GetServerName()} 서버 파티 현황";
		RefreshPartyList();
	}

	private void RefreshPartyList()
	{
		foreach (var VARIABLE in partyToggles)
		{
			Destroy(VARIABLE.Value.gameObject);
		}

		partyToggles.Clear();
		// Update chat channels (partyId) and Display(Instantiate entry)
		FirebaseManager.Instance.UpdatePartyAndList();
		parties = null;
		parties = FirebaseManager.Instance.GetPartyList();
		print(JsonConvert.SerializeObject(parties));
		if (parties != null)
		{
			foreach (var party in parties)
			{
				// 가입 가능한 지 확인하기
				// 1. 해당 파티장이 CurrentUser와 같은 room에 있는 가?
				// 2. 인원이 가득 차지 않았는 가?
				if (party.party_size > party.party_Members.Count &&
				    FirebaseManager.Instance.CurrentUserData.user_CurrentServer ==
				    party.party_Owner.user_CurrentServer)
				{
					PartyEntry partyEntry = Instantiate(partyEntryPrefab, partyContent);
					partyEntry.sizeText.text = party.party_size.ToString();
					partyEntry.nameText.text = party.party_Name;
					partyToggles[partyEntry.nameText.text] =
						partyEntry.GetComponent<Toggle>();
					partyToggles[partyEntry.nameText.text].group = partyEntryToggleGroup;
				}
			}
		}
	}

	private void CreateButtonClick()
	{
		// 파티가 이미 있을 경우
		if (FirebaseManager.Instance.CurrentUserData.user_CurrentParty != "")
		{
			UIManager.Instance.popUp.PopUpOpen("이미 파티에 가입되어 있습니다.",
				() => UIManager.Instance.popUp.PopUpClose());
		}
		else
		{
			UIManager.Instance.OpenPartyCreatePanel();
		}
	}

	private void RequestButtonClick()
	{
		foreach (var partyEntry in partyToggles)
		{
			if (partyEntry.Value.isOn)
			{
				FirebaseManager.Instance.JoinParty(partyEntry.Key);
				break;
			}
		}
	}

	private void CancelButtonClick()
	{
		UIManager.Instance.ClosePartyPanel();
	}
}