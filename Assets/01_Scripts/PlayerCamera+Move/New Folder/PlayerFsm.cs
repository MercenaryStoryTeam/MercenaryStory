using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Photon.Pun;
using Cinemachine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class PlayerFsm : MonoBehaviourPun
{
    [Header("Virtual Camera 할당 (자동으로 할당됩니다)")]
    public Transform cameraTransform;

    [Header("Die 애니메이션 재생 시간")]
    public float dieAnimationDuration = 2f;

    [Header("콤보 타이머")]
    public float comboResetTime = 1.0f;

    [Header("씬 목록")]
    public List<string> specialScenes = new List<string>();

    private Rigidbody rb;
    private Animator animator;
    private Vector3 movementInput;
    private const float moveThreshold = 0.05f;

    // 현재 이동 속도
    private float currentSpeed;

    public enum State
    {
        Idle,
        Moving,
        Attack1,
        Attack2,
        Attack3,
        Skill,
        Die,
        Hit // 피격 상태
    }

    private State currentState = State.Idle;
    private bool isDead = false;
    private int attackCombo = 0;
    private float lastAttackTime = 0f;

    private SkillFsm skillFsm;
    private Player player; // Player 스크립트 참조를 위한 변수

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

        // SkillFsm 스크립트 참조
        skillFsm = GetComponent<SkillFsm>();
        if (skillFsm == null)
        {
            Debug.LogError("PlayerFsm의 GameObject에 SkillFsm 스크립트가 없습니다.");
            enabled = false;
            return;
        }

        // Player 스크립트 참조
        player = GetComponent<Player>();
        if (player == null)
        {
            Debug.LogError("PlayerFsm의 GameObject에 Player 스크립트가 없습니다.");
            enabled = false;
        }
    }

    private void Update()
    {
        if (!isDead)
        {
            HandleInput();
            HandleState();
        }
    }

    private void FixedUpdate()
    {
        HandlePhysics();
    }

    private bool IsCurrentSceneSpecial()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        return specialScenes.Contains(currentSceneName);
    }

    private void HandleInput()
    {
        // Hit 상태도 이동 가능하도록 수정
        bool canProcessMovement = (currentState != State.Die && currentState != State.Skill);

        if (canProcessMovement)
        {
            // 기본 이동키 입력
            float inputX = Input.GetAxisRaw("Horizontal");
            float inputZ = Input.GetAxisRaw("Vertical");

            // 기본 마우스 클릭 입력
            movementInput = CalculateMovementDirection(inputX, inputZ);

            // Player의 moveSpeed를 참조하여 현재 이동 속도 설정
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

            // baseMoveSpeed가 기준이므로, 현재 이동 속도 대비 비율 계산
            float normalizedSpeed = (movementInput.magnitude * currentSpeed) /
                                    (player != null ? player.moveSpeed : 1f);
            animator.SetFloat("Speed", normalizedSpeed);
        }
        else
        {
            movementInput = Vector3.zero;
            animator.SetFloat("Speed", 0f);
        }

        // 공격 입력 처리 시 현재 상태가 Moving, Skill이 아닌지 확인
        // (Hit 상태에서도 공격을 막고 싶다면 여기서 조건을 추가하면 됨)
        if (Input.GetMouseButtonDown(0) && !isDead && currentState != State.Moving &&
            currentState != State.Skill)
        {
            HandleAttackInput();
        }

        if (Time.time - lastAttackTime > comboResetTime)
        {
            attackCombo = 0;
        }
    }

    private void HandleAttackInput()
    {
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
                // 이동/정지 상태
                break;
            case State.Attack1:
            case State.Attack2:
            case State.Attack3:
                // 공격 상태
                break;
            case State.Skill:
                // 스킬 상태
                break;
            case State.Hit:
                // 피격 상태 (이제 별도 이동 막거나 전환 처리하지 않음)
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
                // 공격 중 물리 처리
                break;
            case State.Skill:
                // 스킬 중 물리 처리
                break;
            case State.Die:
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                break;
            case State.Hit:
                // 피격 중에도 이동 가능하도록 별도 처리하지 않음
                // (원한다면 MovePlayer() 호출도 가능)
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
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * 10f);
        }
    }

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
            case State.Skill:
                break;
            case State.Die:
                EnterDieState();
                break;
            case State.Hit:
                EnterHitState();
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

    /// <summary>
    /// 피격 상태로 전환
    /// </summary>
    private void EnterHitState()
    {
        animator.SetTrigger("Hit");
    }

    /// <summary>
    /// 외부에서 호출해 피격 상태로 만들기
    /// </summary>
    public void TakeDamage()
    {
        // 이미 죽었거나, 다른 이유로 피격 불가라면 무시
        if (isDead || currentState == State.Hit) return;
        TransitionToState(State.Hit);
    }

    public void Die()
    {
        if (!isDead)
        {
            TransitionToState(State.Die);
        }
    }

    // 공격 애니메이션 이벤트
    public void OnAttackAnimationEnd()
    {
        if (movementInput.sqrMagnitude > moveThreshold)
            TransitionToState(State.Moving);
        else
            TransitionToState(State.Idle);
    }

    // 스킬 애니메이션 이벤트
    public void OnSkillAnimationEnd()
    {
        if (movementInput.sqrMagnitude > moveThreshold)
            TransitionToState(State.Moving);
        else
            TransitionToState(State.Idle);
    }

    // Hit 애니메이션 이벤트 - 별도 처리 없이 둠
    public void OnHitAnimationEnd()
    {
        // Hit 애니 끝나도 자동 복귀하지 않음.
        // (원하는 경우 주석 해제해서 복귀 로직 추가 가능)
        // if (movementInput.sqrMagnitude > moveThreshold)
        //     TransitionToState(State.Moving);
        // else
        //     TransitionToState(State.Idle);
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

    public void TransitionToSkillState(string skillName)
    {
        if (currentState == State.Die) return;

        currentState = State.Skill;

        float skillAnimationDuration = GetSkillAnimationDuration(skillName);
        Invoke("ExitSkillState", skillAnimationDuration);
    }

    private float GetSkillAnimationDuration(string skillName)
    {
        switch (skillName)
        {
            case "Rush":
                return 1f;
            case "Parry":
                return 1f;
            case "Skill1":
                return 1f;
            case "Skill2":
                return 1f;
            default:
                return 1f;
        }
    }

    private void ExitSkillState()
    {
        if (movementInput.sqrMagnitude > moveThreshold)
            TransitionToState(State.Moving);
        else
            TransitionToState(State.Idle);
    }
}
