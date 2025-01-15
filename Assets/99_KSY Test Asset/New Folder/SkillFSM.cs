using UnityEngine;

public class SkillFSM
{
    private enum State { Ready, Casting, Cooldown }
    private State currentState;
    private Animator animator;
    private float cooldownTime;
    private float lastSkillTime;

    public SkillFSM(Animator animator, float cooldownTime)
    {
        this.animator = animator;
        this.cooldownTime = cooldownTime;
        currentState = State.Ready;
    }

    public void HandleSkill()
    {
        if (currentState != State.Ready) return;

        currentState = State.Casting;
        animator.SetTrigger("Skill");
        lastSkillTime = Time.time;
        currentState = State.Cooldown;
    }

    public void UpdateCooldown()
    {
        if (currentState == State.Cooldown && Time.time - lastSkillTime >= cooldownTime)
        {
            currentState = State.Ready;
        }
    }

    public bool IsInCooldown()
    {
        return currentState == State.Cooldown;
    }
}
