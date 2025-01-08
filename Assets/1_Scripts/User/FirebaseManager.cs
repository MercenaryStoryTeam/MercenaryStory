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

	public async void SignUp(string email, string password, Action<FirebaseUser, UserData> callback = null)
	{
		try
		{
			var result = await Auth.CreateUserWithEmailAndPasswordAsync(email, password);

			usersRef = DB.GetReference($"users/{result.User.UserId}");
			UserData userData = new UserData(email, result.User.UserId);
			string userDataJson = JsonConvert.SerializeObject(userData);
			await usersRef.SetRawJsonValueAsync(userDataJson);
			callback?.Invoke(result.User, userData);

			PanelManager.Instance.popUpPanel.DialogOpen("회원가입이 완료되었습니다.", () => PanelManager.Instance.PanelOpen("SignIn"));
		}
		catch (FirebaseException e)
		{
			Debug.LogError(e.Message);
		}
	}

	public async void CheckEmail(string email)
	{
		DataSnapshot usersData = await DB.GetReference("users").GetValueAsync();
		userDictionary = JsonConvert.DeserializeObject<Dictionary<string, UserData>>(usersData.GetRawJsonValue());
		if (userDictionary != null)
		{
			userList = new List<UserData>(userDictionary.Values);
			foreach (UserData userData in userList)
			{
				if (userData.email == email)
				{
					PanelManager.Instance.popUpPanel.DialogOpen("이미 사용중인 email입니다.",
						() => PanelManager.Instance.popUpPanel.DialogClose());
					state = State.EmailNotChecked;
					return;
				}
			}
		}

		PanelManager.Instance.popUpPanel.DialogOpen("사용 가능한 email입니다.",
			() => PanelManager.Instance.popUpPanel.DialogClose());
		state = State.EmailChecked;
	}

	public async void SignIn(string email, string password, Action<FirebaseUser, UserData> callback = null)
	{
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
		callback?.Invoke(result.User, userData);

		PanelManager.Instance.PanelOpen("CharacterSelect");
	}
}