using System;
using Firebase;
using UnityEngine;

public class ExceptionManager
{
	public static void HandleFirebaseException(FirebaseException e)
	{
		switch (e.ErrorCode)
		{
			case 37:
			case 38:
				ShowPopup("이메일과 비밀번호를 입력해 주세요");
				break;
			case 1:
				ShowPopup("이메일과 비밀번호가 일치하지 않거나\n존재하지 않는 이메일입니다.");
				break;
			case 11:
				ShowPopup("이메일의 형식이 아닙니다.");
				break;
			default:
				ShowPopup(e.Message);
				break;
		}
	}

	public static void HandleException(Exception e)
	{
		ShowPopup(e.Message);
	}

	private static void ShowPopup(string message)
	{
		UIManager.Instance.popUp.PopUpOpen(message,
			() => UIManager.Instance.popUp.PopUpClose());
		Debug.LogWarning(message);
	}
}