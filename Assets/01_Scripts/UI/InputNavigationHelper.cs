using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InputNavigationHelper : MonoBehaviour
{
	public Selectable[] navigationElements; // InputField와 Button을 모두 포함
	private int currentIndex = 0;
	public KeyCode shiftNextSelectableKey = KeyCode.Tab;
	public KeyCode selectCurrentSelectableKey = KeyCode.Return;

	private void Awake()
	{
		if (navigationElements.Length > 0)
		{
			SelectElement(currentIndex);
		}
	}

	private void Update()
	{
		HandleTabNavigation();
		HandleEnterAction();
	}

	private void HandleTabNavigation()
	{
		if (Input.GetKeyDown(shiftNextSelectableKey))
		{
			// Shift + Tab 시 이전으로 이동
			int direction = Input.GetKey(KeyCode.LeftShift) ||
			                Input.GetKey(KeyCode.RightShift)
				? -1
				: 1;
			currentIndex = (currentIndex + direction + navigationElements.Length) %
			               navigationElements.Length;
			SelectElement(currentIndex);
		}
	}

	private void HandleEnterAction()
	{
		if (Input.GetKeyDown(selectCurrentSelectableKey))
		{
			// 현재 선택된 Selectable이 Button이면 클릭
			var currentSelectable = navigationElements[currentIndex];
			if (currentSelectable is Button button)
			{
				button.onClick.Invoke();
			}
		}
	}

	private void SelectElement(int index)
	{
		var selectable = navigationElements[index];
		selectable.Select();

		// For better accessibility: highlight the selected button visually
		var eventSystem = EventSystem.current;
		if (eventSystem != null)
		{
			eventSystem.SetSelectedGameObject(selectable.gameObject);
		}
	}
}