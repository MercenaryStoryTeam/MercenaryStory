using System;
using System.Collections;
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
	public UserData CurrentUserData { get; private set; }

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

			PanelManager.Instance.popUp.PopUpOpen("회원가입이 완료되었습니다.", () =>
			{
				PanelManager.Instance.popUp.PopUpClose();
				PanelManager.Instance.PanelOpen("SignIn");
			});
		}
		catch (FirebaseException e)
		{
			Debug.LogError(e.Message);
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
					PanelManager.Instance.popUp.PopUpOpen("이미 사용중인 email입니다.",
						() => PanelManager.Instance.popUp.PopUpClose());
					state = State.EmailNotChecked;
					return;
				}
			}
		}

		PanelManager.Instance.popUp.PopUpOpen("사용 가능한 email입니다.",
			() => PanelManager.Instance.popUp.PopUpClose());
		state = State.EmailChecked;
	}

	public async void SignIn(string email, string password)
	{
		try
		{
			PanelManager.Instance.popUp.WaitPopUpOpen("로그인 중입니다.");
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

			PanelManager.Instance.popUp.PopUpClose();
			ServerManager.ConnectLobby();
			if (CurrentUserData.user_Appearance == 0)
			{
				PanelManager.Instance.PanelOpen("CharacterSelect");
			}
			else
			{
				PanelManager.Instance.PanelOpen("ServerSelect");
			}
		}
		catch (FirebaseException e)
		{
			switch (e.ErrorCode)
			{
				case 37:
				case 38:
					PanelManager.Instance.popUp.PopUpOpen("이메일과 비밀번호를 입력해 주세요",
						() => PanelManager.Instance.popUp.PopUpClose());
					break;
				case 1:
					PanelManager.Instance.popUp.PopUpOpen("이메일과 비밀번호가 일치하지 않거나\n존재하지 않는 이메일입니다.",
						() => PanelManager.Instance.popUp.PopUpClose());
					break;
				case 11:
					PanelManager.Instance.popUp.PopUpOpen("이메일의 형식이 아닙니다.",
						() => PanelManager.Instance.popUp.PopUpClose());
					break;
				default:
					PanelManager.Instance.popUp.PopUpOpen(e.Message,
						() => PanelManager.Instance.popUp.PopUpClose());
					break;
			}
		}
	}

	public async void UpdateCurrentUserData(string childName, object value,
		Action<object> callback = null)
	{
		DatabaseReference targetRef = usersRef.Child(childName);
		await targetRef.SetValueAsync(value);
		callback?.Invoke(value);
	}
}