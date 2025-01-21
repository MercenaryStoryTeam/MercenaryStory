using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Photon.Pun;
using Cinemachine;
using UnityEngine.InputSystem.HID;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class PlayerFsm : MonoBehaviourPun
{
	[Header("Virtual Camera 할당 (자동으로 할당됩니다)")]
	public Transform cameraTransform;

	[Header("Die 애니메이션 재생 시간")] public float dieAnimationDuration = 2f;

	[Header("콤보 타이머")] public float comboResetTime = 1.0f;

	[Header("씬 목록")] public List<string> specialScenes = new List<string>();

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
		Attack3,
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
			Debug.LogWarning("Rigidbody useGravity is disabled.");
		}

		// Virtual Camera를 자동으로 찾아 할당
		if (!cameraTransform)
		{
			var vCam = FindObjectOfType<CinemachineVirtualCamera>();
			if (vCam)
			{
				cameraTransform = vCam.transform;
				vCam.GetComponent<VirtualCameraController>().enabled = true;
				Debug.Log("Virtual Camera가 자동 할당되었습니다.");
			}
			else
			{
				Debug.LogError("씬에 Virtual Camera가 없습니다. 카메라를 수동으로 할당하세요.");
			}
		}

		animator.applyRootMotion = false;

		bool isScene = IsCurrentSceneSpecial();
		animator.SetBool("Scene", isScene);

		// Player 스크립트 참조 (이동 속도, 스탯 등 활용)
		player = GetComponent<Player>();
		if (player == null)
		{
			Debug.LogError("PlayerFsm의 GameObject에 Player 스크립트가 없습니다.");
			enabled = false;
		}
	}

	private void Start()
	{
		if (FirebaseManager.Instance.CurrentUserData.user_Name == gameObject.name)
		{
			if (StageManager.Instance != null)
			{
				StageManager.Instance.playerFsm = this;
			}
		}
	}

	private void OnEnable()
	{
		PlayerInputManager.OnAttackInput += HandleAttackInput;
	}

	private void OnDisable()
	{
		// 콤보 공격 이벤트 해제
		PlayerInputManager.OnAttackInput -= HandleAttackInput;
	}

	private void Update()
	{
		if (!isDead)
		{
			HandleMovementInput();
			HandleState();
		}

        // pun 동기화를 위함. 지우지 마시오!! - 지원
        if (!photonView.IsMine) return;
    }

    private void FixedUpdate()
	{
		HandlePhysics();

        // pun 동기화를 위함. 지우지 마시오!! - 지원
        if (!photonView.IsMine) return;
    }

    private bool IsCurrentSceneSpecial()
	{
		string currentSceneName = SceneManager.GetActiveScene().name;
		return specialScenes.Contains(currentSceneName);
	}

	// 이동 입력만 별도 처리 (콤보 공격은 PlayerInputManager 이벤트 사용)
	private void HandleMovementInput()
	{
		// Die 상태가 아닐 때만 이동 처리 (Hit 상태도 이동 가능)
		bool canProcessMovement = (currentState != State.Die) && !isMovementLocked;

		if (canProcessMovement)
		{
			float inputX = Input.GetAxisRaw("Horizontal");
			float inputZ = Input.GetAxisRaw("Vertical");
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
		else
		{
			movementInput = Vector3.zero;
			animator.SetFloat("Speed", 0f);
		}

		// 콤보 리셋 확인
		if (Time.time - lastAttackTime > comboResetTime)
		{
			attackCombo = 0;
		}
	}

	// 플레이어가 공격을 시도할 때 호출되는 콤보 처리 함수
	private void HandleAttackInput()
	{
		// 사망이거나, 이동 중이거나, 공격 잠금 중이면 공격 불가
		if (isDead || currentState == State.Moving || isAttackLocked)
			return;

		lastAttackTime = Time.time;
		attackCombo++;
		if (attackCombo > 3)
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
			case 3:
				TransitionToState(State.Attack3);
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
			case State.Attack3:
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
				idleVelocity.x = Mathf.Lerp(idleVelocity.x, 0f, Time.fixedDeltaTime * 10f);
				idleVelocity.z = Mathf.Lerp(idleVelocity.z, 0f, Time.fixedDeltaTime * 10f);
				rb.velocity = idleVelocity;
				break;
			case State.Moving:
				MovePlayer();
				break;
			case State.Attack1:
			case State.Attack2:
			case State.Attack3:
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
				Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * 10f);
		}
	}

	// 상태 관리
	public void TransitionToState(State newState, bool force = false)
	{
		if (isDead && newState != State.Die && !force) return;
		if (currentState == newState) return;

		currentState = newState;

		switch (currentState)
		{
			case State.Idle:
				EnterIdleState();
				break;
			case State.Moving:
				EnterMovingState();
				break;
			case State.Attack1:
				EnterAttackState(1);
				break;
			case State.Attack2:
				EnterAttackState(2);
				break;
			case State.Attack3:
				EnterAttackState(3);
				break;
			case State.Hit:
				EnterHitState();
				break;
			case State.Die:
				EnterDieState();
				break;
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
		switch (attackNumber)
		{
			case 1:
				animator.SetTrigger("Attack1");
				break;
			case 2:
				animator.SetTrigger("Attack2");
				break;
			case 3:
				animator.SetTrigger("Attack3");
				break;
		}
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
	private void EnterHitState()
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
}