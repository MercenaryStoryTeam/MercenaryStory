using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UserData
{
	public string user_Id { get; set; }
	public string user_Email { get; set; }
	public string user_Name { get; set; }
	public int user_Appearance { get; set; }
	public int user_Rank { get; set; }
	public float user_RankCurrentEXP { get; set; }
	public float user_HP { get; set; }
	public int user_weapon_item_Id { get; set; }
	public List<ItemData> user_Inventory { get; set; }
	public float user_Gold { get; set; }


	public UserData()
	{
	}

	// SignUp
	public UserData(string id, string email, string name)
	{
		user_Id = id;
		user_Email = email;
		user_Name = name;
		user_Appearance = 0;
		user_Rank = 1;
		user_RankCurrentEXP = 0;
		user_HP = 100;
		user_weapon_item_Id = 31000;
		user_Inventory = new List<ItemData>();
	}

	// SignIn
	public UserData(string id, string email, string name, int appearance, int rank, float rankCurrentExp, float hp,
		int weaponItemId, List<ItemData> inventory, float gold)
	{
		user_Id = id;
		user_Email = email;
		user_Name = name;
		user_Appearance = appearance;
		user_Rank = rank;
		user_RankCurrentEXP = rankCurrentExp;
		user_HP = hp;
		user_weapon_item_Id = weaponItemId;
		user_Inventory = inventory;
		user_Gold = gold;
	}

	// 사용 예시 : userData.UpdateUserData(rank: 2, rankCurrentExp: 50.0f, hp: 80.0f);
	public void UpdateUserData(
		int? appearance = null,
		int? rank = null,
		float? rankCurrentExp = null,
		float? hp = null,
		int? weaponItemId = null,
		List<ItemData> inventory = null,
		float? gold = null)
	{
		if (appearance.HasValue) user_Appearance = appearance.Value;
		if (rank.HasValue) user_Rank = rank.Value;
		if (rankCurrentExp.HasValue) user_RankCurrentEXP = rankCurrentExp.Value;
		if (hp.HasValue) user_HP = hp.Value;
		if (weaponItemId.HasValue) user_weapon_item_Id = weaponItemId.Value;
		if (inventory != null) user_Inventory = inventory;
		if (gold.HasValue) user_Gold = gold.Value;
	}
}