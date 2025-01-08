using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]

public class PlayerMove : MonoBehaviour
{
    [Header("�̵� �ӵ� (5 ����)")]
    [SerializeField] private float moveSpeed = 5f; // [SerializeField] ����ϴ� ����: private ������ ������ �ν����ͻ� �����Ű�� ���� ���

    [Header("�ٴ� üũ ���� (0.5 ����)")]
    [SerializeField] private float groundCheckRadius = 0.5f;

    [Header("�ٴ� ���̾�")]
    [SerializeField] private LayerMask groundLayer; // �ٴ����� üũ�� ����� ���̾�

    [Header("Virtual Camera �Ҵ�")]
    [SerializeField] private Transform cameraTransform; // ���� ���̴� �並 �������� �̵� ó��

    [Header("ĳ���� ���� �������Ʈ Transform")]
    [SerializeField] private Transform ankleTransform; // ĳ���� ���� �������Ʈ ��ġ�� �������� �ٴ� ���� ���� Ȯ��

    [Header("�� ��� (����Ʈ�� ���̸� �߰��� �߰��� ������ �̵� �ִϸ��̼� ����)")]
    [SerializeField] private List<string> specialScenes = new List<string>(); // �ν����Ϳ��� �� �Ҵ� -> �������� �ٸ� �⺻ ������ ���� ����

    [Header("��� �ִϸ��̼� ��� �ð�")]
    [SerializeField] private float dieAnimationDuration = 2f; // ��� �ִϸ��̼��� ���� �ð�

    private Rigidbody rb;
    private Animator animator;

    // �̵� ����
    private Vector3 movementInput; // �̵� ����
    private bool isGrounded = false;

    // �̵� ���·� ��ȯ�ϱ� ���� �ּ� �Է� ũ��
    private const float moveThreshold = 0.05f;

    private enum State
    {
        Idle,
        Moving,
        Die
    }

    private State currentState = State.Idle;

    // ���� �ӵ�
    private float currentSpeed;

    // ��� ����
    private bool isDead = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        // Rigidbody ���� Ȯ�� 
        if (rb.isKinematic)
        {
            Debug.LogWarning("Rigidbody�� Kinematic�� Ȱ��ȭ�Ǿ� �ֽ��ϴ�.");
        }

        if (!rb.useGravity)
        {
            Debug.LogWarning("Rigidbody�� Use Gravity�� ��Ȱ��ȭ�Ǿ� �ֽ��ϴ�.");
        }

        // ���� ī�޶� �ڵ� �Ҵ�
        if (!cameraTransform)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera)
            {
                cameraTransform = mainCamera.transform;
            }
            else
            {
                Debug.LogError("Main Camera�� �������� �ʽ��ϴ�.");
                enabled = false;
                return;
            }
        }

        // Animator ���� Ȯ��
        animator.applyRootMotion = false;

        // ���� ���� Ư���� ������ Ȯ���ϰ� Animator �Ķ���� ����
        bool isScene = IsCurrentSceneSpecial();
        animator.SetBool("Scene", isScene);

        // �� ���� �� Scene �Ķ���� ������Ʈ
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // �̺�Ʈ ����
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
        if (!isDead) // ��� ������ ���� �Է� ó�� �� ���� ������ ����
        {
            HandleInput();    // �Է� ó�� ����
            HandleState();    // �� �� ���� ó��
        }
    }

    private void FixedUpdate()
    {
        CheckGrounded();   // �ٴ� ����
        HandlePhysics();   // ���º� ���� ó��
    }

    /// <summary>
    /// ���� ���� specialScenes�� ���Ե� ������ Ȯ���ϴ� �Լ�
    /// </summary>
    /// <returns>specialScenes�� ���ԵǸ� true, �ƴϸ� false</returns>
    private bool IsCurrentSceneSpecial()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        return specialScenes.Contains(currentSceneName);
    }

    /// <summary>
    /// �ٴ� ���� (OverlapSphere ���)
    /// </summary>
    private void CheckGrounded()
    {
        isGrounded = false;

        Vector3 sphereOrigin = ankleTransform.position + Vector3.down * 0.05f; // �ణ �Ʒ��� ��ġ
        float sphereRadius = groundCheckRadius;
        Collider[] colliders = Physics.OverlapSphere(sphereOrigin, sphereRadius, groundLayer);

        if (colliders.Length > 0)
        {
            isGrounded = true;
        }

        // Animator -> isGrounded �Ķ���� 
        animator.SetBool("isGrounded", isGrounded);
    }

    /// <summary>
    /// ����� �Է� ó��
    /// </summary>
    private void HandleInput()
    {
        // �̵� ���� ���
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputZ = Input.GetAxisRaw("Vertical");
        movementInput = CalculateMovementDirection(inputX, inputZ);

        // ���� �ӵ� ���� (�޸��� ��� ���ŷ� �׻� moveSpeed ���)
        currentSpeed = moveSpeed;

        // �̵�/���� ���� ��ȯ
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

        // Speed �Ķ���� ����ȭ (0 ~ 1 ����) -> �ν����Ϳ��� �̵��ӵ��� �����ص� ���, �ִϸ����Ϳ��� ���� ���� ������ �ʿ� ����
        float normalizedSpeed = (movementInput.magnitude * currentSpeed) / moveSpeed;
        animator.SetFloat("Speed", normalizedSpeed);
    }

    /// <summary>
    /// ī�޶� �������� �̵� ���� ���
    /// </summary>
    private Vector3 CalculateMovementDirection(float inputX, float inputZ)
    {
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        // (Z��: ����, X��: �¿�)
        Vector3 direction = (camForward * inputZ + camRight * inputX).normalized;
        return direction;
    }

    /// <summary>
    /// ���¿� ���� �߰� ����
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
    /// ���º� ���� ó��
    /// </summary>
    private void HandlePhysics()
    {
        switch (currentState)
        {
            case State.Idle:
                // Idle ���¸� �ӵ� ���ݾ� ����
                Vector3 idleVelocity = rb.velocity;
                idleVelocity.x = Mathf.Lerp(rb.velocity.x, 0f, Time.fixedDeltaTime * 10f);
                idleVelocity.z = Mathf.Lerp(rb.velocity.z, 0f, Time.fixedDeltaTime * 10f);
                rb.velocity = idleVelocity;
                break;

            case State.Moving:
                MovePlayer();
                break;

            case State.Die:
                // ��� ���¿����� �̵��� ���߰� �ӵ��� 0���� ����
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                break;
        }
    }

    /// <summary>
    /// ���� �̵� ó�� (X,Z�� ����, Y�� ����)
    /// </summary>
    private void MovePlayer()
    {
        Vector3 moveVelocity = movementInput * currentSpeed;
        rb.velocity = new Vector3(moveVelocity.x, rb.velocity.y, moveVelocity.z);

        // �̵� �������� ȸ��
        if (movementInput.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movementInput);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * 10f);
        }
    }

    /// <summary>
    /// ���� ��ȯ
    /// </summary>
    private void TransitionToState(State newState)
    {
        if (currentState == newState) return;

        if (isDead && newState != State.Die) return; // ��� ���¿����� Die ���� �� ��ȯ �Ұ�

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
    /// ���� ���� ó��
    /// </summary>
    private void ExitCurrentState()
    {

    }

    // Idle ����
    private void EnterIdleState()
    {
        animator.SetFloat("Speed", 0f);
    }

    // Moving ����
    private void EnterMovingState()
    {
        float speed = movementInput.magnitude * currentSpeed;
        animator.SetFloat("Speed", speed);
    }

    // Die ����
    private void EnterDieState()
    {
        isDead = true;
        animator.SetTrigger("Die"); // Animator���� 'Die' Ʈ���Ÿ� ����

        // Die ���¿��� Invoke�� �̿��� dieAnimationDuration �ð� ��ŭ �ִϸ��̼� ��� �� DisablePlayer �޼��� ȣ��
        Invoke("DisablePlayer", dieAnimationDuration); // Invoke: ������ �ð� �Ŀ� Ư�� �޼��� ȣ��
    }

    /// <summary>
    /// �÷��̾� ������Ʈ ��Ȱ��ȭ
    /// </summary>
    private void DisablePlayer()
    {
        gameObject.SetActive(false); // DisablePlayer �޼��� ȣ�� �� �÷��̾� ��Ȱ��ȭ ó��
    }

    /// <summary>
    /// �÷��̾� ��� ó�� �޼���
    /// </summary>
    public void Die()
    {
        if (!isDead) // �÷��̾ Die �������� ���� üũ
        {
            TransitionToState(State.Die);
        }
    }

    /// <summary>
    /// Gizmos �ð�ȭ, ���� ���, ���� ��� �����Ҷ� ���ǹ� �� ������ ����
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (ankleTransform == null)
            return;

        // �÷��̾��� �ٴ� ���� ���θ� �Ǵ��ϴ� OverlapSphere �ð�ȭ
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Vector3 sphereOrigin = ankleTransform.position + Vector3.down * 0.05f; // �ణ �Ʒ� ��ġ
        float sphereRadius = groundCheckRadius;
        Gizmos.DrawWireSphere(sphereOrigin, sphereRadius);

        // ���� �÷��̾� ��ġ �ð�ȭ (�÷��̾� �Ʒ� ��ġ�� �ִ� ������)
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.02f);
    }

#if UNITY_EDITOR
    /// <summary>
    /// ���� ���� ���� �����
    /// </summary>
    private void LogDebugInfo()
    {
        Debug.Log($"Scene: {animator.GetBool("Scene")}, State: {currentState}, Movement: {movementInput}, Grounded: {isGrounded}, Velocity: {rb.velocity}");
    }
#endif
}
