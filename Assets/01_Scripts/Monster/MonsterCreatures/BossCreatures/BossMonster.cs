using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class BossMonster : MonoBehaviourPun
{
    #region 변수
    public int hp = 5000;
    public int maxHp = 5000;
    public int damage = 15;
    public float moveSpeed = 7.5f;
    public float rotationSpeed = 3f;
    public float attackSpeed = 0.7f;

    public float slashAttackRange = 4f;

    public bool chargePossible = true;
    public bool slashPossible = true;
    public bool hungerPossible = true;

    public int chargeCooltime = 5;
    public int slashCooltime = 6;
    public int hungerCooltime = 13;
    public GameObject minionPrefab;
    public int slashCount = 0;
    public BossStateType currentState;
    public Transform Target;
    public List<Player> playerList = new List<Player>();
    public List<Minion> minionList = new List<Minion>();
    public List<Transform> nestList = new List<Transform>();
    
    public GameObject hungerEffect;
    public GameObject slashEffect;
    
    private NavMeshAgent agent;
    private BossStateMachine stateMachine;
    private int playerLayer;
    private int minionLayer;
    
    [Header("드랍 아이템")]
    public List<ItemBase> bossDropItems;
    
    [HideInInspector] public Vector3 CenterPoint;
    [HideInInspector] public Animator Animator;

    private ObjectPoolManager poolManager;

    #endregion

    #region 프로퍼티
    public NavMeshAgent Agent => agent;
    public int PlayerLayer => playerLayer;
    public int MinionLayer => minionLayer;

    #endregion
    
    protected virtual void Start()
    {
        Animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        playerLayer = LayerMask.GetMask("Player");
        minionLayer = LayerMask.GetMask("Minion");
        stateMachine = new BossStateMachine(this);
        stateMachine.ChangeState(BossStateType.Idle);
        poolManager = FindObjectOfType<ObjectPoolManager>();
    }
    
    protected virtual void Update()
    {
        currentState = stateMachine.currentStateType;
        if (playerList.Count == 0 && currentState != BossStateType.Idle)
        {
            ChangeState(BossStateType.Idle);
        }

        if (((float)hp / (float)maxHp) <= 0.3f && hungerPossible)
        {
            ChangeState(BossStateType.Hunger);
        }
        stateMachine.CurrentState?.ExecuteState(this);
    }

    public void ChangeState(BossStateType newState)
    {
        print($"{gameObject.name} State change : {newState}");
        stateMachine.ChangeState(newState);
    }

    #region 애니메이션 끝날 때 호출하는 함수
    public void OnSlashAnimationEnd()
    {
        if (stateMachine.currentStateType == BossStateType.Slash)
        {
            print("slash animation end");
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
        if (stateMachine.currentStateType == BossStateType.Bite)
        {
            Target.gameObject.GetComponent<Minion>().TakeDamage(Target.gameObject.GetComponent<Minion>().hp);
            hp += maxHp/10;
            if (hp > maxHp)
            {
                hp = maxHp;
            }
            ChangeState(BossStateType.Idle);
        }
    }
    public void OnHungerAnimationEnd()
    {
        if (stateMachine.currentStateType == BossStateType.Hunger)
        {
            ChangeState(BossStateType.Idle);
        }
    }
    
    public void GetHitAnimationEnd()
    {
        if (stateMachine.currentStateType == BossStateType.GetHit)
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
            ChangeState(BossStateType.Die);
            DroppedLightLine(TryDropItem(bossDropItems));
        }
    }

    public void SpawnMinion()
    {
        foreach (Transform nest in nestList)
        {
            //Instantiate(minionPrefab, nest.position, nest.rotation);
            PhotonObjectPool<Minion> minionPool = poolManager.GetPool(minionPrefab.GetComponent<Minion>(), 10);
            Minion newMinion = minionPool.GetObject(nest.position, Quaternion.identity);
        }
    }

    #region 쿨타임 코루틴
    public void StartCoolDown()
    {
        if (stateMachine.currentStateType == BossStateType.Charge)
        {
            print("charge cooldown");
            StartCoroutine(ChargeCoolDown());
        }

        else if (stateMachine.currentStateType == BossStateType.Slash && slashCount == 0)
        {
            print("slash cooldown");
            StartCoroutine(SlashCoolDown());
        }

        else if (stateMachine.currentStateType == BossStateType.Hunger)
        {
            print("hunger cooldown");
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
        print("EndChargeCool");
        chargePossible = true;
    }
    
    IEnumerator HungerCoolDown()
    {
        hungerPossible = false;
        yield return new WaitForSeconds(hungerCooltime);
        print("EndHungerCool");
        hungerPossible = true;
    }
    #endregion

    #region 아이템 드랍

    private ItemBase TryDropItem(List<ItemBase> items)
    {
        ItemBase droppedItem = null;

        float dropChance = UnityEngine.Random.Range(0f, 1f);
        if (dropChance <= 0.5f)
        {
            foreach (ItemBase item in items)
            {
                float randomValue = UnityEngine.Random.Range(0f, 1f);
                if (randomValue <= item.dropPercent)
                {
                    Debug.Log($"{dropChance}의 확률로 아이템 획득!");

                    droppedItem = item;
                    Debug.Log($"아이템 {droppedItem}을 {randomValue}의 확률로 얻음!");


                    break;
                }
            }
        }

        else
        {
            Debug.Log($"{dropChance}의 확률로 아이템을 획득하지 못함");
        }

        return droppedItem;
    }

    private void DroppedLightLine(ItemBase item)
    {
        Player player = FindObjectOfType<Player>();
        GameObject itemLightLine = Instantiate(item.dropLightLine, this.transform.position, Quaternion.identity);
        player.droppedItems.Add((itemLightLine, item));
    }

    #endregion
    
    private void OnDrawGizmos()
    {
        // 도륙내기 공격 범위
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, slashAttackRange);
    }
}
