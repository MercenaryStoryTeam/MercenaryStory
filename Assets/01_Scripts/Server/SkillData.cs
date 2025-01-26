using System;

[Serializable]
public class SkillData
{
	public int skill_Id { get; set; }
	public int skill_Level { get; set; }

	public SkillData()
	{
	}

	public SkillData(int id, int level)
	{
		skill_Id = id;
		skill_Level = level;
	}
}