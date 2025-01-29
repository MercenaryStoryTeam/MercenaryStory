using Photon.Pun;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 플레이어의 입력을 관리하는 스크립트
/// </summary>
public class PlayerInputManager : MonoBehaviourPun
{
	public static System.Action<Vector2> OnMoveInput;
	public static System.Action OnAttackInput;
	public static System.Action OnSkillInput;
	public static System.Action OnRightClickInput;
	public static System.Action OnShiftLeftClickInput;
	public static System.Action OnShiftRightClickInput;

	// PC에서 모바일 입력 테스트를 위해 모바일 입력 활성화
	[Header("PC에서 모바일 테스트 실행 여부")] public bool forceMobile = false;

	// 스크립트 자동 참조
	private Player player;
	private VirtualJoystick virtualJoystick;

	// 모바일 플랫폼 여부
	private bool useMobileInput;

	// SkillUIManager 참조 추가
	private SkillUIManager skillUIManager;

	private Button attackButton;

	private void Awake()
	{
		// Player 스크립트 자동 참조 -> PlayerInputManager 스크립트가 속한 부모 객체에서 찾음
		player = GetComponent<Player>();

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

			GameObject canvas = GameObject.Find("MainCanvas");
			attackButton = canvas.transform.FindDeepChild("BasicAttackButton")
				.GetComponent<Button>();

			attackButton.onClick.AddListener(AttackButtonClicked);
		}

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

		// SkillUIManager 자동 참조
		skillUIManager = FindObjectOfType<SkillUIManager>();
		if (skillUIManager == null)
		{
			Debug.LogError("[PlayerInputManager] 씬에 SkillUIManager가 없습니다.");
		}
	}

	void Update()
	{
		if (!photonView.IsMine) return;
		if (!UIManager.Instance.IsAnyPanelOpen() &&
		    !UIManager.Instance.IsPopUpOpen())
		{
			if (useMobileInput)
			{
				HandleMobileInputs();
			}
			else
			{
				HandleDesktopInputs();
			}
		}

		// ESC 키 처리
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			HandleEscapeKey();
		}
	}

	/// <summary>
	/// ESC 키 처리 메서드
	/// </summary>
	private void HandleEscapeKey()
	{
		if (UIManager.Instance.IsAnyPanelOpen())
		{
			UIManager.Instance.CloseAllPanels();
		}
		else
		{
			UIManager.Instance.OpenOptionPanel();
		}
	}

	// 컴퓨터 입력 
	private void HandleDesktopInputs()
	{
		if (UIManager.Instance.IsAnyPanelOpen() ||
		    UIManager.Instance.IsPopUpOpen()) return;
		Vector2 movement = new Vector2(Input.GetAxisRaw("Horizontal"),
			Input.GetAxisRaw("Vertical"));

		// 항상 호출하여 Idle 상태 전환 가능하게 함
		OnMoveInput?.Invoke(movement);

		if (Input.GetMouseButtonDown(0))
		{
			if (EventSystem.current.IsPointerOverGameObject()) return;
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
			if (EventSystem.current.IsPointerOverGameObject()) return;
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

		if (Input.GetKeyDown(KeyCode.I))
		{
			UIManager.Instance.inventory.TryOpenInventory();
		}

		if (Input.GetKeyDown(KeyCode.Z))
		{
			UIManager.Instance.OpenDungeonPanel();
		}
	}

	// 모바일 입력 
	private void HandleMobileInputs()
	{
		if (!photonView.IsMine) return;

		// 조이스틱을 통한 이동 처리
		if (virtualJoystick != null)
		{
			Vector2 mobileMovement = virtualJoystick.InputVector;

			// 항상 호출하여 Idle 상태 전환 가능하게 함
			OnMoveInput?.Invoke(mobileMovement);
		}
	}

	private void AttackButtonClicked()
	{
		OnAttackInput?.Invoke();
	}

	/// <summary>
	/// 터치가 UI 요소 위에 있는지 확인하는 유틸리티 함수
	/// </summary>
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