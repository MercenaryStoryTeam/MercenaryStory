using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class PlayerMove : MonoBehaviour
{
    [Header("�̵� �ӵ� (2 ����)")]
    [Tooltip("�ִϸ��̼� ���� ��ȯ���� �� �Է� ����\n�� Idle -> Walk : 0.1\n�� Walk -> Run : �޸��� �� �״�� �Է�\n�� Run -> Walk : �޸��� �� + 0.1\n�� Walk -> Idle : 0.2")]
    [SerializeField] private float moveSpeed = 2f;

    [Header("�޸��� �ӵ� (3 ����)")]
    [Tooltip("�ִϸ��̼� ���� ��ȯ���� �� �Է� ����\n�� Idle -> Walk : 0.1\n�� Walk -> Run : �޸��� �� �״�� �Է�\n�� Run -> Walk : �޸��� �� + 0.1\n�� Walk -> Idle : 0.2")]
    [SerializeField] private float runSpeed = 3f;

    [Header("���� ���� (5 ����)")]
    [SerializeField] private float jumpForce = 5f;

    [Header("�ٴ� üũ ���� (0.5 ����)")]
    [SerializeField] private float groundCheckRadius = 0.5f;

    [Header("�ٴ� ���̾�")]
    [SerializeField] private LayerMask groundLayer; // �ٴ����� üũ�� ���

    [Header("Virtual Camera �Ҵ�")]
    [SerializeField] private Transform cameraTransform; // ���� ���̴� �並 �������� �̵� ó��

    [Header("�÷��̾� ���� GameObject Transform")]
    [SerializeField] private Transform ankleTransform; // �ش� ��ġ�� �������� OverlapSphere Ȱ��ȭ

    private Rigidbody rb;
    private Animator animator;

    // �̵�/���� ����
    private Vector3 movementInput; // �̵� ����(���� ����)
    private bool isGrounded = false;
    private bool canJump = true;

    // �̵�/���� ���� ��ȯ �Ӱ谪
    private const float moveThreshold = 0.05f;

    private enum State
    {
        Idle,
        Moving,
        Jumping
    }

    private State currentState = State.Idle;

    // ���� �ӵ�
    private float currentSpeed;

    // �޸��� Ű�� ����� ���ο��� ���� (Left Shift)
    private const KeyCode runKey = KeyCode.LeftShift;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        // Rigidbody ���� Ȯ��
        if (rb.isKinematic)
        {
            Debug.LogWarning("Rigidbody�� Kinematic���� �����Ǿ� �ֽ��ϴ�. ���� �� ���� ������ ����� �۵����� ���� �� �ֽ��ϴ�.");
        }

        // ī�޶� Ʈ������ �ڵ� �Ҵ�
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
            }
        }

        // Animator ���� Ȯ��
        animator.applyRootMotion = false;
    }

    private void Update()
    {
#if UNITY_EDITOR
        // ����׿� �α�
        Debug.Log($"State: {currentState}, Movement: {movementInput}, Grounded: {isGrounded}, canJump: {canJump}");
#endif
        HandleInput();    // �Է� ó�� ����
        HandleState();    // �� �� ���� ó��
    }

    private void FixedUpdate()
    {
        CheckGrounded();   // �ٴ� ����
        HandlePhysics();   // ���º� ���� ó��
    }

    /// <summary>
    /// �ٴ� ���� (OverlapSphere ���)
    /// </summary>
    private void CheckGrounded()
    {
        bool wasGrounded = isGrounded;
        isGrounded = false;

        Vector3 sphereOrigin = ankleTransform.position; // �߸� ��ġ ����
        float sphereRadius = groundCheckRadius;
        Collider[] colliders = Physics.OverlapSphere(sphereOrigin, sphereRadius, groundLayer);

        if (colliders.Length > 0)
        {
            isGrounded = true;
            if (!wasGrounded)
            {
                canJump = true;
                Debug.Log($"����: canJump�� true�� ������. ������ ������Ʈ ��: {colliders.Length}");
            }
        }
        else
        {
            isGrounded = false;
            canJump = false;
            Debug.Log("����: canJump�� false�� ������");
        }

        // Animator�� isGrounded �Ķ���� ������Ʈ
        animator.SetBool("isGrounded", isGrounded);
    }

    /// <summary>
    /// ����� �Է� ó��
    /// </summary>
    private void HandleInput()
    {
        // ���� �߿��� �ٸ� �Է� ����
        if (currentState == State.Jumping) return;

        // �̵� ���� ���
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputZ = Input.GetAxisRaw("Vertical");
        movementInput = CalculateMovementDirection(inputX, inputZ);

        // �޸��� �Է� Ȯ��
        bool isRunning = Input.GetKey(runKey) && movementInput.sqrMagnitude > moveThreshold;

        // ���� �ӵ� ����
        currentSpeed = isRunning ? runSpeed : moveSpeed;

        // ���� �Է� ó��
        if (Input.GetButtonDown("Jump") && isGrounded && canJump && currentState != State.Jumping)
        {
            Debug.Log("Jump ��ư�� ����");
            TransitionToState(State.Jumping);
            return;
        }

        // �̵�/���� ���� ��ȯ
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

        // Animator�� Speed �Ķ���� ������Ʈ
        float speed = movementInput.magnitude * currentSpeed;
        animator.SetFloat("Speed", speed);
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
                // ���� ���� ����
                break;

            case State.Jumping:
                // ���� �� �ٴڿ� ������ Moving/Idle ��ȯ
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
    /// ���º� ���� ó��
    /// </summary>
    private void HandlePhysics()
    {
        switch (currentState)
        {
            case State.Idle:
                // Idle ���¸� XZ �ӵ� 0���� ���������� ����
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
                // ���� �߿��� ���� �̵�
                ApplyAirControl();
                Debug.Log($"HandlePhysics: Jumping Velocity = {rb.velocity}");
                break;
        }
    }

    /// <summary>
    /// ���� �̵� ó�� (XZ�� ����, Y�� �߷�/���� ����)
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
    /// ���� ���� ����
    /// </summary>
    private void ApplyAirControl()
    {
        Vector3 airMove = movementInput * currentSpeed * 0.5f; // ���� �̵� �ӵ� ����
        Vector3 newVelocity = new Vector3(airMove.x, rb.velocity.y, airMove.z);
        rb.velocity = Vector3.Lerp(rb.velocity, newVelocity, Time.fixedDeltaTime * 5f);

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
        Debug.Log($"State ��ȯ: {currentState} -> {newState}");

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
    /// ���� ���� ó��
    /// </summary>
    private void ExitCurrentState()
    {
        // Animator�� Speed �Ķ���ʹ� HandleInput���� �̹� �����ϹǷ� ���� ó�� ���ʿ�
        // �ʿ� �� �߰����� ���� ���� ������ ���⿡ �ۼ�
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
    /// ���� ����
    /// </summary>
    private void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z); // ���� Y �ӵ� �ʱ�ȭ
        rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
        isGrounded = false;
        canJump = false;  // ���� ������ �ٽ� ���� �Ұ�
        Debug.Log("Jump ����: Y �ӵ� ���� �� canJump False");
    }

    /// <summary>
    /// (�ɼ�) Gizmos �ð�ȭ
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (ankleTransform == null)
            return;

        // OverlapSphere �ð�ȭ
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Vector3 sphereOrigin = ankleTransform.position;
        float sphereRadius = groundCheckRadius;
        Gizmos.DrawWireSphere(sphereOrigin, sphereRadius);

        // ���� �÷��̾� ��ġ �ð�ȭ
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.02f);
    }
}

// �߰� �ϼ�