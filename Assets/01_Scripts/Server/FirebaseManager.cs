using System;
using System.Collections.Generic;
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

	private DatabaseReference usersRef;
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
		}
		else
		{
			Debug.LogWarning($"Firebase initialization failed: {status}");
		}
	}

	#region User Management

	public async void SignUp(string email, string password, string user_Name,
		Action<FirebaseUser, UserData> callback = null)
	{
		try
		{
			var result = await Auth.CreateUserWithEmailAndPasswordAsync(email, password);

			usersRef = DB.GetReference($"users/{result.User.UserId}");
			UserData userData = new UserData(result.User.UserId, email, user_Name);
				userData.user_Inventory.Add(InventoryManger.Instance.SetBasicItem(InventoryManger.Instance.basicWeapon, 
				InventoryManger.Instance.basicEquipWeapon));
			
			string userDataJson = JsonConvert.SerializeObject(userData);
			await usersRef.SetRawJsonValueAsync(userDataJson);
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

	public async void CheckEmail(string email)
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

	public async void SignIn(string email, string password)
	{
		try
		{
			UIManager.Instance.popUp.WaitPopUpOpen("로그인 중입니다.");
			var result = await Auth.SignInWithEmailAndPasswordAsync(email, password);
			usersRef = DB.GetReference($"users/{result.User.UserId}");
			DataSnapshot userDataValues = await usersRef.GetValueAsync();
			UserData userData = null;
			if (userDataValues.Exists)
			{
				string json = userDataValues.GetRawJsonValue();
				userData = JsonConvert.DeserializeObject<UserData>(json);
			}

			CurrentUserData = userData;

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

	public async void UploadCurrentUserData(string childName, object value,
		Action<object> callback = null)
	{
		try
		{
			DatabaseReference targetRef = usersRef.Child(childName);
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

	#endregion

	#region Party Management

	public async void CreateParty(string party_Name, int party_Size)
	{
		try
		{
			// Create PartyData
			PartyData partyData = new PartyData(CurrentUserData.user_CurrentServer, party_Name,
				party_Size, CurrentUserData);
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
				UIManager.Instance.ClosePartyCreatePanel();
				UIManager.Instance.OpenPartyPanel();
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

	public async void UpdateParty()
	{
		try
		{
			DataSnapshot partiesData = await DB.GetReference("parties").GetValueAsync();
			partyDictionary =
				JsonConvert.DeserializeObject<Dictionary<string, PartyData>>(
					partiesData.GetRawJsonValue());
			if (partyDictionary != null)
			{
				partyList = new List<PartyData>(partyDictionary.Values);
			}
			else
			{
				print("파티 없음");
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

	public async void JoinParty(string name)
	{
		try
		{
			DataSnapshot partiesData = await DB.GetReference("parties").GetValueAsync();
			partyDictionary =
				JsonConvert.DeserializeObject<Dictionary<string, PartyData>>(
					partiesData.GetRawJsonValue());

			if (partyDictionary == null)
			{
				UIManager.Instance.popUp.PopUpOpen("파티가 존재하지 않습니다.\n새로고침 해주세요.",
					() => UIManager.Instance.popUp.PopUpClose());
			}
			else
			{
				partyList = new List<PartyData>(partyDictionary.Values);
				foreach (PartyData partyData in partyList)
				{
					if (partyData.party_Name == name)
					{
						if (partyData.party_Members.Count < partyData.party_size)
						{
							// 가입
							partyData.party_Members.Add(CurrentUserData);
							CurrentPartyData = partyData;
							CurrentPartyData.AddMember(CurrentUserData);
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

	public async void ExitParty()
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
					UIManager.Instance.OpenPartyPanel();
				});
				CurrentUserData.user_CurrentParty = "";
				await usersRef.Child(CurrentUserData.user_Id).Child("user_CurrentParty")
					.SetValueAsync("");
				return;
			}

			// 파티 데이터 역직렬화
			PartyData partyData =
				JsonConvert.DeserializeObject<PartyData>(partySnapshot.GetRawJsonValue());

			// 파티장이 나가는 경우
			if (partyData.party_Owner.user_Id == CurrentUserData.user_Id)
			{
				Debug.Log("파티장이 파티를 나갑니다.");

				// 모든 멤버의 user_CurrentParty 초기화
				foreach (UserData member in partyData.party_Members)
				{
					DatabaseReference memberRef = DB.GetReference($"users/{member.user_Id}");
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
				await usersRef.Child(CurrentUserData.user_Id).Child("user_CurrentParty")
					.SetValueAsync("");
			}

			// 로컬 데이터 초기화
			CurrentUserData.user_CurrentParty = "";
			CurrentPartyData = null;

			UIManager.Instance.popUp.PopUpOpen("파티에서 나갔습니다.", () =>
			{
				UIManager.Instance.popUp.PopUpClose();
				UIManager.Instance.OpenPartyPanel();
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

	public List<PartyData> GetPartyList()
	{
		return partyList;
	}

	#endregion
}