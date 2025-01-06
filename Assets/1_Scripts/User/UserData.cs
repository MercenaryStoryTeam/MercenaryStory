using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UserData
{
	public string email;
	public string userId { get; set; }
	public string userName;
	public int gold;

	public UserData()
	{
	}

	public UserData(string email, string userId)
	{
		this.email = email;
		this.userId = userId;
		userName = "무명의 전사";
		gold = 0;
	}

	public UserData(string email, string userId, string userName, int gold)
	{
		this.email = email;
		this.userId = userId;
		this.userName = userName;
		this.gold = gold;
	}
}