using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BossMonster : MonoBehaviour
{
    #region 변수
    public int hp = 3000;
    public int maxHp = 3000;
    public int damage = 15;
    public float moveSpeed = 7.5f;
    public float rotationSpeed = 3f;
    public float attackSpeed = 0.7f;

    public float slashDetectionRange = 10f;
    public float slashAttackRange = 7f;

    public bool chargePossible = true;
    public bool slashPossible = true;
    public bool hungerPossible = false;
    public bool bitePossible = true;

    public int chargeCooltime = 5;
    public int slashCooltime = 6;
    public int hungerCooltime = 13;

    public int slashCount = 0;

    public List<Player> playerList = new List<Player>();
    public List<Minion> minionList = new List<Minion>();
    private NavMeshAgent agent;
    private BossStateMachine stateMachine;
    private LayerMask playerLayer;
    
    public BossStateType currentState;
    [HideInInspector] public Transform TargetTransform;
    [HideInInspector] public Vector3 CenterPoint;
    [HideInInspector] public Animator Animator;

    #endregion

    #region 프로퍼티
    public NavMeshAgent Agent => agent;
    public BossStateMachine StateMachine => stateMachine;
    public LayerMask PlayerLayer => playerLayer;
    #endregion
    
    protected virtual void Start()
    {
        Animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        playerLayer = LayerMask.GetMask("Player");
        stateMachine = new BossStateMachine(this);
        stateMachine.ChangeState(BossStateType.Idle);
    }
    
    protected virtual void Update()
    {
        currentState = stateMachine.currentStateType;
        stateMachine.CurrentState?.ExecuteState(this);
    }

    public void ChangeState(BossStateType newState)
    {
        print($"{gameObject.name} State change : {newState}");
        stateMachine.ChangeState(newState);
    }

    public void RevertToExState()
    {
        print($"{gameObject.name} Revert To Ex : {stateMachine.CurrentState}");
        stateMachine.RevertToExState();
    }

    #region 애니메이션 끝날 때 호출하는 함수
    public void OnChargeAnimationEnd()
    {
        if (currentState == BossStateType.Charge)
        {
            ChangeState(BossStateType.Idle);
        }
    }
    public void OnSlashAnimationEnd()
    {
        if (currentState == BossStateType.Slash)
        {
            slashCount++;
            if (slashCount == 3)
            {
                slashCount = 0;
                ChangeState(BossStateType.Idle);
            }
            else
            {ChangeState(BossStateType.SlashChase);}
        }
    }
    public void OnBiteAnimationEnd()
    {
        if (currentState == BossStateType.Bite)
        {
            ChangeState(BossStateType.Idle);
        }
    }
    public void OnHungerAnimationEnd()
    {
        if (currentState == BossStateType.Hunger)
        {
            ChangeState(BossStateType.Idle);
        }
    }
    
    public void GetHitAnimationEnd()
    {
        if (currentState == BossStateType.GetHit)
        {
            ChangeState(BossStateType.Idle);
        }
    }
    #endregion

    public void TakeDamage(int damage)
    {
        hp -= damage;
        if (hp <= 0)
        {
            stateMachine.ChangeState(BossStateType.Die);
        }
    }

    public void StartCoolDown()
    {
        print("StartCool");
        if (currentState == BossStateType.Charge)
        {
            
            StartCoroutine(ChargeCoolDown());
        }

        else if (currentState == BossStateType.Slash)
        {
            print("StartSlashCool");
            StartCoroutine(SlashCoolDown());
        }

        else if (currentState == BossStateType.Hunger)
        {
            StartCoroutine(HungerCoolDown());
        }
    }

    IEnumerator SlashCoolDown()
    {
        
        slashPossible = false;
        yield return new WaitForSeconds(slashCooltime);
        print("EndSlashCool");
        slashPossible = true;
    }
    
    IEnumerator ChargeCoolDown()
    {
        chargePossible = false;
        yield return new WaitForSeconds(chargeCooltime);
        chargePossible = true;
    }
    
    IEnumerator HungerCoolDown()
    {
        hungerPossible = false;
        yield return new WaitForSeconds(hungerCooltime);
        hungerPossible = true;
    }
    
    private void OnDrawGizmos()
    {
        if(currentState == BossStateType.SlashChase)
        // 도륙내기 감지 범위
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, slashDetectionRange);
    
        // 도륙내기 공격 범위
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, slashAttackRange);
    }
}
