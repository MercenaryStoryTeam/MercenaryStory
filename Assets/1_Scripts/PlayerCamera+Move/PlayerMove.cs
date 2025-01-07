using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class PlayerMove : MonoBehaviour
{
    [Header("이동 속도 (2 적정)")]
    [Tooltip("애니메이션 상태 전환간의 값 입력 공식\nㄴ Idle -> Walk : 0.1\nㄴ Walk -> Run : 달리기 값 그대로 입력\nㄴ Run -> Walk : 달리기 값 + 0.1\nㄴ Walk -> Idle : 0.2")]
    [SerializeField] private float moveSpeed = 2f;

    [Header("달리기 속도 (3 적정)")]
    [Tooltip("애니메이션 상태 전환간의 값 입력 공식\nㄴ Idle -> Walk : 0.1\nㄴ Walk -> Run : 달리기 값 그대로 입력\nㄴ Run -> Walk : 달리기 값 + 0.1\nㄴ Walk -> Idle : 0.2")]
    [SerializeField] private float runSpeed = 3f;

    [Header("점프 정도 (5 적정)")]
    [SerializeField] private float jumpForce = 5f;

    [Header("바닥 체크 범위 (0.5 적정)")]
    [SerializeField] private float groundCheckRadius = 0.5f;

    [Header("바닥 레이어")]
    [SerializeField] private LayerMask groundLayer; // 바닥으로 체크할 대상

    [Header("Virtual Camera 할당")]
    [SerializeField] private Transform cameraTransform; // 현재 보이는 뷰를 기준으로 이동 처리

    [Header("플레이어 하위 GameObject Transform")]
    [SerializeField] private Transform ankleTransform; // 해당 위치를 기준으로 OverlapSphere 활성화

    private Rigidbody rb;
    private Animator animator;

    // 이동/점프 제어
    private Vector3 movementInput; // 이동 방향(월드 기준)
    private bool isGrounded = false;
    private bool canJump = true;

    // 이동/정지 상태 전환 임계값
    private const float moveThreshold = 0.05f;

    private enum State
    {
        Idle,
        Moving,
        Jumping
    }

    private State currentState = State.Idle;

    // 현재 속도
    private float currentSpeed;

    // 달리기 키를 상수로 내부에서 고정 (Left Shift)
    private const KeyCode runKey = KeyCode.LeftShift;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        // Rigidbody 설정 확인
        if (rb.isKinematic)
        {
            Debug.LogWarning("Rigidbody가 Kinematic으로 설정되어 있습니다. 점프 및 물리 동작이 제대로 작동하지 않을 수 있습니다.");
        }

        // 카메라 트랜스폼 자동 할당
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
            }
        }

        // Animator 설정 확인
        animator.applyRootMotion = false;
    }

    private void Update()
    {
#if UNITY_EDITOR
        // 디버그용 로그
        Debug.Log($"State: {currentState}, Movement: {movementInput}, Grounded: {isGrounded}, canJump: {canJump}");
#endif
        HandleInput();    // 입력 처리 먼저
        HandleState();    // 그 후 상태 처리
    }

    private void FixedUpdate()
    {
        CheckGrounded();   // 바닥 판정
        HandlePhysics();   // 상태별 물리 처리
    }

    /// <summary>
    /// 바닥 판정 (OverlapSphere 사용)
    /// </summary>
    private void CheckGrounded()
    {
        bool wasGrounded = isGrounded;
        isGrounded = false;

        Vector3 sphereOrigin = ankleTransform.position; // 발목 위치 기준
        float sphereRadius = groundCheckRadius;
        Collider[] colliders = Physics.OverlapSphere(sphereOrigin, sphereRadius, groundLayer);

        if (colliders.Length > 0)
        {
            isGrounded = true;
            if (!wasGrounded)
            {
                canJump = true;
                Debug.Log($"착지: canJump가 true로 설정됨. 감지된 오브젝트 수: {colliders.Length}");
            }
        }
        else
        {
            isGrounded = false;
            canJump = false;
            Debug.Log("공중: canJump가 false로 설정됨");
        }

        // Animator의 isGrounded 파라미터 업데이트
        animator.SetBool("isGrounded", isGrounded);
    }

    /// <summary>
    /// 사용자 입력 처리
    /// </summary>
    private void HandleInput()
    {
        // 점프 중에는 다른 입력 무시
        if (currentState == State.Jumping) return;

        // 이동 방향 계산
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputZ = Input.GetAxisRaw("Vertical");
        movementInput = CalculateMovementDirection(inputX, inputZ);

        // 달리기 입력 확인
        bool isRunning = Input.GetKey(runKey) && movementInput.sqrMagnitude > moveThreshold;

        // 현재 속도 설정
        currentSpeed = isRunning ? runSpeed : moveSpeed;

        // 점프 입력 처리
        if (Input.GetButtonDown("Jump") && isGrounded && canJump && currentState != State.Jumping)
        {
            Debug.Log("Jump 버튼이 눌림");
            TransitionToState(State.Jumping);
            return;
        }

        // 이동/정지 상태 전환
        if (movementInput.sqrMagnitude > moveThreshold)
        {
            if (currentState != State.Moving && currentState != State.Jumping)
            {
                TransitionToState(State.Moving);
            }
        }
        else
        {
            if (isGrounded && currentState != State.Idle && currentState != State.Jumping)
            {
                TransitionToState(State.Idle);
            }
        }

        // Animator의 Speed 파라미터 업데이트
        float speed = movementInput.magnitude * currentSpeed;
        animator.SetFloat("Speed", speed);
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
    /// 상태에 따른 추가 로직
    /// </summary>
    private void HandleState()
    {
        switch (currentState)
        {
            case State.Idle:
            case State.Moving:
                // 별도 로직 없음
                break;

            case State.Jumping:
                // 점프 중 바닥에 닿으면 Moving/Idle 전환
                if (isGrounded)
                {
                    if (movementInput.sqrMagnitude > moveThreshold)
                        TransitionToState(State.Moving);
                    else
                        TransitionToState(State.Idle);
                }
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
                // Idle 상태면 XZ 속도 0으로 점진적으로 감속
                Vector3 idleVelocity = rb.velocity;
                idleVelocity.x = Mathf.Lerp(rb.velocity.x, 0f, Time.fixedDeltaTime * 10f);
                idleVelocity.z = Mathf.Lerp(rb.velocity.z, 0f, Time.fixedDeltaTime * 10f);
                rb.velocity = idleVelocity;
                Debug.Log($"HandlePhysics: Idle Velocity = {rb.velocity}");
                break;

            case State.Moving:
                MovePlayer();
                Debug.Log($"HandlePhysics: Moving Velocity = {rb.velocity}");
                break;

            case State.Jumping:
                // 점프 중에도 공중 이동
                ApplyAirControl();
                Debug.Log($"HandlePhysics: Jumping Velocity = {rb.velocity}");
                break;
        }
    }

    /// <summary>
    /// 실제 이동 처리 (XZ만 갱신, Y는 중력/점프 유지)
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
    /// 공중 제어 적용
    /// </summary>
    private void ApplyAirControl()
    {
        Vector3 airMove = movementInput * currentSpeed * 0.5f; // 공중 이동 속도 감소
        Vector3 newVelocity = new Vector3(airMove.x, rb.velocity.y, airMove.z);
        rb.velocity = Vector3.Lerp(rb.velocity, newVelocity, Time.fixedDeltaTime * 5f);

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
        Debug.Log($"State 전환: {currentState} -> {newState}");

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
            case State.Jumping:
                EnterJumpingState();
                break;
        }
    }

    /// <summary>
    /// 상태 종료 처리
    /// </summary>
    private void ExitCurrentState()
    {
        // Animator의 Speed 파라미터는 HandleInput에서 이미 관리하므로 별도 처리 불필요
        // 필요 시 추가적인 상태 종료 로직을 여기에 작성
    }

    // Idle
    private void EnterIdleState()
    {
        animator.SetFloat("Speed", 0f);
    }

    // Moving
    private void EnterMovingState()
    {
        float speed = movementInput.magnitude * currentSpeed;
        animator.SetFloat("Speed", speed);
    }

    // Jumping
    private void EnterJumpingState()
    {
        animator.SetTrigger("Jump");
        Jump();
    }

    /// <summary>
    /// 점프 로직
    /// </summary>
    private void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z); // 기존 Y 속도 초기화
        rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
        isGrounded = false;
        canJump = false;  // 착지 전까지 다시 점프 불가
        Debug.Log("Jump 실행: Y 속도 설정 및 canJump False");
    }

    /// <summary>
    /// (옵션) Gizmos 시각화
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (ankleTransform == null)
            return;

        // OverlapSphere 시각화
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Vector3 sphereOrigin = ankleTransform.position;
        float sphereRadius = groundCheckRadius;
        Gizmos.DrawWireSphere(sphereOrigin, sphereRadius);

        // 현재 플레이어 위치 시각화
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.02f);
    }
}

// 중간 완성