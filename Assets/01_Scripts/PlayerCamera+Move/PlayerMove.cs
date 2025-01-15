using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Photon.Pun;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class PlayerMove : MonoBehaviourPun
{
    [Header("Virtual Camera 할당")]
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
    private float currentSpeed;
    private bool isDead = false;
    private int attackCombo = 0;
    private float lastAttackTime = 0f;

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

        if (!cameraTransform)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera)
            {
                cameraTransform = mainCamera.transform;
            }
            else
            {
                Debug.LogError("No Main Camera found.");
                enabled = false;
                return;
            }
        }

        animator.applyRootMotion = false;

        bool isScene = IsCurrentSceneSpecial();
        animator.SetBool("Scene", isScene);
    }

    private void Update()
    {
        //if (!photonView.IsMine) return;
        if (!isDead)
        {
            HandleInput();
            HandleState();
        }
    }

    private void FixedUpdate()
    {
        //if (!photonView.IsMine) return;
        HandlePhysics();
    }

    private bool IsCurrentSceneSpecial()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        return specialScenes.Contains(currentSceneName);
    }

    private void HandleInput()
    {
        bool canProcessMovement = (currentState != State.Die);
        if (canProcessMovement)
        {
            float inputX = Input.GetAxisRaw("Horizontal");
            float inputZ = Input.GetAxisRaw("Vertical");
            movementInput = CalculateMovementDirection(inputX, inputZ);
            currentSpeed = PlayerData.Instance.moveSpeed;

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

            float normalizedSpeed = (movementInput.magnitude * currentSpeed) / PlayerData.Instance.moveSpeed;
            animator.SetFloat("Speed", normalizedSpeed);
        }
        else
        {
            movementInput = Vector3.zero;
            animator.SetFloat("Speed", 0f);
        }

        if (Input.GetMouseButtonDown(0) && !isDead)
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
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * 10f);
        }
    }

    private void TransitionToState(State newState, bool force = false)
    {
        if (isDead && newState != State.Die && !force) return;
        if (currentState == newState) return;

        // 상태 변경 전 정리(필요 시)
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

    public void Die()
    {
        if (!isDead)
        {
            TransitionToState(State.Die);
        }
    }

    public void OnAttackAnimationEnd()
    {
        if (movementInput.sqrMagnitude > moveThreshold)
            TransitionToState(State.Moving);
        else
            TransitionToState(State.Idle);
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
}

//