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
			UIManager.Instance.popUp.PopUpOpen($"오류 발생.\n다시 시도해 주세요.\n{e.Message}",
				() => UIManager.Instance.popUp.PopUpClose());
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
			UIManager.Instance.popUp.PopUpOpen($"오류 발생.\n다시 시도해 주세요.\n{e.Message}",
				() => UIManager.Instance.popUp.PopUpClose());
		}
	}

	public async void UpdateCurrentUserData(string childName, object value,
		Action<object> callback = null)
	{
		DatabaseReference targetRef = usersRef.Child(childName);
		await targetRef.SetValueAsync(value);
		callback?.Invoke(value);
	}

	#endregion

	#region Party Management

	public async void CreateParty(string party_Name, int party_Size)
	{
		try
		{
			partiesRef = DB.GetReference($"parties/{party_Name}");
			PartyData partyData = new PartyData(CurrentUserData.user_CurrentServer, party_Name,
				party_Size, CurrentUserData);
			string partyDataJson = JsonConvert.SerializeObject(partyData);
			CurrentPartyData = partyData;
			await partiesRef.SetRawJsonValueAsync(partyDataJson);

			DatabaseReference targetRef = usersRef.Child("users_CurrentParty");
			await targetRef.SetValueAsync(party_Name);

			UIManager.Instance.popUp.PopUpOpen("파티가 생성되었습니다.", () =>
			{
				UIManager.Instance.popUp.PopUpClose();
				// Update party list
				UIManager.Instance.ClosePartyCreatePanel();
			});
		}
		catch (FirebaseException e)
		{
			ExceptionManager.HandleFirebaseException(e);
		}
		catch (Exception e)
		{
			UIManager.Instance.popUp.PopUpOpen($"오류 발생.\n다시 시도해 주세요.\n{e.Message}",
				() => UIManager.Instance.popUp.PopUpClose());
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

			partyList = new List<PartyData>(partyDictionary.Values);
		}
		catch (FirebaseException e)
		{
			ExceptionManager.HandleFirebaseException(e);
		}
		catch (Exception e)
		{
			UIManager.Instance.popUp.PopUpOpen($"오류 발생.\n다시 시도해 주세요.\n{e.Message}",
				() => UIManager.Instance.popUp.PopUpClose());
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
			UIManager.Instance.popUp.PopUpOpen($"오류 발생.\n다시 시도해 주세요.\n{e.Message}",
				() => UIManager.Instance.popUp.PopUpClose());
		}
	}

	public async void ExitParty()
	{
	}

	public List<PartyData> GetPartyList()
	{
		return partyList;
	}

	#endregion
}