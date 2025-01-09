using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]

public class PlayerMove : MonoBehaviour
{
    [Header("이동 속도 (5 적정)")]
    [SerializeField] private float moveSpeed = 5f; // [SerializeField] private 사용한 이유: private을 사용하면서 인스펙터에 노출, 다른 스크립트에 의해 의도치 않은 정보 변경을 막고자 

    [Header("바닥 체크 범위 (0.5 적정)")]
    [SerializeField] private float groundCheckRadius = 0.5f;

    [Header("바닥 레이어")]
    [SerializeField] private LayerMask groundLayer;

    [Header("Virtual Camera 할당")]
    [SerializeField] private Transform cameraTransform;

    [Header("캐릭터 하위 빈오브젝트 Transform")]
    [SerializeField] private Transform ankleTransform;

    [Header("씬 목록 (리스트에 씬이름 추가시 추가된 씬에서 이동 애니메이션 변경)")]
    [SerializeField] private List<string> specialScenes = new List<string>();

    [Header("Die 애니메이션 재생 시간 (2초 적정)")]
    [SerializeField] private float dieAnimationDuration = 2f;

    [Header("콤보 타이머 (1초 적정)")]
    [SerializeField] private float comboResetTime = 1.0f; // 콤보를 초기화할 시간

    private Rigidbody rb;
    private Animator animator;

    // 이동 제어
    private Vector3 movementInput; // 이동 방향
    private bool isGrounded = false;

    // 이동 상태로 전환하기 위한 최소 입력 크기
    private const float moveThreshold = 0.05f;

    private enum State
    {
        Idle,
        Moving,
        Attack1,
        Attack2,
        Attack3,
        Die
    }

    private State currentState = State.Idle;

    // 현재 속도
    private float currentSpeed;

    // 사망 여부
    private bool isDead = false;

    // 콤보 변수
    private int attackCombo = 0; // 현재 콤보 수
    private float lastAttackTime = 0f; // 마지막 공격 입력 시간

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        // Rigidbody 설정 확인 
        if (rb.isKinematic)
        {
            Debug.LogWarning("Rigidbody의 Kinematic이 활성화되어 있습니다.");
        }

        if (!rb.useGravity)
        {
            Debug.LogWarning("Rigidbody의 Use Gravity가 비활성화되어 있습니다.");
        }

        // 메인 카메라 자동 할당
        if (!cameraTransform)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera)
            {
                cameraTransform = mainCamera.transform;
            }
            else
            {
                Debug.LogError("Main Camera가 존재하지 않습니다.");
                enabled = false;
                return;
            }
        }

        // Animator 설정 확인
        animator.applyRootMotion = false;

        // 현재 씬이 특별한 씬인지 확인하고 Animator 파라미터 설정
        bool isScene = IsCurrentSceneSpecial();
        animator.SetBool("Scene", isScene);

        // 씬 변경 시 Scene 파라미터 업데이트
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // 이벤트 해제
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool isScene = IsCurrentSceneSpecial();
        animator.SetBool("Scene", isScene);
    }

    private void Update()
    {
#if UNITY_EDITOR
        LogDebugInfo();
#endif
        if (!isDead) // Die 상태일 때는 입력 처리 및 상태 변경을 막음
        {
            HandleInput(); // 입력 처리
            HandleState(); // 상태 처리
        }
    }

    private void FixedUpdate()
    {
        CheckGrounded();   // 바닥 판정
        HandlePhysics();   // 상태별 물리 처리
    }

    /// <summary>
    /// 현재 씬이 specialScenes에 포함된 씬인지 확인하는 함수
    /// </summary>
    /// <returns>specialScenes에 포함되면 true, 아니면 false</returns>
    private bool IsCurrentSceneSpecial()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        return specialScenes.Contains(currentSceneName);
    }

    /// <summary>
    /// 바닥 판정 (OverlapSphere 사용)
    /// </summary>
    private void CheckGrounded()
    {
        isGrounded = false;

        Vector3 sphereOrigin = ankleTransform.position + Vector3.down * 0.05f; // 약간 아래로 위치
        float sphereRadius = groundCheckRadius;
        Collider[] colliders = Physics.OverlapSphere(sphereOrigin, sphereRadius, groundLayer);

        if (colliders.Length > 0)
        {
            isGrounded = true;
        }

        // Animator -> isGrounded 파라미터 
        animator.SetBool("isGrounded", isGrounded);
    }

    /// <summary>
    /// 사용자 입력 처리
    /// </summary>
    private void HandleInput()
    {
        // 공격 중이거나 사망 상태가 아닐 때만 이동 입력 처리
        bool canProcessMovement = !IsAttacking() && currentState != State.Die;

        if (canProcessMovement)
        {
            // 이동 방향 계산
            float inputX = Input.GetAxisRaw("Horizontal");
            float inputZ = Input.GetAxisRaw("Vertical");
            movementInput = CalculateMovementDirection(inputX, inputZ);

            // 현재 속도 설정 (달리기 기능 제거로 항상 moveSpeed 사용)
            currentSpeed = moveSpeed;

            // 이동/정지 상태 전환
            if (movementInput.sqrMagnitude > moveThreshold)
            {
                if (currentState != State.Moving)
                {
                    TransitionToState(State.Moving);
                }
            }
            else
            {
                if (currentState != State.Idle)
                {
                    TransitionToState(State.Idle);
                }
            }

            // Speed 파라미터 정규화 (0 ~ 1 사이)
            float normalizedSpeed = (movementInput.magnitude * currentSpeed) / moveSpeed;
            animator.SetFloat("Speed", normalizedSpeed);
        }
        else
        {
            // 공격 중이거나 사망 상태일 때는 이동을 멈추고 Speed를 0으로 설정
            movementInput = Vector3.zero;
            animator.SetFloat("Speed", 0f);
        }

        // 공격 입력 처리
        if (Input.GetMouseButtonDown(0) && !isDead)
        {
            HandleAttackInput();
        }

        // 콤보 리셋 타이머 처리
        if (Time.time - lastAttackTime > comboResetTime)
        {
            attackCombo = 0;
        }
    }

    /// <summary>
    /// 공격 입력 처리 및 콤보 로직 관리
    /// </summary>
    private void HandleAttackInput() // 공격 애니메이션 클립이 루프되지 않도록 설정 
    {
        lastAttackTime = Time.time;
        attackCombo++;

        if (attackCombo > 3)
        {
            attackCombo = 1; // 콤보가 3을 초과하면 초기화
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

    /// <summary>
    /// 상태에 따른 추가 로직
    /// </summary>
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
                // 공격 중 추가 로직이 필요하면 여기에 작성
                break;
        }
    }

    /// <summary>
    /// 상태별 물리 처리
    /// </summary>
    private void HandlePhysics()
    {
        switch (currentState)
        {
            case State.Idle:
                // Idle 상태면 속도 조금씩 감속
                Vector3 idleVelocity = rb.velocity;
                idleVelocity.x = Mathf.Lerp(rb.velocity.x, 0f, Time.fixedDeltaTime * 10f);
                idleVelocity.z = Mathf.Lerp(rb.velocity.z, 0f, Time.fixedDeltaTime * 10f);
                rb.velocity = idleVelocity;
                break;

            case State.Moving:
                MovePlayer();
                break;

            case State.Attack1:
            case State.Attack2:
            case State.Attack3:
                // 공격 중에는 플레이어 이동을 멈춤
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                break;

            case State.Die:
                // 사망 상태에서는 이동을 멈추고 속도를 0으로 설정
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                break;
        }
    }

    /// <summary>
    /// 실제 이동 처리 (X,Z만 갱신, Y는 유지)
    /// </summary>
    private void MovePlayer()
    {
        Vector3 moveVelocity = movementInput * currentSpeed;
        rb.velocity = new Vector3(moveVelocity.x, rb.velocity.y, moveVelocity.z);

        // 이동 방향으로 회전
        if (movementInput.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movementInput);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * 10f);
        }
    }

    /// <summary>
    /// 상태 전환
    /// </summary>
    private void TransitionToState(State newState)
    {
        // 사망 상태일 때 Die 상태로만 전환 가능
        if (isDead && newState != State.Die) return;

        // 공격 중일 때 Idle 또는 Moving 상태로의 전환 방지
        if (IsAttacking() && (newState == State.Idle || newState == State.Moving))
        {
            return;
        }

        if (currentState == newState) return;

        ExitCurrentState();
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
            case State.Die:
                EnterDieState();
                break;
        }
    }

    /// <summary>
    /// 상태 종료 처리
    /// </summary>
    private void ExitCurrentState()
    {
        // 현재 상태에서 종료 시 필요한 로직이 있으면 여기에 작성
    }

    // Idle 상태
    private void EnterIdleState()
    {
        animator.SetFloat("Speed", 0f);
    }

    // Moving 상태
    private void EnterMovingState()
    {
        float speed = movementInput.magnitude * currentSpeed;
        animator.SetFloat("Speed", speed);
    }

    /// <summary>
    /// 공격 상태 진입 처리
    /// </summary>
    /// <param name="attackNumber">공격 번호 (1, 2, 3)</param>
    private void EnterAttackState(int attackNumber)
    {
        // 공격 중 이동 멈춤
        rb.velocity = Vector3.zero;

        // 적절한 공격 애니메이션 트리거
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

        // 애니메이션 종료 시 상태 전환을 위해 Animation Event 사용
    }

    // Die 상태
    private void EnterDieState()
    {
        isDead = true;
        animator.SetTrigger("Die"); // Animator에서 'Die' 트리거 설정

        // Die 상태에서 Invoke를 이용해 dieAnimationDuration 시간 만큼 애니메이션 재생 후 DisablePlayer 메서드 호출
        Invoke("DisablePlayer", dieAnimationDuration); // 지정한 시간 후에 DisablePlayer 호출
    }

    /// <summary>
    /// 플레이어 오브젝트 비활성화
    /// </summary>
    private void DisablePlayer()
    {
        gameObject.SetActive(false); // 플레이어 비활성화
    }

    /// <summary>
    /// 플레이어 사망 처리 메서드
    /// </summary>
    public void Die() // 플레이어가 Die할 때 호출되어 Die 상태로 전환
    {
        if (!isDead) // 플레이어가 Die인지 여부 체크
        {
            TransitionToState(State.Die);
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// 현재 핵심 정보 디버그
    /// </summary>
    private void LogDebugInfo()
    {
        Debug.Log($"Scene: {animator.GetBool("Scene")}, State: {currentState}, Combo: {attackCombo}");
    }
#endif

    /// <summary>
    /// 애니메이션 종료 후 상태 전환 처리
    /// </summary>
    public void OnAttackAnimationEnd()
    {
        // 공격 애니메이션이 끝난 후, 현재 입력에 따라 이동 상태로 전환
        if (movementInput.sqrMagnitude > moveThreshold)
        {
            TransitionToState(State.Moving);
        }
        else
        {
            TransitionToState(State.Idle);
        }
    }

    /// <summary>
    /// 카메라 기준으로 이동 방향 계산
    /// </summary>
    private Vector3 CalculateMovementDirection(float inputX, float inputZ)
    {
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        // (Z축: 전후, X축: 좌우)
        Vector3 direction = (camForward * inputZ + camRight * inputX).normalized;
        return direction;
    }

    /// <summary>
    /// 현재 상태가 공격 상태인지 확인하는 메서드
    /// </summary>
    /// <returns>공격 중이면 true, 아니면 false</returns>
    private bool IsAttacking()
    {
        return currentState == State.Attack1 || currentState == State.Attack2 || currentState == State.Attack3;
    }
}
