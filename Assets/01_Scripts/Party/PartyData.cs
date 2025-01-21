using System;
using System.Collections.Generic;

[Serializable]
public class PartyData
{
	public string party_Id { get; set; }
	public string party_ServerId { get; set; }
	public string party_ServerName { get; set; }
	public string party_Name { get; set; }
	public int party_size { get; set; }
	public UserData party_Owner { get; set; }
	public List<UserData> party_Members { get; set; }

	public PartyData()
	{
	}

	// CreateParty
	public PartyData(string serverId, string serverName, string name, int size,
		UserData owner)
	{
		party_Id = Guid.NewGuid().ToString();
		party_ServerId = serverId;
		party_ServerName = serverName;
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

	public void UpdatePartyData(
		string id = null,
		string serverId = null,
		string serverName = null,
		string name = null,
		int? size = null,
		UserData owner = null,
		List<UserData> members = null
	)
	{
		if (!string.IsNullOrEmpty(id)) party_Id = id;
		if (!string.IsNullOrEmpty(serverId)) party_ServerId = serverId;
		if (!string.IsNullOrEmpty(serverName)) party_ServerName = serverName;
		if (!string.IsNullOrEmpty(name)) party_Name = name;
		if (size.HasValue) party_size = size.Value;
		if (owner != null) party_Owner = owner;
		if (members != null) party_Members = members;
	}
}