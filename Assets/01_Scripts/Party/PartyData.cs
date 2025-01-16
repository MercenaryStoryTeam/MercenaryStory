using System;
using System.Collections.Generic;

[Serializable]
public class PartyData
{
	public string party_Id { get; set; }
	public string party_Server { get; set; }
	public string party_Name { get; set; }
	public int party_size { get; set; }
	public UserData party_Owner { get; set; }
	public List<UserData> party_Members { get; set; }

	public PartyData()
	{
	}

	// CreateParty
	public PartyData(string server, string name, int size, UserData owner)
	{
		party_Id = Guid.NewGuid().ToString();
		party_Server = server;
		party_Name = name;
		party_size = size;
		party_Owner = owner;
		party_Members = new List<UserData>();
		party_Members.Add(owner);
	}

	// AddMember
	public void AddMember(UserData userdata)
	{
		party_Members.Add(userdata);
	}
}