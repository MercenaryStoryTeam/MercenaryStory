using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Newtonsoft.Json;
using UnityEngine;

public class FirebaseManager : SingletonManager<FirebaseManager>
{
	public FirebaseApp App { get; private set; }
	public FirebaseAuth Auth { get; private set; }
	public FirebaseDatabase DB { get; private set; }

	private Dictionary<string, UserData> userDictionary;
	private List<UserData> userList;
	private DatabaseReference partiesRef;
	private Dictionary<string, PartyData> partyDictionary;
	private List<PartyData> partyList;

	public UserData CurrentUserData { get; private set; }
	public PartyData CurrentPartyData { get; private set; }

	public enum State
	{
		EmailNotChecked,
		EmailChecked
	}

	public State state = State.EmailNotChecked;

	private async void Start()
	{
		state = State.EmailNotChecked;
		DependencyStatus status = await FirebaseApp.CheckAndFixDependenciesAsync();
		if (status == DependencyStatus.Available)
		{
			App = FirebaseApp.DefaultInstance;
			Auth = FirebaseAuth.DefaultInstance;
			DB = FirebaseDatabase.DefaultInstance;
			// StartCoroutine(CheckForEmptyPartiesCoroutine());
		}
		else
		{
			Debug.LogWarning($"Firebase initialization failed: {status}");
		}
	}

	#region User Management

	public async Task SignUp(string email, string password, string user_Name,
		Action<FirebaseUser, UserData> callback = null)
	{
		try
		{
			var result =
				await Auth.CreateUserWithEmailAndPasswordAsync(email, password);

			UserData userData = new UserData(result.User.UserId, email, user_Name);
			userData.user_Inventory.Add(
				InventoryManger.Instance.SetBasicItem(InventoryManger.Instance
					.basicEquipWeapon));

			// skill
			userData.user_Skills.Add(new SkillData(0, 1));
			userData.user_Skills.Add(new SkillData(1, 1));
			userData.user_Skills.Add(new SkillData(2, 1));
			userData.user_Skills.Add(new SkillData(3, 1));

			string userDataJson = JsonConvert.SerializeObject(userData);
			await DB.GetReference($"users/{result.User.UserId}")
				.SetRawJsonValueAsync(userDataJson);
			callback?.Invoke(result.User, userData);

			UIManager.Instance.popUp.PopUpOpen("회원가입이 완료되었습니다.", () =>
			{
				UIManager.Instance.popUp.PopUpClose();
				TitleUI.Instance.PanelOpen("SignIn");
			});
		}
		catch (FirebaseException e)
		{
			ExceptionManager.HandleFirebaseException(e);
		}
		catch (Exception e)
		{
			ExceptionManager.HandleException(e);
		}
	}

	public async Task CheckEmail(string email)
	{
		try
		{
			DataSnapshot usersData = await DB.GetReference("users").GetValueAsync();
			userDictionary =
				JsonConvert.DeserializeObject<Dictionary<string, UserData>>(
					usersData.GetRawJsonValue());
			if (userDictionary != null)
			{
				userList = new List<UserData>(userDictionary.Values);
				foreach (UserData userData in userList)
				{
					if (userData.user_Email == email)
					{
						UIManager.Instance.popUp.PopUpOpen("이미 사용중인 email입니다.",
							() => UIManager.Instance.popUp.PopUpClose());
						state = State.EmailNotChecked;
						return;
					}
				}
			}

			UIManager.Instance.popUp.PopUpOpen("사용 가능한 email입니다.",
				() => UIManager.Instance.popUp.PopUpClose());
			state = State.EmailChecked;
		}
		catch (FirebaseException e)
		{
			ExceptionManager.HandleFirebaseException(e);
		}
		catch (Exception e)
		{
			ExceptionManager.HandleException(e);
		}
	}

	public async Task SignIn(string email, string password)
	{
		try
		{
			UIManager.Instance.popUp.WaitPopUpOpen("로그인 중입니다.");
			var result = await Auth.SignInWithEmailAndPasswordAsync(email, password);
			DataSnapshot userSnapshot =
				await DB.GetReference($"users/{result.User.UserId}").GetValueAsync();
			UserData userData = null;
			if (userSnapshot.Exists)
			{
				userData =
					JsonConvert.DeserializeObject<UserData>(
						userSnapshot.GetRawJsonValue());
			}

			CurrentUserData = userData;
			CurrentUserData.UpdateUserData(currentParty: "", isOnline: true, hp: 100);
			// 비동기 작업들을 Task 배열로 생성
			var tasks = new[]
			{
				UploadCurrentUserData("user_CurrentParty", ""),
				UploadCurrentUserData("user_IsOnline", true),
				UploadCurrentUserData("user_HP", 100)
			};

			// 모든 작업 완료 대기
			await Task.WhenAll(tasks);

			InventoryManger.Instance.LoadInventoryFromDatabase();

			UIManager.Instance.popUp.PopUpClose();
			ServerManager.ConnectLobby();
			if (CurrentUserData.user_Appearance == 0)
			{
				TitleUI.Instance.PanelOpen("CharacterSelect");
			}
			else
			{
				TitleUI.Instance.PanelOpen("ServerSelect");
			}
		}
		catch (FirebaseException e)
		{
			ExceptionManager.HandleFirebaseException(e);
		}
		catch (Exception e)
		{
			ExceptionManager.HandleException(e);
		}
	}

	public async Task UploadCurrnetInvenData(string childName,
		List<SlotData> value)
	{
		try
		{
			DatabaseReference targetRef =
				DB.GetReference($"users/{CurrentUserData.user_Id}").Child(childName);

			string jsonData = JsonConvert.SerializeObject(value);
			await targetRef.SetRawJsonValueAsync(jsonData);
		}

		catch (FirebaseException e)
		{
			ExceptionManager.HandleFirebaseException(e);
		}
		catch (Exception e)
		{
			ExceptionManager.HandleException(e);
		}
	}

	public async Task UploadCurrentUserData()
	{
		try
		{
			string userDataJson = JsonConvert.SerializeObject(CurrentUserData);
			await DB.GetReference($"users/{CurrentUserData.user_Id}")
				.SetRawJsonValueAsync(userDataJson);
		}
		catch (FirebaseException e)
		{
			ExceptionManager.HandleFirebaseException(e);
		}
		catch (Exception e)
		{
			ExceptionManager.HandleException(e);
		}
	}

	public async Task UploadCurrentUserData(string childName, object value,
		Action<object> callback = null)
	{
		try
		{
			DatabaseReference targetRef =
				DB.GetReference($"users/{CurrentUserData.user_Id}").Child(childName);
			await targetRef.SetValueAsync(value);
			callback?.Invoke(value);
		}
		catch (FirebaseException e)
		{
			ExceptionManager.HandleFirebaseException(e);
		}
		catch (Exception e)
		{
			ExceptionManager.HandleException(e);
		}
	}

	private async Task UpdateUserList()
	{
		try
		{
			DataSnapshot usersSnapshot =
				await DB.GetReference("users").GetValueAsync();
			userDictionary =
				JsonConvert.DeserializeObject<Dictionary<string, UserData>>(
					usersSnapshot.GetRawJsonValue());

			if (userDictionary == null)
			{
				print("유저가 존재하지 않습니다.");
			}
			else
			{
				userList = new List<UserData>(userDictionary.Values);
			}
		}
		catch (FirebaseException e)
		{
			ExceptionManager.HandleFirebaseException(e);
		}
		catch (Exception e)
		{
			ExceptionManager.HandleException(e);
		}
	}

	#endregion

	#region Party Management

	public async Task CreateParty(string party_Name, int party_Size)
	{
		try
		{
			// Create PartyData
			PartyData partyData = new PartyData(CurrentUserData.user_CurrentServer,
				CurrentUserData.user_CurrentServer, party_Name, party_Size,
				CurrentUserData);
			CurrentPartyData = partyData;

			// Set current user's party data
			CurrentUserData.UpdateUserData(currentParty: CurrentPartyData.party_Id);

			// Upload UserData
			UploadCurrentUserData("user_CurrentParty", CurrentPartyData.party_Id);

			// Upload PartyData
			partiesRef = DB.GetReference($"parties/{CurrentPartyData.party_Id}");
			string partyDataJson = JsonConvert.SerializeObject(partyData);
			await partiesRef.SetRawJsonValueAsync(partyDataJson);

			UIManager.Instance.popUp.PopUpOpen("파티가 생성되었습니다.", () =>
			{
				UIManager.Instance.popUp.PopUpClose();
				// Update party list
				UIManager.Instance.ClosePartyPanel();
			});
		}
		catch (FirebaseException e)
		{
			ExceptionManager.HandleFirebaseException(e);
		}
		catch (Exception e)
		{
			ExceptionManager.HandleException(e);
		}
	}

	public async Task UpdatePartyAndList()
	{
		try
		{
			DataSnapshot partiesData =
				await DB.GetReference("parties").GetValueAsync();

			if (partiesData.Exists)
			{
				partyDictionary =
					JsonConvert.DeserializeObject<Dictionary<string, PartyData>>(
						partiesData.GetRawJsonValue());
				if (partyDictionary != null)
				{
					partyList = new List<PartyData>(partyDictionary.Values);
					if (CurrentPartyData != null)
					{
						if (partyDictionary.TryGetValue(CurrentPartyData.party_Id,
							    out var value))
						{
							CurrentPartyData = value;
						}
					}
				}
				else
				{
					partyList = null;
					print("파티 없음");
				}
			}
			else
			{
				partyList = null;
				print("파티 진짜 없음");
			}
		}
		catch (FirebaseException e)
		{
			ExceptionManager.HandleFirebaseException(e);
		}
		catch (Exception e)
		{
			ExceptionManager.HandleException(e);
		}
	}

	public async Task JoinParty(string name)
	{
		try
		{
			await UpdatePartyList();

			if (partyList != null)
			{
				foreach (PartyData partyData in partyList)
				{
					if (partyData.party_Name == name)
					{
						if (partyData.party_Members.Count < partyData.party_size)
						{
							// 가입
							CurrentPartyData = partyData;
							CurrentPartyData.AddMember(CurrentUserData);
							CurrentUserData.UpdateUserData(
								currentParty: CurrentPartyData.party_Id);
							UploadCurrentUserData("user_CurrentParty",
								CurrentPartyData.party_Id);
							UploadCurrentPartyData();
							UIManager.Instance.popUp.PopUpOpen(
								$"{CurrentPartyData.party_Name}\n파티에 가입하였습니다.",
								() => UIManager.Instance.popUp.PopUpClose());
							UIManager.Instance.ClosePartyPanel();
						}
						else
						{
							UIManager.Instance.popUp.PopUpOpen("파티가 가득 찼습니다.",
								() => UIManager.Instance.popUp.PopUpClose());
						}
					}
				}
			}
		}
		catch (FirebaseException e)
		{
			ExceptionManager.HandleFirebaseException(e);
		}
		catch (Exception e)
		{
			ExceptionManager.HandleException(e);
		}
	}

	public async Task ExitParty()
	{
		try
		{
			if (string.IsNullOrEmpty(CurrentUserData.user_CurrentParty))
			{
				Debug.LogWarning("현재 파티 정보가 없습니다.");
				return;
			}

			// 파티 데이터 가져오기
			string partyId = CurrentUserData.user_CurrentParty;
			partiesRef = DB.GetReference($"parties/{partyId}");
			DataSnapshot partySnapshot = await partiesRef.GetValueAsync();

			if (!partySnapshot.Exists)
			{
				Debug.LogWarning("파티가 존재하지 않습니다.");
				UIManager.Instance.popUp.PopUpOpen("파티가 이미 삭제되었습니다.", () =>
				{
					UIManager.Instance.popUp.PopUpClose();
					UIManager.Instance.OnPartyButtonClick();
				});
				CurrentUserData.user_CurrentParty = "";
				return;
			}

			// 파티 데이터 역직렬화
			PartyData partyData =
				JsonConvert.DeserializeObject<PartyData>(
					partySnapshot.GetRawJsonValue());

			// 파티장이 나가는 경우
			if (partyData.party_Owner.user_Id == CurrentUserData.user_Id)
			{
				Debug.Log("파티장이 파티를 나갑니다.");

				// 모든 멤버의 user_CurrentParty 초기화
				foreach (UserData member in partyData.party_Members)
				{
					DatabaseReference memberRef =
						DB.GetReference($"users/{member.user_Id}");
					await memberRef.Child("user_CurrentParty").SetValueAsync("");
				}

				// Firebase에서 파티 삭제
				await partiesRef.RemoveValueAsync();
			}
			else // 일반 파티원이 나가는 경우
			{
				Debug.Log("파티원이 파티를 나갑니다.");

				// 멤버 제거
				partyData.party_Members.RemoveAll(member =>
					member.user_Id == CurrentUserData.user_Id);

				// 업데이트된 파티 데이터 업로드
				string updatedPartyDataJson = JsonConvert.SerializeObject(partyData);
				await partiesRef.SetRawJsonValueAsync(updatedPartyDataJson);

				// 현재 유저의 user_CurrentParty 초기화
				CurrentUserData.user_CurrentParty = "";
				UploadCurrentUserData("user_CurrentParty",
					CurrentUserData.user_CurrentParty);
			}

			// 로컬 데이터 초기화
			CurrentUserData.user_CurrentParty = "";
			CurrentPartyData = null;

			UIManager.Instance.popUp.PopUpOpen("파티에서 나갔습니다.", () =>
			{
				UIManager.Instance.popUp.PopUpClose();
				UIManager.Instance.partyMemberPanel.gameObject.SetActive(false);
			});
		}
		catch (FirebaseException e)
		{
			ExceptionManager.HandleFirebaseException(e);
		}
		catch (Exception e)
		{
			ExceptionManager.HandleException(e);
		}
	}

	public async Task UploadPartyDataToLoadScene(string serverName)
	{
		try
		{
			CurrentPartyData.UpdatePartyData(serverId: Guid.NewGuid().ToString());
			CurrentPartyData.UpdatePartyData(serverName: serverName);
			await UploadCurrentPartyData();
		}
		catch (FirebaseException e)
		{
			ExceptionManager.HandleFirebaseException(e);
		}
		catch (Exception e)
		{
			ExceptionManager.HandleException(e);
		}
	}

	public async Task UploadCurrentPartyData()
	{
		try
		{
			partiesRef = DB.GetReference($"parties/{CurrentPartyData.party_Id}");
			string partyDataJson = JsonConvert.SerializeObject(CurrentPartyData);
			await partiesRef.SetRawJsonValueAsync(partyDataJson);
		}
		catch (FirebaseException e)
		{
			ExceptionManager.HandleFirebaseException(e);
		}
		catch (Exception e)
		{
			ExceptionManager.HandleException(e);
		}
	}

	public async Task UpdateCurrentPartyDataAndLoadScene(string sceneName)
	{
		try
		{
			DataSnapshot partyData =
				await DB.GetReference($"parties/{CurrentPartyData.party_Id}")
					.GetValueAsync();
			bool isReady = false;

			while (!isReady)
			{
				partyData =
					await DB.GetReference($"parties/{CurrentPartyData.party_Id}")
						.GetValueAsync();
				CurrentPartyData =
					JsonConvert.DeserializeObject<PartyData>(partyData.GetRawJsonValue());
				if (CurrentPartyData.party_ServerName == sceneName) isReady = true;
			}

			// 현재 파티 데이터의 룸 이름대로 이동
			ServerManager.LeaveAndLoadScene(sceneName);
		}
		catch (FirebaseException e)
		{
			ExceptionManager.HandleFirebaseException(e);
		}
		catch (Exception e)
		{
			ExceptionManager.HandleException(e);
		}
	}

	public async Task RemovePartyMemberFromServer(string partyId)
	{
		try
		{
			print($"삭제하려는 파티id: {CurrentPartyData.party_Id}");
			DataSnapshot partySnapshot =
				await DB.GetReference($"parties/{CurrentPartyData.party_Id}")
					.GetValueAsync();
			PartyData targetParty =
				JsonConvert.DeserializeObject<PartyData>(
					partySnapshot.GetRawJsonValue());

			await DB.GetReference($"parties/{CurrentPartyData.party_Id}")
				.RemoveValueAsync();
			if (targetParty.party_Owner.user_Id != partyId)
			{
				// 멤버 제거
				targetParty.party_Members.RemoveAll(member =>
					member.user_Id == partyId);

				// 업데이트된 파티 데이터 업로드
				string updatedPartyDataJson = JsonConvert.SerializeObject(targetParty);
				await DB.GetReference($"parties/{CurrentPartyData.party_Id}")
					.SetRawJsonValueAsync(updatedPartyDataJson);
			}
		}
		catch (FirebaseException e)
		{
			ExceptionManager.HandleFirebaseException(e);
		}
		catch (Exception e)
		{
			ExceptionManager.HandleException(e);
		}
	}

	private async Task UpdatePartyList()
	{
		try
		{
			DataSnapshot partiesSnapshot =
				await DB.GetReference("parties").GetValueAsync();
			partyDictionary =
				JsonConvert.DeserializeObject<Dictionary<string, PartyData>>(
					partiesSnapshot.GetRawJsonValue());

			if (partyDictionary == null)
			{
				print("파티가 존재하지 않습니다.");
			}
			else
			{
				partyList = new List<PartyData>(partyDictionary.Values);
			}
		}
		catch (FirebaseException e)
		{
			ExceptionManager.HandleFirebaseException(e);
		}
		catch (Exception e)
		{
			ExceptionManager.HandleException(e);
		}
	}

	public List<PartyData> GetPartyList()
	{
		return partyList;
	}

	public List<UserData> GetCurrentPartyMembers()
	{
		return CurrentPartyData.party_Members;
	}

	#endregion
}