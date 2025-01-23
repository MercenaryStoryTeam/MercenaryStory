using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTestSY : MonoBehaviour
{
	private float interactRange = 3f;
	private MonsterTest monster;

	private List<(GameObject droppedLightLine, ItemBase droppedItem)> droppedItems =
		new List<(GameObject droppedLightLine, ItemBase droppedItem)>();

	private void Awake()
	{
		monster = FindObjectOfType<MonsterTest>();
	}

	private void Update()
	{
		if (droppedItems.Count > 0)
		{
			for (int i = droppedItems.Count - 1; i >= 0; i--)
			{
				if (droppedItems[i].droppedItem == null ||
				    droppedItems[i].droppedLightLine == null)
				{
					droppedItems.RemoveAt(i);
					continue;
				}

				if (Vector3.Distance(transform.position,
					    droppedItems[i].droppedLightLine.transform.position) <
				    interactRange)
				{
					if (Input.GetKeyDown(KeyCode.E)) // E키 누르면 반경 안에 있는 아이템 인벤토리로 들어감. 테스트용 키임
					{
						if (droppedItems[i].droppedItem != null &&
						    droppedItems[i].droppedLightLine != null)
						{
							bool isDropped = InventoryManger.Instance.UpdateSlotData();
							if (isDropped)
							{
								InventoryManger.Instance.AddItemToInventory(droppedItems[i].droppedItem);
								Destroy(droppedItems[i].droppedLightLine);
								droppedItems.RemoveAt(i);
							}
						}
					}
				}
			}
		}
	}

	public ItemBase TryDropItems(List<ItemBase> items)
	{
		ItemBase droppedItem = null;

		foreach (ItemBase item in items)
		{
			float randomValue = UnityEngine.Random.Range(0f, 1f);
			if (randomValue <= item.dropPercent)
			{
				droppedItem = item;
			}
		}

		return droppedItem;
	}

	public void InvenSceneTestDrop(ItemBase item)
	{
		if (item == null)
		{
			Debug.LogError("드롭할 아이템이 null입니다!");
			return;
		}

		if (item.dropLightLine == null)
		{
			Debug.LogError($"아이템 {item.itemName}의 dropLightLine 프리팹이 할당되지 않았습니다!");
			return;
		}

		GameObject itemLightLine = Instantiate(item.dropLightLine, this.transform.position,
			Quaternion.identity);
		droppedItems.Add((itemLightLine, item));
	}

	public void TestDrop(ItemBase item)
	{
		GameObject itemLightLine = Instantiate(item.dropLightLine, monster.transform.position,
			Quaternion.identity);
		droppedItems.Add((itemLightLine, item));
	}
}