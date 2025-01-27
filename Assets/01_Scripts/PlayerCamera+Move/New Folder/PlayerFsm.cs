using System;
using UnityEngine;
using Photon.Pun;
using Cinemachine;
using System.Collections;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody), typeof(Animator), typeof(Player))]
public class PlayerFsm : MonoBehaviourPun
{
	[HideInInspector] 
	public Transform cameraTransform;

	[Header("Die 애니메이션 재생 시간")] 
	public float dieAnimationDuration = 2f;

	[Header("콤보 타이머")]
	public float comboResetTime = 1.0f;

    [Header("기본 공격 소리 딜레이 조정")]
    public float soundDelay = 0.8f;

    private Rigidbody rb;
	private Animator animator;
	private Vector3 movementInput;
	private const float moveThreshold = 0.05f;

	// 현재 이동 속도
	private float currentSpeed;

	// 상태 유형
	public enum State
	{
		Idle,
		Moving,
		Attack1,
		Attack2,
		Die,
		Hit
	}

	private State currentState = State.Idle;
	private bool isDead = false;

	// 콤보 시스템
	private enum ComboState
	{
		None,
		Attack1,
		Attack2
	}

	private ComboState currentComboState = ComboState.None;
	private float lastAttackTime = 0f;

	// Player 스크립트 참조: 이동 속도, HP 관리 등 플레이어 전반의 데이터를 가져오기 위함
	private Player player;

	// 이동 및 공격 잠금 상태
	private bool isMovementLocked = false;
	private bool isAttackLocked = false;

	// 상태 변경 시 이벤트
	public event Action<FSMManager.PlayerState> OnStateChanged;

	private bool isMobile = false;

	private void Awake()
	{
		InitializeComponents();
		InitializeCamera();
	}

	private void Start()
	{
		if (photonView.IsMine)
		{
			gameObject.AddComponent<AudioListener>();
		}
		if (FirebaseManager.Instance.CurrentUserData.user_Name == gameObject.name)
		{
			GameManager.Instance.currentPlayerFsm = this;
		}

		if (PhotonNetwork.IsMasterClient && GameManager.Instance != null)
		{
			if (FirebaseManager.Instance.CurrentPartyData.party_Owner.user_Name ==
			    gameObject.name)
			{
				GameManager.Instance.hostPlayerFsm = this;
			}
		}
	}

	private void OnEnable()
	{
		UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;

		PlayerInputManager.OnMoveInput += HandleMoveInput;
		PlayerInputManager.OnAttackInput += HandleAttackInput;
	}

	private void OnDisable()
	{
		UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;

		PlayerInputManager.OnMoveInput -= HandleMoveInput;
		PlayerInputManager.OnAttackInput -= HandleAttackInput;
	}

	private void Update()
	{
		// pun 동기화를 위함. 지우지 마시오!! - 지원
		if (!photonView.IsMine) return;

		if (!isDead)
		{
			HandleState();
			HandleComboReset();
		}
	}

	private void FixedUpdate()
	{
		// pun 동기화를 위함. 지우지 마시오!! - 지원
		if (!photonView.IsMine) return;

		HandlePhysics();
	}

	// 컴포넌트 초기화 메서드
	private void InitializeComponents()
	{
		rb = GetComponent<Rigidbody>();
		animator = GetComponent<Animator>();
		player = GetComponent<Player>();

		if (rb == null)
		{
			Debug.LogError("PlayerFsm는 Rigidbody 컴포넌트를 필요로 합니다.");
		}

		if (animator == null)
		{
			Debug.LogError("PlayerFsm는 Animator 컴포넌트를 필요로 합니다.");
		}

		if (player == null)
		{
			Debug.LogError("PlayerFsm의 GameObject에 Player 스크립트가 없습니다.");
			enabled = false;
		}

		if (rb != null)
		{
			if (rb.isKinematic)
			{
				Debug.LogWarning("Rigidbody is Kinematic.");
			}

			if (!rb.useGravity)
			{
				Debug.LogWarning("Rigidbody useGravity가 비활성화되어 있습니다.");
			}
		}

		// 플랫폼 감지
#if UNITY_ANDROID || UNITY_IOS
        isMobile = true;
#else
		isMobile = false;
#endif
	}

	// 카메라 초기화 메서드
	private void InitializeCamera()
	{
		if (cameraTransform == null)
		{
			CinemachineVirtualCamera vCam =
				FindObjectOfType<CinemachineVirtualCamera>();
			if (vCam)
			{
				cameraTransform = vCam.transform;
				VirtualCameraController vCamController =
					vCam.GetComponent<VirtualCameraController>();
				if (vCamController != null)
				{
					vCamController.enabled = true;
				}
			}
			else
			{
				Debug.LogError("씬에 Virtual Camera가 없습니다. 카메라를 수동으로 할당하세요.");
			}
		}

		if (animator != null)
		{
			animator.applyRootMotion = false;
		}
	}

	// 씬 로딩 시 호출되는 메서드
	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		InitializeComponents();
		InitializeCamera();
	}

	// 새로운 HandleMoveInput 메서드
	private void HandleMoveInput(Vector2 movement)
	{
		if (isDead || isMovementLocked) return;

		float inputX = movement.x;
		float inputZ = movement.y;

		movementInput = CalculateMovementDirection(inputX, inputZ);

		currentSpeed = player != null ? player.moveSpeed : 0f;

		if (movementInput.sqrMagnitude > moveThreshold)
		{
			if (currentState != State.Moving)
				TransitionToState(State.Moving);
		}
		else
		{
			if (currentState != State.Idle)
				TransitionToState(State.Idle);
		}

		float normalizedSpeed = (movementInput.magnitude * currentSpeed) /
		                        (player != null ? player.moveSpeed : 1f);
		animator.SetFloat("Speed", normalizedSpeed);
	}

	// 플레이어가 공격을 시도할 때 호출되는 콤보 처리 함수
	private void HandleAttackInput()
	{
		// 사망이거나, 공격 잠금 중이면 공격 불가
		if (isDead || isAttackLocked)
		{
			return;
		}

		// 콤보 타이밍 체크
		if (Time.time - lastAttackTime > comboResetTime)
		{
			// 콤보 타이머 초과 시 콤보 초기화
			currentComboState = ComboState.None;
		}

		lastAttackTime = Time.time;

		// 콤보 단계에 따라 공격 처리
		switch (currentComboState)
		{
			case ComboState.None:
				currentComboState = ComboState.Attack1;
				TransitionToState(State.Attack1);
				break;
			case ComboState.Attack1:
				currentComboState = ComboState.Attack2;
				TransitionToState(State.Attack2);
				break;
			case ComboState.Attack2:
				// 최대 콤보 단계 도달 시 추가 공격 불가 또는 반복
				currentComboState = ComboState.None;
				break;
		}

		// 콤보 타이머 재시작
		StartCoroutine(ComboResetCoroutine());
	}

	// 콤보 타이머를 리셋하는 코루틴
	private IEnumerator ComboResetCoroutine()
	{
		yield return new WaitForSeconds(comboResetTime);
		currentComboState = ComboState.None;
	}

	// 콤보 리셋을 확인하는 함수
	private void HandleComboReset()
	{
		if (Time.time - lastAttackTime > comboResetTime &&
		    currentComboState != ComboState.None)
		{
			currentComboState = ComboState.None;
		}
	}

	private void HandleState()
	{
		switch (currentState)
		{
			case State.Idle:
			case State.Moving:
				break;
			case State.Attack1:
			case State.Attack2:
				break;
			case State.Hit:
				break;
			case State.Die:
				break;
		}
	}

	private void HandlePhysics()
	{
		switch (currentState)
		{
			case State.Idle:
				Vector3 idleVelocity = rb.velocity;
				idleVelocity.x =
					Mathf.Lerp(idleVelocity.x, 0f, Time.fixedDeltaTime * 10f);
				idleVelocity.z =
					Mathf.Lerp(idleVelocity.z, 0f, Time.fixedDeltaTime * 10f);
				rb.velocity = idleVelocity;
				break;
			case State.Moving:
				MovePlayer();
				break;
			case State.Attack1:
			case State.Attack2:
				break;
			case State.Hit:
				break;
			case State.Die:
				rb.velocity = Vector3.zero;
				rb.angularVelocity = Vector3.zero;
				break;
		}
	}

	private void MovePlayer()
	{
		Vector3 moveVelocity = movementInput * currentSpeed;
		rb.velocity = new Vector3(moveVelocity.x, rb.velocity.y, moveVelocity.z);

		if (movementInput.sqrMagnitude > 0.001f)
		{
			Quaternion targetRotation = Quaternion.LookRotation(movementInput);
			rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation,
				Time.fixedDeltaTime * 10f);
		}
	}

	// 상태 관리
	public void TransitionToState(State newState, bool force = false)
	{
		if (isDead && newState != State.Die && !force) return;
		if (currentState == newState) return;

		currentState = newState;

		// FSMManager.PlayerState로 변환
		FSMManager.PlayerState fsmState = ConvertToFSMState(newState);

		// 상태 변경 이벤트 호출
		OnStateChanged?.Invoke(fsmState);

		switch (currentState)
		{
			case State.Idle:
				EnterIdleState();
				break;
			case State.Moving:
				EnterMovingState();
				break;
			case State.Attack1:
			case State.Attack2:
				EnterAttackState((int)(currentState - State.Attack1 + 1));
				break;
			case State.Hit:
				EnterHitState();
				break;
			case State.Die:
				EnterDieState();
				break;
		}
	}

	private FSMManager.PlayerState ConvertToFSMState(State state)
	{
		switch (state)
		{
			case State.Idle:
				return FSMManager.PlayerState.Idle;
			case State.Moving:
				return FSMManager.PlayerState.Moving;
			case State.Attack1:
				return FSMManager.PlayerState.Attack1;
			case State.Attack2:
				return FSMManager.PlayerState.Attack2;
			case State.Hit:
				return FSMManager.PlayerState.Hit;
			case State.Die:
				return FSMManager.PlayerState.Die;
			default:
				return FSMManager.PlayerState.Idle;
		}
	}

	private void EnterIdleState()
	{
		animator.SetFloat("Speed", 0f);
	}

	private void EnterMovingState()
	{
		float speed = movementInput.magnitude * currentSpeed;
		animator.SetFloat("Speed", speed);
	}

	private void EnterAttackState(int attackNumber)
	{
		string[] comboSoundClips = { "player_Twohandattack1", "player_Twohandattack2" };

		float delay = soundDelay;

		StartCoroutine(PlayDelayedRandomSound(comboSoundClips, delay));

		switch (attackNumber)
		{
			case 1:
				animator.SetTrigger("Attack1");
				break;
			case 2:
				animator.SetTrigger("Attack2");
				break;
		}
	}

	private IEnumerator PlayDelayedRandomSound(string[] soundClips, float delay)
	{
		yield return new WaitForSeconds(delay);

		string randomClip =
			soundClips[UnityEngine.Random.Range(0, soundClips.Length)];
		SoundManager.Instance.PlaySFX(randomClip, gameObject);
	}

	private void EnterDieState()
	{
		isDead = true;
		animator.SetTrigger("Die");
		Invoke("DisablePlayer", dieAnimationDuration);
	}

	private void DisablePlayer()
	{
		gameObject.SetActive(false);
	}

	// 피격 상태로 전환
	public void EnterHitState()
	{
		animator.SetTrigger("Hit");
	}

	// 공격을 받았을 때 호출
	public void TakeDamage()
	{
		// 이미 die 상태거나 현재 hit 상태라면, 메서드를 종료
		if (isDead || currentState == State.Hit) return;
		// hit 상태로 전환
		TransitionToState(State.Hit);
	}

	public void Die()
	{
		if (!isDead)
		{
			TransitionToState(State.Die);
		}
	}

	// Attack 애니메이션 종료 이후 처리
	public void OnAttackAnimationEnd()
	{
		if (movementInput.sqrMagnitude > moveThreshold)
			TransitionToState(State.Moving);
		else
			TransitionToState(State.Idle);
	}

	// Hit 애니메이션 종료 이후 처리
	public void OnHitAnimationEnd()
	{
		// 필요 시 구현
	}

	private Vector3 CalculateMovementDirection(float inputX, float inputZ)
	{
		Vector3 camForward = cameraTransform.forward;
		Vector3 camRight = cameraTransform.right;
		camForward.y = 0f;
		camRight.y = 0f;
		camForward.Normalize();
		camRight.Normalize();

		return (camForward * inputZ + camRight * inputX).normalized;
	}

	// 이동 및 공격을 잠금
	public void LockMovementAndAttack()
	{
		isMovementLocked = true;
		isAttackLocked = true;
		animator.SetFloat("Speed", 0f);
		rb.velocity = Vector3.zero;

		// 현재 상태를 Idle로 전환
		TransitionToState(State.Idle);
	}

	// 이동 및 공격을 잠금 해제
	public void UnlockMovementAndAttack()
	{
		isMovementLocked = false;
		isAttackLocked = false;
	}

	// 씬 이동을 위함 -지원
	public void MoveMembersToRoom(string sceneName)
	{
		// 마스터가 아니라 파티장일 때!
		if (FirebaseManager.Instance.CurrentPartyData.party_Owner.user_Id ==
		    FirebaseManager.Instance.CurrentUserData.user_Id)
		{
			// 파티원에게만!
			foreach (UserData member in FirebaseManager.Instance.CurrentPartyData
				         .party_Members)
			{
				// 닉네임이 네임인 플레이어 찾아서 rpc 호출
				foreach (Photon.Realtime.Player photonPlayer in
				         PhotonNetwork.PlayerList)
				{
					if (photonPlayer.NickName ==
					    member.user_Name) // Firebase user_Id와 Photon NickName 매칭
					{
						photonView.RPC("RPC_MoveToScene", photonPlayer, sceneName);
						break;
					}
				}
			}
		}
	}

	[PunRPC]
	private void RPC_MoveToScene(string sceneName)
	{
		// 파티장일 때
		if (FirebaseManager.Instance.CurrentPartyData.party_Owner.user_Id ==
		    FirebaseManager.Instance.CurrentUserData.user_Id)
		{
			// 1. Firebase에 Room 정보 업데이트
			FirebaseManager.Instance.UploadPartyDataToLoadScene(sceneName);
		}

		// 파티 정보 업데이트 (파티장과 파티원 둘 다 서버에서 받아오기)
		FirebaseManager.Instance.UpdateCurrentPartyDataAndLoadScene(sceneName);
	}

    public void InstantiatePlayerPrefabs()
    {
        GameManager.Instance.ChangeScene(GameManager.Instance.CurrentScene+1);
        photonView.RPC("RPC_InstantiatePlayerPrefabs", RpcTarget.Others);
    }

    [PunRPC]
    private void RPC_InstantiatePlayerPrefabs()
    {
        GameManager.Instance.ChangeScene(GameManager.Instance.CurrentScene+1);
        GameManager.Instance.PlayerSpawn();
    }

	public void ReturnToTown()
	{
		if (FirebaseManager.Instance.CurrentPartyData.party_Owner.user_Id ==
		    FirebaseManager.Instance.CurrentUserData.user_Id)
		{
			photonView.RPC("RPC_ReturnToTown", RpcTarget.All);
		}
		else
		{
			RPC_ReturnToTown();
		}
	}

    [PunRPC]
    private void RPC_ReturnToTown()
    {
        player.currentHp = player.maxHp;
        FirebaseManager.Instance.CurrentUserData.UpdateUserData(
            hp: player.currentHp);
        FirebaseManager.Instance.UploadCurrentUserData();
        GameManager.Instance.ChangeScene(1);
        ServerManager.LeaveAndLoadScene("LJW_TownScene");
    }
}
