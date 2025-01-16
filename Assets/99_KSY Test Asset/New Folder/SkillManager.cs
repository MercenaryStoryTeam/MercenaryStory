using UnityEngine;
using System.Collections;

public class SkillManager : MonoBehaviour
{
    private Animator animator;
    private Rigidbody rb;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        if (animator == null)
        {
            Debug.LogError("Animator 컴포넌트가 없습니다.");
        }
    }

    private void OnEnable()
    {
        PlayerInputManager.OnSkillInput += ExecuteRush;
        PlayerInputManager.OnRightClickInput += ExecuteParry;
        PlayerInputManager.OnShiftLeftClickInput += ExecuteSkill1;
        PlayerInputManager.OnShiftRightClickInput += ExecuteSkill2;
    }

    private void OnDisable()
    {
        PlayerInputManager.OnSkillInput -= ExecuteRush;
        PlayerInputManager.OnRightClickInput -= ExecuteParry;
        PlayerInputManager.OnShiftLeftClickInput -= ExecuteSkill1;
        PlayerInputManager.OnShiftRightClickInput -= ExecuteSkill2;
    }

    private void ExecuteRush()
    {
        Debug.Log("Rush 스킬 실행");
        if (animator != null)
        {
            Debug.Log("Animator에 Rush 트리거 설정");
            animator.SetTrigger("Rush");
            StartCoroutine(RushCoroutine());
        }
    }

    private void ExecuteParry()
    {
        Debug.Log("Parry 스킬 실행");
        if (animator != null)
        {
            Debug.Log("Animator에 Parry 트리거 설정");
            animator.SetTrigger("Parry");
            StartCoroutine(ParryCoroutine());
        }
    }

    private void ExecuteSkill1()
    {
        Debug.Log("Skill1 스킬 실행");
        if (animator != null)
        {
            Debug.Log("Animator에 Skill1 트리거 설정");
            animator.SetTrigger("Skill1");
            StartCoroutine(Skill1Coroutine());
        }
    }

    private void ExecuteSkill2()
    {
        Debug.Log("Skill2 스킬 실행");
        if (animator != null)
        {
            Debug.Log("Animator에 Skill2 트리거 설정");
            animator.SetTrigger("Skill2");
            StartCoroutine(Skill2Coroutine());
        }
    }

    private IEnumerator RushCoroutine()
    {
        yield return new WaitForSeconds(1f);
    }

    private IEnumerator ParryCoroutine()
    {
        yield return new WaitForSeconds(1f);
    }

    private IEnumerator Skill1Coroutine()
    {
        yield return null;
    }

    private IEnumerator Skill2Coroutine()
    {
        yield return null;
    }
}
