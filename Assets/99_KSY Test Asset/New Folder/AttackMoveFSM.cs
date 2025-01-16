//using UnityEngine;

//public class AttackMoveFSM
//{
//    private enum State { Idle, Moving, Attacking, Die }
//    private State currentState;
//    private Rigidbody rb;
//    private Animator animator;
//    private Transform cameraTransform;

//    public AttackMoveFSM(Rigidbody rb, Animator animator)
//    {
//        this.rb = rb;
//        this.animator = animator;
//        this.cameraTransform = Camera.main.transform;
//        currentState = State.Idle;
//    }

//    /// <summary>
//    /// 이동 입력을 처리하고 상태를 전환합니다.
//    /// </summary>
//    /// <param name="input">이동 입력 벡터</param>
//    public void HandleMove(Vector2 input)
//    {
//        Debug.Log($"HandleMove 호출됨. 입력값: {input}");

//        if (currentState == State.Die) return;

//        float moveSpeed = PlayerData.Instance.moveSpeed;

//        if (input.sqrMagnitude > 0)
//        {
//            if (currentState != State.Moving)
//            {
//                TransitionToState(State.Moving);
//            }

//            Vector2 normalizedInput = input.normalized;

//            Vector3 camForward = cameraTransform.forward;
//            Vector3 camRight = cameraTransform.right;
//            camForward.y = 0f;
//            camRight.y = 0f;
//            camForward.Normalize();
//            camRight.Normalize();

//            Vector3 movementDirection = camForward * normalizedInput.y + camRight * normalizedInput.x;
//            rb.velocity = movementDirection * moveSpeed;

//            Quaternion targetRotation = Quaternion.LookRotation(movementDirection);
//            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.deltaTime * 10f);

//            animator.SetFloat("Speed", 1f);
//            Debug.Log("Speed 파라미터를 1로 설정.");
//        }
//        else
//        {
//            if (currentState != State.Idle)
//            {
//                TransitionToState(State.Idle);
//            }

//            rb.velocity = Vector3.zero;
//            animator.SetFloat("Speed", 0f);
//            Debug.Log("Speed 파라미터를 0으로 설정.");
//        }
//    }

//    /// <summary>
//    /// 공격 입력을 처리하고 상태를 전환합니다.
//    /// </summary>
//    /// <param name="comboIndex">콤보 인덱스</param>
//    public void HandleAttack(int comboIndex)
//    {
//        if (currentState == State.Die) return;

//        TransitionToState(State.Attacking);

//        switch (comboIndex)
//        {
//            case 1:
//                animator.SetTrigger("Attack1");
//                break;
//            case 2:
//                animator.SetTrigger("Attack2");
//                break;
//            case 3:
//                animator.SetTrigger("Attack3");
//                break;
//            default:
//                Debug.LogWarning("잘못된 콤보 인덱스입니다.");
//                break;
//        }

//        rb.velocity = Vector3.zero;
//    }

//    /// <summary>
//    /// 사망 처리를 하고 상태를 전환합니다.
//    /// </summary>
//    public void HandleDie()
//    {
//        if (currentState == State.Die) return;

//        TransitionToState(State.Die);
//        rb.velocity = Vector3.zero;
//        rb.angularVelocity = Vector3.zero;
//    }

//    /// <summary>
//    /// 공격 애니메이션이 끝난 후 상태를 Idle로 리셋합니다.
//    /// </summary>
//    public void ResetToIdle()
//    {
//        if (currentState == State.Die) return;

//        Vector2 currentInput = GetCurrentInput();
//        if (currentInput.sqrMagnitude > 0)
//        {
//            HandleMove(currentInput);
//        }
//        else
//        {
//            TransitionToState(State.Idle);
//        }
//    }

//    /// <summary>
//    /// 현재 입력을 가져오는 메서드. 필요에 따라 구현.
//    /// </summary>
//    /// <returns>현재 이동 입력 벡터</returns>
//    private Vector2 GetCurrentInput()
//    {
//        return new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
//    }

//    /// <summary>
//    /// 상태를 전환하고 필요한 동작을 수행합니다.
//    /// </summary>
//    /// <param name="newState">전환할 새로운 상태</param>
//    private void TransitionToState(State newState)
//    {
//        if (currentState == newState) return;

//        Debug.Log($"상태 전환: {currentState} → {newState}");
//        currentState = newState;

//        switch (newState)
//        {
//            case State.Idle:
//                animator.SetFloat("Speed", 0f);
//                rb.velocity = Vector3.zero;
//                break;
//            case State.Moving:
//                animator.SetFloat("Speed", 1f);
//                break;
//            case State.Attacking:
//                // 공격 트리거는 HandleAttack에서 이미 설정됨
//                break;
//            case State.Die:
//                animator.SetTrigger("Die");
//                rb.velocity = Vector3.zero;
//                rb.angularVelocity = Vector3.zero;
//                break;
//        }
//    }
//}
