using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Photon.Pun;
using Cinemachine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class PlayerFsm : MonoBehaviourPun
{
	[Header("Virtual Camera 할당 (자동으로 할당됩니다)")]
	public Transform cameraTransform;

	[Header("Die 애니메이션 재생 시간")] public float dieAnimationDuration = 2f;

	[Header("콤보 타이머")] public float comboResetTime = 1.0f;

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
	private int attackCombo = 0;
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
		rb = GetComponent<Rigidbody>();
		animator = GetComponent<Animator>();

		if (rb.isKinematic)
		{
			Debug.LogWarning("Rigidbody is Kinematic.");
		}

		if (!rb.useGravity)
		{
			Debug.LogWarning("Rigidbody useGravity가 비활성화되어 있습니다.");
		}

		// 플랫폼 감지
#if UNITY_ANDROID || UNITY_IOS
        isMobile = true;
#else
		// 모바일 입력 처리 허용
		isMobile = false;
#endif

		// Virtual Camera를 자동으로 찾아 할당
		if (!cameraTransform)
		{
			var vCam = FindObjectOfType<CinemachineVirtualCamera>();
			if (vCam)
			{
				cameraTransform = vCam.transform;
				var vCamController = vCam.GetComponent<VirtualCameraController>();
				if (vCamController != null)
				{
					vCamController.enabled = true;
				}

				Debug.Log("Virtual Camera가 자동 할당되었습니다.");
			}
			else
			{
				Debug.LogError("씬에 Virtual Camera가 없습니다. 카메라를 수동으로 할당하세요.");
			}
		}

		animator.applyRootMotion = false;

		// Player 스크립트 참조 (이동 속도, 스탯 등 활용)
		if (player == null)
		{
			player = GetComponent<Player>();
			if (player == null)
			{
				Debug.LogError("PlayerFsm의 GameObject에 Player 스크립트가 없습니다.");
				enabled = false;
			}
		}
	}

	private void Start()
	{
		if (FirebaseManager.Instance.CurrentUserData.user_Name == gameObject.name)
		{
			StageManager.Instance.currentPlayerFsm = this;
		}

		if (PhotonNetwork.IsMasterClient && StageManager.Instance != null)
		{
			if (FirebaseManager.Instance.CurrentPartyData.party_Owner.user_Name ==
			    gameObject.name)
			{
				StageManager.Instance.hostPlayerFsm = this;
			}
		}
	}

	private void OnEnable()
	{
		PlayerInputManager.OnMoveInput += HandleMoveInput;
		PlayerInputManager.OnAttackInput += HandleAttackInput;
		// 필요한 경우 다른 이벤트도 구독
	}

	private void OnDisable()
	{
		PlayerInputManager.OnMoveInput -= HandleMoveInput;
		PlayerInputManager.OnAttackInput -= HandleAttackInput;
		// 필요한 경우 다른 이벤트도 해제
	}

	private void Update()
	{
		// pun 동기화를 위함. 지우지 마시오!! - 지원
		if (!photonView.IsMine) return;

		if (!isDead)
		{
			HandleState();
		}
	}

	private void FixedUpdate()
	{
		// pun 동기화를 위함. 지우지 마시오!! - 지원
		if (!photonView.IsMine) return;

		HandlePhysics();
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
		// 사망이거나, Idle 상태가 아니거나, 공격 잠금 중이면 공격 불가
		if (isDead || currentState != State.Idle || isAttackLocked)
		{
			// 경고 메시지 출력
			Debug.LogWarning("[PlayerFsm] 현재 Idle 상태가 아니므로 공격을 사용할 수 없습니다.");
			return;
		}

		lastAttackTime = Time.time;
		attackCombo++;
		if (attackCombo > 2) // 최대 콤보를 2로 제한
		{
			attackCombo = 1;
		}

		switch (attackCombo)
		{
			case 1:
				TransitionToState(State.Attack1);
				break;
			case 2:
				TransitionToState(State.Attack2);
				break;
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
			rb.rotation =
				Quaternion.Slerp(rb.rotation, targetRotation,
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
		// 콤보 사운드 배열
		string[] comboSoundClips =
			{ "sound_player_hit1", "sound_player_hit2" }; // Attack3 제거

		// 지연 시간 설정 (필요 시 조정)
		float delay = 0.8f;

		// 랜덤 사운드 재생 호출
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

		// 랜덤으로 클립 선택
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

		// 업로드가 잘 되고 난 후에 받아오면 좋겠는데 이 부분 주의해야함. 일단은 그냥 바로 받아오자
		// 파티 정보 업데이트 (파티장과 파티원 둘 다 서버에서 받아오기)
		FirebaseManager.Instance.UpdateCurrentPartyDataAndLoadScene(sceneName);

		// ServerManager.LeaveScene(sceneName);
		// 여기서 loadlevel 해야 함. leave Scene을 여기서 하게 되면
		// 방 이동 안하는 상태에서는 다른 경우 발생
	}

	public void InstantiatePlayerPrefabs()
	{
		StageManager.Instance.currentStage++;
		StageManager.Instance.ChangeStage(StageManager.Instance.currentStage);
		photonView.RPC("RPC_InstantiatePlayerPrefabs", RpcTarget.Others);
	}

	[PunRPC]
	private void RPC_InstantiatePlayerPrefabs()
	{
		StageManager.Instance.currentStage++;
		StageManager.Instance.ChangeStage(StageManager.Instance.currentStage);
		StageManager.Instance.PlayerSpawn();
	}

	public void ReturnToTown()
	{
		photonView.RPC("RPC_ReturnToTown", RpcTarget.All);
	}

	[PunRPC]
	private void RPC_ReturnToTown()
	{
		player.currentHp = player.maxHp;
		FirebaseManager.Instance.CurrentUserData.UpdateUserData(
			hp: player.currentHp);
		ServerManager.LeaveAndLoadScene("LJW_TownScene");
	}
}