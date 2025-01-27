using Photon.Pun;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerInputManager : MonoBehaviourPun
{
	public static System.Action<Vector2> OnMoveInput;
	public static System.Action OnAttackInput;
	public static System.Action OnSkillInput;
	public static System.Action OnRightClickInput;
	public static System.Action OnShiftLeftClickInput;
	public static System.Action OnShiftRightClickInput;
	public static System.Action OnBInput;
	public static System.Action OnKInput;

	// PC에서 모바일 입력 테스트를 위해 모바일 입력 활성화
	[Header("PC에서 모바일 테스트 실행 여부")] public bool forceMobile = false;

	// 스크립트 자동 참조
	private Player player;
	private VirtualJoystick virtualJoystick;

	// 모바일 플랫폼 여부
	private bool useMobileInput;

	private GameObject shop; // 상점 오브젝트

	private void Awake()
	{
		// Player 스크립트 자동 참조 -> PlayerInputManager 스크립트가 속한 부모 객체에서 찾음
		player = GetComponent<Player>();

		shop = GameObject.Find("Store");
		print(shop);

		// 모바일 입력 활성화 여부 설정 (모바일 테스트 true 또는 모바일 플랫폼 감지)
		useMobileInput = forceMobile || Application.isMobilePlatform;


		// forceMobile = true면 실행
		if (forceMobile)
		{
			// VirtualJoystick 스크립트 자동 참조 -> 씬 전체에서 찾음
			if (virtualJoystick == null)
			{
				virtualJoystick = FindObjectOfType<VirtualJoystick>();

				if (virtualJoystick == null)
				{
					Debug.LogError("[PlayerInputManager] 씬에 VirtualJoystick이 없습니다.");
				}
			}
		}

		// 글자가 검은색인 이유: 현재 플랫폼이 모바일이 아니라서
		// 모바일 플랫폼에서 활성화
#if UNITY_IOS || UNITY_ANDROID
        if (useMobileInput)
        {
            if (virtualJoystick == null)
            {
                virtualJoystick = FindObjectOfType<VirtualJoystick>();
                if (virtualJoystick == null)
                {
                    Debug.LogError("씬에 VirtualJoystick이 없습니다. 인스펙터에서 할당하세요.");
                }
            }
        }
#endif
	}

	void Update()
	{
		if (!photonView.IsMine) return;

		if (useMobileInput)
		{
			HandleMobileInputs();
		}
		else
		{
			HandleDesktopInputs();
		}
	}

	// 컴퓨터 입력 
	private void HandleDesktopInputs()
	{
		Vector2 movement = new Vector2(Input.GetAxisRaw("Horizontal"),
			Input.GetAxisRaw("Vertical"));

		// 항상 호출하여 Idle 상태 전환 가능하게 함
		OnMoveInput?.Invoke(movement);

		if (Input.GetMouseButtonDown(0))
		{
			if (Input.GetKey(KeyCode.LeftShift))
			{
				OnShiftLeftClickInput?.Invoke();
			}
			else
			{
				OnAttackInput?.Invoke();
			}
		}

		if (Input.GetMouseButtonDown(1))
		{
			if (Input.GetKey(KeyCode.LeftShift))
			{
				OnShiftRightClickInput?.Invoke();
			}
			else
			{
				OnRightClickInput?.Invoke();
			}
		}

		if (Input.GetButtonDown("Jump"))
		{
			OnSkillInput?.Invoke();
		}

		if (Input.GetKeyDown(KeyCode.B))
		{
			OnBInput?.Invoke();
		}

		if (Input.GetKeyDown(KeyCode.K))
		{
			OnKInput?.Invoke();
		}

		if (Input.GetKeyDown(KeyCode.I))
		{
			UIManager.Instance.inventory.TryOpenInventory();
		}

		if (Input.GetKeyDown(KeyCode.E))
		{
			if (player.droppedItems.Count > 0)
			{
				for (int i = player.droppedItems.Count - 1; i >= 0; i--)
				{
					if (player.droppedItems[i].droppedItem == null ||
					    player.droppedItems[i].droppedLightLine == null)
					{
						player.droppedItems.RemoveAt(i);
						continue;
					}

					if (Vector3.Distance(transform.position,
						    player.droppedItems[i].droppedLightLine.transform.position) <
					    3f)
					{
						if (player.droppedItems[i].droppedItem != null &&
						    player.droppedItems[i].droppedLightLine != null)
						{
							InventoryManger.Instance.AddItemToInventory(player.droppedItems[i]
								.droppedItem);
							Destroy(player.droppedItems[i].droppedLightLine);
						}
					}
				}
			}

			if(Vector3.Distance(transform.position, shop.transform.position) < 7f)
			{
				UIManager.Instance.shop.TryOpenShop();
			}
		}

		if (Input.GetKeyDown(KeyCode.Z))
		{
			UIManager.Instance.OpenDungeonPanel();
		}

		if (Input.GetKeyDown(KeyCode.Escape))
		{
			// if (UIManager.Instance.currentPanel)
			// {
			//     UIManager.Instance.currentPanel.SetActive(false);
			//     UIManager.Instance.currentPanel = null;
			//     UIManager.Instance.isInventoryActive = false;
			//     UIManager.Instance.isShopActive = false;
			//     UIManager.Instance.isOptionActive = false;
			//     UIManager.Instance.isItemInfoActive = false;
			// }
			// else
			// {
			//     UIManager.Instance.OpenOptionPanel();    
			// }
			if (UIManager.Instance.IsAnyPanelOpen())
				UIManager.Instance.CloseAllPanels();
			else UIManager.Instance.OpenOptionPanel();
		}
	}

	// 모바일 입력 
	private void HandleMobileInputs()
	{
		// 조이스틱을 통한 이동 처리
		if (virtualJoystick != null)
		{
			Vector2 mobileMovement = virtualJoystick.InputVector;

			// 항상 호출하여 Idle 상태 전환 가능하게 함
			OnMoveInput?.Invoke(mobileMovement);
		}

		// 터치 입력을 통한 공격 처리
		if (Input.touchCount > 0)
		{
			foreach (Touch touch in Input.touches)
			{
				// 터치가 시작될 때
				if (touch.phase == TouchPhase.Began)
				{
					// 터치가 UI 위에 있는지 확인
					if (!IsPointerOverUIObject(touch))
					{
						// 빈 공간 터치 시 공격
						OnAttackInput?.Invoke();
					}
				}

				// 필요시 다른 터치 단계를 처리할 수 있습니다.
			}
		}

		// 스킬 및 기타 액션은 UI 버튼을 통해 처리
		// UI 버튼이 적절히 액션을 호출하도록 설정되어야 합니다.
	}

	// 터치가 UI 요소 위에 있는지 확인하는 유틸리티 함수
	private bool IsPointerOverUIObject(Touch touch)
	{
		// 현재 터치 위치를 기반으로 PointerEventData 생성
		PointerEventData eventData = new PointerEventData(EventSystem.current);
		eventData.position = touch.position;

		// UI 요소와의 충돌 여부를 확인
		var results = new System.Collections.Generic.List<RaycastResult>();
		EventSystem.current.RaycastAll(eventData, results);
		return results.Count > 0;
	}
}