using System;

[Serializable]
public class SlotData
{
	public int item_Id { get; set; }
	public int item_Stack { get; set; }

	public SlotData()
	{
	}

	public SlotData(int itemId, int stack)
	{
		item_Id = itemId;
		item_Stack = stack;
	}
}