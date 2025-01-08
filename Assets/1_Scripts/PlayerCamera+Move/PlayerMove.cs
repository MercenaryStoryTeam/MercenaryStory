using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]

public class PlayerMove : MonoBehaviour
{
    [Header("이동 속도 (5 적정)")]
    [SerializeField] private float moveSpeed = 5f; // [SerializeField] 사용하는 이유: private 상태의 변수를 인스펙터상에 노출시키기 위해 사용

    [Header("바닥 체크 범위 (0.5 적정)")]
    [SerializeField] private float groundCheckRadius = 0.5f;

    [Header("바닥 레이어")]
    [SerializeField] private LayerMask groundLayer; // 바닥으로 체크할 대상의 레이어

    [Header("Virtual Camera 할당")]
    [SerializeField] private Transform cameraTransform; // 현재 보이는 뷰를 기준으로 이동 처리

    [Header("캐릭터 하위 빈오브젝트 Transform")]
    [SerializeField] private Transform ankleTransform; // 캐릭터 하위 빈오브젝트 위치를 기준으로 바닥 접촉 여부 확인

    [Header("씬 목록 (리스트에 씬이름 추가시 추가된 씬에서 이동 애니메이션 변경)")]
    [SerializeField] private List<string> specialScenes = new List<string>(); // 인스펙터에서 씬 할당 -> 마을에서 다른 기본 움직임 구현 가능

    [Header("사망 애니메이션 재생 시간")]
    [SerializeField] private float dieAnimationDuration = 2f; // 사망 애니메이션의 지속 시간

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
        Die
    }

    private State currentState = State.Idle;

    // 현재 속도
    private float currentSpeed;

    // 사망 여부
    private bool isDead = false;

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
        if (!isDead) // 사망 상태일 때는 입력 처리 및 상태 변경을 막음
        {
            HandleInput();    // 입력 처리 먼저
            HandleState();    // 그 후 상태 처리
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

        // Speed 파라미터 정규화 (0 ~ 1 사이) -> 인스펙터에서 이동속도만 변경해도 충분, 애니메이터에서 따로 조건 수정할 필요 없음
        float normalizedSpeed = (movementInput.magnitude * currentSpeed) / moveSpeed;
        animator.SetFloat("Speed", normalizedSpeed);
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
        if (currentState == newState) return;

        if (isDead && newState != State.Die) return; // 사망 상태에서는 Die 상태 외 전환 불가

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

    // Die 상태
    private void EnterDieState()
    {
        isDead = true;
        animator.SetTrigger("Die"); // Animator에서 'Die' 트리거를 설정

        // Die 상태에서 Invoke를 이용해 dieAnimationDuration 시간 만큼 애니메이션 재생 후 DisablePlayer 메서드 호출
        Invoke("DisablePlayer", dieAnimationDuration); // Invoke: 지정한 시간 후에 특정 메서드 호출
    }

    /// <summary>
    /// 플레이어 오브젝트 비활성화
    /// </summary>
    private void DisablePlayer()
    {
        gameObject.SetActive(false); // DisablePlayer 메서드 호출 시 플레이어 비활성화 처리
    }

    /// <summary>
    /// 플레이어 사망 처리 메서드
    /// </summary>
    public void Die()
    {
        if (!isDead) // 플레이어가 Die 상태인지 여부 체크
        {
            TransitionToState(State.Die);
        }
    }

    /// <summary>
    /// Gizmos 시각화, 추후 사망, 공격 모션 구현할때 유의미 할 것으로 보임
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (ankleTransform == null)
            return;

        // 플레이어의 바닥 접촉 여부를 판단하는 OverlapSphere 시각화
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Vector3 sphereOrigin = ankleTransform.position + Vector3.down * 0.05f; // 약간 아래 위치
        float sphereRadius = groundCheckRadius;
        Gizmos.DrawWireSphere(sphereOrigin, sphereRadius);

        // 현재 플레이어 위치 시각화 (플레이어 아래 위치에 있는 빨간공)
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.02f);
    }

#if UNITY_EDITOR
    /// <summary>
    /// 현재 상태 정보 디버그
    /// </summary>
    private void LogDebugInfo()
    {
        Debug.Log($"Scene: {animator.GetBool("Scene")}, State: {currentState}, Movement: {movementInput}, Grounded: {isGrounded}, Velocity: {rb.velocity}");
    }
#endif
}
