using System;
using System.Collections.Generic;

[Serializable]
public class UserData
{
	public string user_Id { get; set; }
	public string user_Email { get; set; }
	public string user_Name { get; set; }
	public int user_Appearance { get; set; }
	public string user_CurrentServer { get; set; }
	public string user_CurrentParty { get; set; }
	public int user_Rank { get; set; }
	public float user_RankCurrentEXP { get; set; }
	public float user_HP { get; set; }
	public int user_weapon_item_Id { get; set; }
	public List<SlotData> user_Inventory { get; set; } = new List<SlotData>();
	public float user_Gold { get; set; }
	public bool user_IsOnline { get; set; }

	public List<SkillData> user_Skills { get; set; } = new List<SkillData>();

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
		user_CurrentServer = "";
		user_CurrentParty = "";
		user_Rank = 1;
		user_RankCurrentEXP = 0;
		user_HP = 100;
		user_weapon_item_Id = 32000;
		user_Inventory = new List<SlotData>();
		user_IsOnline = false;
		user_Skills = new List<SkillData>();
	}

	// 사용 예시 : userData.UpdateUserData(rank: 2, rankCurrentExp: 50.0f, hp: 80.0f);
	public void UpdateUserData(
		int? appearance = null,
		string currentServer = null,
		string currentParty = null,
		int? rank = null,
		float? rankCurrentExp = null,
		float? hp = null,
		int? weaponItemId = null,
		List<SlotData> inventory = null,
		float? gold = null,
		bool? isOnline = null,
		List<SkillData> skills = null
	)
	{
		if (appearance.HasValue) user_Appearance = appearance.Value;
		if (!string.IsNullOrEmpty(currentServer))
			user_CurrentServer = currentServer;
		if (!string.IsNullOrEmpty(currentParty)) user_CurrentParty = currentParty;
		if (rank.HasValue) user_Rank = rank.Value;
		if (rankCurrentExp.HasValue) user_RankCurrentEXP = rankCurrentExp.Value;
		if (hp.HasValue) user_HP = hp.Value;
		if (weaponItemId.HasValue) user_weapon_item_Id = weaponItemId.Value;
		if (inventory != null) user_Inventory = inventory;
		if (gold.HasValue) user_Gold = gold.Value;
		if (isOnline.HasValue) user_IsOnline = isOnline.Value;
		if (skills != null) user_Skills = skills;
	}
}