using UnityEngine;

public class AttackMoveFSM
{
    private enum State { Idle, Moving, Attacking, Die }
    private State currentState;
    private Rigidbody rb;
    private Animator animator;
    private Transform cameraTransform;

    public AttackMoveFSM(Rigidbody rb, Animator animator)
    {
        this.rb = rb;
        this.animator = animator;
        this.cameraTransform = Camera.main.transform;
        currentState = State.Idle;
    }

    public void HandleMove(Vector2 input)
    {
        if (currentState == State.Die) return;

        float moveSpeed = PlayerData.Instance.moveSpeed;

        if (input.sqrMagnitude > 0)
        {
            currentState = State.Moving;
            Vector2 normalizedInput = input.normalized;

            Vector3 camForward = cameraTransform.forward;
            Vector3 camRight = cameraTransform.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 movementDirection = camForward * normalizedInput.y + camRight * normalizedInput.x;
            rb.velocity = movementDirection * moveSpeed;

            Quaternion targetRotation = Quaternion.LookRotation(movementDirection);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.deltaTime * 10f);

            animator.SetFloat("Speed", 1f);
        }
        else
        {
            currentState = State.Idle;
            rb.velocity = Vector3.zero;
            animator.SetFloat("Speed", 0);
        }
    }

    public void HandleAttack(int comboIndex)
    {
        if (currentState == State.Die) return;

        currentState = State.Attacking;

        switch (comboIndex)
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

    public void HandleDie()
    {
        if (currentState == State.Die) return;

        currentState = State.Die;
        animator.SetTrigger("Die");
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    public void ResetToIdle()
    {
        if (currentState == State.Die) return;

        currentState = State.Idle;
        animator.SetFloat("Speed", 0);
    }
}
