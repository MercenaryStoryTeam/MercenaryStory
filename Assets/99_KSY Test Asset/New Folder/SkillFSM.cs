using UnityEngine;

public class SkillFSM
{
    public enum State { Ready, Casting, Cooldown }
    private State currentState;
    private Animator animator;
    private float cooldownTime;
    private float lastSkillTime;
    private string animationTrigger;

    public SkillFSM(Animator animator, float cooldownTime, string animationTrigger)
    {
        this.animator = animator;
        this.cooldownTime = cooldownTime;
        this.animationTrigger = animationTrigger;
        currentState = State.Ready;
    }

    public void HandleSkill()
    {
        if (currentState != State.Ready) return;

        currentState = State.Casting;
        animator.SetTrigger(animationTrigger);
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

    public string GetAnimationTrigger()
    {
        return animationTrigger;
    }
}
