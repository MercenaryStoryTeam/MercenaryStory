using UnityEngine;

public class FSMManager : MonoBehaviour
{
    [Header("스킬 쿨타임")]
    [SerializeField] private float skillCooldownTime = 2f;
    [SerializeField] private float loadSceneDelay = 2f;

    private AttackMoveFSM attackMoveFSM;
    private SkillFSM skillFSM;

    private Rigidbody rb;
    private Animator animator;

    public static FSMManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        attackMoveFSM = new AttackMoveFSM(rb, animator);
        skillFSM = new SkillFSM(animator, skillCooldownTime);

        PlayerInputManager.OnMoveInput += attackMoveFSM.HandleMove;
        PlayerInputManager.OnAttackInput += attackMoveFSM.HandleAttack;
        PlayerInputManager.OnSkillInput += skillFSM.HandleSkill;
    }

    void Update()
    {
        skillFSM.UpdateCooldown();
    }

    void OnDestroy()
    {
        PlayerInputManager.OnMoveInput -= attackMoveFSM.HandleMove;
        PlayerInputManager.OnAttackInput -= attackMoveFSM.HandleAttack;
        PlayerInputManager.OnSkillInput -= skillFSM.HandleSkill;
    }

    // Die 상태 전환 메서드
    public void TriggerDie()
    {
        attackMoveFSM.HandleDie();
    }
}
