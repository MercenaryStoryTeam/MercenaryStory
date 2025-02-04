using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Cinemachine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody), typeof(Animator), typeof(Player))]
public class PlayerFsm : MonoBehaviourPun
{
    [HideInInspector] public Transform cameraTransform;

    [Header("Die 애니메이션 재생 시간")]
    public float dieAnimationDuration = 2f;

    [Header("콤보 타이머")]
    public float comboResetTime = 1.0f;

    [Header("기본 공격 소리 딜레이 조정")]
    public float soundDelay = 0.8f;

    private Rigidbody rb;
    private Animator animator;
    private Vector3 movementInput;
    private const float moveThreshold = 0.05f;
    private const float LERP_SPEED = 10f; // 이동 보간 속도 상수

    // 현재 이동 속도
    private float currentSpeed;

    // 기존에는 PlayerFsm 내부에서 상태를 관리했으나 FSMManager로 중앙 관리하므로 제거합니다.
    // public State currentState = State.Idle;

    // 상태 유형 (PlayerFsm 내부에서 상태별 동작을 구분하기 위한 용도)
    public enum State
    {
        Idle,
        Moving,
        Attack1,
        Attack2,
        Hit,
        Die
    }

    private bool isDead = false;

    // 콤보 시스템
    private enum ComboState
    {
        None,
        Attack1,
        Attack2
    }

    private ComboState currentComboState = ComboState.None;
    private float lastAttackTime = 0f;

    // 공격 리셋 코루틴 참조 (코루틴 중복 실행 관리를 위해 추가)
    private Coroutine comboResetCoroutine;

    // Player 스크립트 참조: 이동 속도, HP 관리 등 플레이어 전반의 데이터를 가져오기 위함
    private Player player;

    // FSMManager 참조 추가
    private FSMManager fsmManager;

    // 이동 및 공격 잠금 상태
    private bool isMovementLocked = false;
    private bool isAttackLocked = false;

    // (이전에는 이벤트로 상태 변경을 알렸으나 중앙 관리하므로 직접 호출합니다.)
    // public event Action<FSMManager.PlayerState> OnStateChanged;

    private bool isMobile = false;

    // 현재 이동 방향 저장
    private Vector3 currentMovementDirection = Vector3.zero;

    // ===== 딕셔너리 활용한 상태 전환 관련 변수 추가 =====
    private Dictionary<State, Action> stateEnterActions;
    private Dictionary<State, Action> statePhysicsActions;
    // 기존 PlayerFsm.State -> FSMManager.PlayerState 매핑 딕셔너리
    private Dictionary<State, FSMManager.PlayerState> stateToFSMMapping;
    // =======================================================

    // FixedUpdate 최적화를 위해 현재 상태의 물리 처리 함수를 저장
    private Action currentPhysicsAction;

    // 애니메이션 종료 처리 딕셔너리
    private Dictionary<string, Action> animationEndActions;

    // 콤보 단계 전환을 위한 딕셔너리
    private Dictionary<ComboState, Action> comboTransitionActions;

    // 애니메이션 이름 상수화
    private const string ANIM_ATTACK1 = "Attack1";
    private const string ANIM_ATTACK2 = "Attack2";
    private const string ANIM_HIT = "Hit";
    private const string ANIM_RUSH = "Rush";
    private const string ANIM_PARRY = "Parry";
    private const string ANIM_SKILL1 = "Skill1";
    private const string ANIM_SKILL2 = "Skill2";
    private const string ANIM_DIE = "Die";

    private void Awake()
    {
        InitializeComponents();
        InitializeCamera();
        InitializeStateDictionaries();
        InitializeAnimationEndActions();
        InitializeComboTransitionActions();
    }

    private void Start()
    {
        if (photonView.IsMine)
        {
            gameObject.AddComponent<AudioListener>();
        }
        if (FirebaseManager.Instance.CurrentUserData.user_Name == gameObject.name)
        {
            GameManager.Instance.currentPlayerFsm = this;
        }

        if (PhotonNetwork.IsMasterClient && GameManager.Instance != null)
        {
            if (FirebaseManager.Instance.CurrentPartyData.party_Owner.user_Name == gameObject.name)
            {
                GameManager.Instance.hostPlayerFsm = this;
            }
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        PlayerInputManager.OnMoveInput += HandleMoveInput;
        PlayerInputManager.OnAttackInput += HandleAttackInput;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        PlayerInputManager.OnMoveInput -= HandleMoveInput;
        PlayerInputManager.OnAttackInput -= HandleAttackInput;
    }

    private void Update()
    {
        // pun 동기화를 위함. 지우지 마시오!! - 지원
        if (!photonView.IsMine) return;

        // 콤보 리셋은 코루틴으로 통합하였으므로 Update()에서 별도 처리 제거함.
    }

    private void FixedUpdate()
    {
        // pun 동기화를 위함. 지우지 마시오!! - 지원
        if (!photonView.IsMine) return;

        // 최적화를 위해 currentPhysicsAction을 직접 호출 (상태 변경 시 업데이트)
        currentPhysicsAction?.Invoke();
    }

    // 컴포넌트 초기화 메서드
    private void InitializeComponents()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        player = GetComponent<Player>();

        if (rb == null)
        {
            Debug.LogError("PlayerFsm는 Rigidbody 컴포넌트를 필요로 합니다.");
        }

        if (animator == null)
        {
            Debug.LogError("PlayerFsm는 Animator 컴포넌트를 필요로 합니다.");
        }

        if (player == null)
        {
            Debug.LogError("PlayerFsm의 GameObject에 Player 스크립트가 없습니다.");
            enabled = false;
        }

        if (rb != null)
        {
            if (rb.isKinematic)
            {
                Debug.LogWarning("Rigidbody is Kinematic.");
            }

            if (!rb.useGravity)
            {
                Debug.LogWarning("Rigidbody useGravity가 비활성화되어 있습니다.");
            }

            // 물리적 이동의 부드러움을 위해 Rigidbody Interpolation 활성화
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        // FSMManager 스크립트 가져오기
        fsmManager = GetComponent<FSMManager>();
        if (fsmManager == null)
        {
            Debug.LogError("[PlayerFsm] FSMManager 스크립트를 찾을 수 없습니다.");
        }

#if UNITY_ANDROID || UNITY_IOS
        isMobile = true;
#else
        isMobile = false;
#endif
    }

    // 카메라 초기화 메서드
    private void InitializeCamera()
    {
        if (cameraTransform == null)
        {
            CinemachineVirtualCamera vCam = FindObjectOfType<CinemachineVirtualCamera>();
            if (vCam)
            {
                cameraTransform = vCam.transform;
                VirtualCameraController vCamController = vCam.GetComponent<VirtualCameraController>();
                if (vCamController != null)
                {
                    vCamController.enabled = true;
                }
            }
            else
            {
                Debug.LogError("씬에 Virtual Camera가 없습니다. 카메라를 수동으로 할당하세요.");
            }
        }

        if (animator != null)
        {
            animator.applyRootMotion = false;
        }
    }

    // 상태 전환 관련 딕셔너리 초기화
    private void InitializeStateDictionaries()
    {
        // 상태 진입 함수 매핑
        stateEnterActions = new Dictionary<State, Action>
        {
            { State.Idle, EnterIdleState },
            { State.Moving, EnterMovingState },
            { State.Attack1, () => HandleAttackState(ANIM_ATTACK1, FSMManager.PlayerState.Attack1) },
            { State.Attack2, () => HandleAttackState(ANIM_ATTACK2, FSMManager.PlayerState.Attack2) },
            { State.Hit, EnterHitState },
            { State.Die, EnterDieState }
        };

        // FSMManager의 PlayerState로의 매핑
        stateToFSMMapping = new Dictionary<State, FSMManager.PlayerState>
        {
            { State.Idle, FSMManager.PlayerState.Idle },
            { State.Moving, FSMManager.PlayerState.Moving },
            { State.Attack1, FSMManager.PlayerState.Attack1 },
            { State.Attack2, FSMManager.PlayerState.Attack2 },
            { State.Hit, FSMManager.PlayerState.Hit },
            { State.Die, FSMManager.PlayerState.Die }
        };

        // 물리 처리 함수 매핑 (MovePlayer()에서 MovePosition 사용)
        statePhysicsActions = new Dictionary<State, Action>
        {
            { State.Idle, () =>
                {
                    // 이동하지 않을 때는 속도를 보간 처리
                    Vector3 idleVelocity = rb.velocity;
                    idleVelocity.x = Mathf.Lerp(idleVelocity.x, 0f, Time.fixedDeltaTime * LERP_SPEED);
                    idleVelocity.z = Mathf.Lerp(idleVelocity.z, 0f, Time.fixedDeltaTime * LERP_SPEED);
                    rb.velocity = idleVelocity;
                }
            },
            { State.Moving, MovePlayer },
            { State.Attack1, () => {} },
            { State.Attack2, () => {} },
            { State.Hit, () => {} },
            { State.Die, () =>
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
            }
        };

        // 초기 상태의 물리 처리 함수 설정
        // 중앙 상태 관리는 FSMManager.currentState에서 이루어지므로 currentPhysicsAction은 나중에 TransitionToState()에서 업데이트됩니다.
    }
    // ======================================================

    // 애니메이션 종료 처리용 딕셔너리 초기화
    private void InitializeAnimationEndActions()
    {
        animationEndActions = new Dictionary<string, Action>
        {
            { ANIM_ATTACK1, () => {
                    InvokeFsmStateChange(FSMManager.PlayerState.Idle);
                    isAttackLocked = false;
                }
            },
            { ANIM_ATTACK2, () => {
                    InvokeFsmStateChange(FSMManager.PlayerState.Idle);
                    isAttackLocked = false;
                }
            },
            { ANIM_HIT, () => {
                    InvokeFsmStateChange(FSMManager.PlayerState.Idle);
                }
            },
            { ANIM_RUSH, () => {
                    InvokeFsmStateChange(FSMManager.PlayerState.Idle);
                }
            },
            { ANIM_PARRY, () => {
                    InvokeFsmStateChange(FSMManager.PlayerState.Idle);
                }
            },
            { ANIM_SKILL1, () => {
                    InvokeFsmStateChange(FSMManager.PlayerState.Idle);
                }
            },
            { ANIM_SKILL2, () => {
                    InvokeFsmStateChange(FSMManager.PlayerState.Idle);
                }
            },
            { ANIM_DIE, () => {
                    Debug.Log($"[PlayerFsm] Die 애니메이션 종료 후 별도의 처리가 필요합니다.");
                }
            }
        };
    }

    // 콤보 단계 전환용 딕셔너리 초기화
    private void InitializeComboTransitionActions()
    {
        comboTransitionActions = new Dictionary<ComboState, Action>
        {
            { ComboState.None, () => {
                    currentComboState = ComboState.Attack1;
                    TransitionToState(State.Attack1);
                }
            },
            { ComboState.Attack1, () => {
                    currentComboState = ComboState.Attack2;
                    TransitionToState(State.Attack2);
                }
            },
            { ComboState.Attack2, () => {
                    currentComboState = ComboState.None;
                }
            }
        };
    }

    // 씬 로딩 시 호출되는 메서드
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InitializeComponents();
        InitializeCamera();
    }

    private void HandleMoveInput(Vector2 movement)
    {
        if (isDead || isMovementLocked) return;

        // 현재 FSM 상태 확인 (FSMManager에서 중앙 관리)
        if (fsmManager == null)
        {
            Debug.LogError("[PlayerFsm] FSMManager 참조가 없습니다.");
            return;
        }

        // 이동 입력은 Idle, Moving 상태에서만 처리
        if (fsmManager.currentState != FSMManager.PlayerState.Idle &&
            fsmManager.currentState != FSMManager.PlayerState.Moving)
        {
            return;
        }

        float inputX = movement.x;
        float inputZ = movement.y;

        movementInput = CalculateMovementDirection(inputX, inputZ);

        currentSpeed = player != null ? player.moveSpeed : 0f;

        currentMovementDirection = movementInput.normalized;

        // FSMManager의 currentState와 비교할 필요는 없으므로, 단순히 입력에 따라 이동/정지 애니메이션을 조절합니다.
        if (movementInput.sqrMagnitude > moveThreshold)
        {
            TransitionToState(State.Moving);
        }
        else
        {
            TransitionToState(State.Idle);
        }

        float normalizedSpeed = (movementInput.magnitude * currentSpeed) /
                            (player != null ? player.moveSpeed : 1f);
        animator.SetFloat("Speed", normalizedSpeed);
    }

    // 플레이어가 공격을 시도할 때 호출되는 콤보 처리 함수
    private void HandleAttackInput()
    {
        // 사망이거나, 공격 잠금 중이면 공격 불가
        if (isDead || isAttackLocked)
        {
            return;
        }

        // 현재 FSM 상태 확인
        if (fsmManager == null)
        {
            Debug.LogError("[PlayerFsm] FSMManager 참조가 없습니다.");
            return;
        }

        // 공격 입력은 Idle 상태에서만 처리
        if (fsmManager.currentState != FSMManager.PlayerState.Idle)
        {
            return;
        }

        // 콤보 타이밍 체크: 마지막 공격 이후 지정 시간(comboResetTime)이 지났으면 콤보를 리셋합니다.
        if (Time.time - lastAttackTime > comboResetTime)
        {
            currentComboState = ComboState.None;
        }

        // 마지막 공격 시간 업데이트
        lastAttackTime = Time.time;

        // 공격 시작 시 movementInput 초기화
        movementInput = Vector3.zero;

        // 콤보 단계에 따른 전환
        if (comboTransitionActions.ContainsKey(currentComboState))
        {
            comboTransitionActions[currentComboState].Invoke();
        }

        // 코루틴 중복 실행 관리: 실행 중인 콤보 리셋 코루틴이 있다면 중단 후 새로 시작
        if (comboResetCoroutine != null)
        {
            StopCoroutine(comboResetCoroutine);
        }
        comboResetCoroutine = StartCoroutine(ComboResetCoroutine());
    }

    // 콤보 타이머를 리셋하는 코루틴 (코루틴만 사용하도록 통합)
    private IEnumerator ComboResetCoroutine()
    {
        yield return new WaitForSeconds(comboResetTime);
        currentComboState = ComboState.None;
        comboResetCoroutine = null; // 코루틴 종료 후 null로 초기화
    }

    // fsmManager의 null 체크 후 상태 전환 호출을 위한 유틸 함수 (중앙 관리용)
    private void InvokeFsmStateChange(FSMManager.PlayerState state)
    {
        if (fsmManager != null)
        {
            fsmManager.HandlePlayerStateChanged(state);
        }
        else
        {
            Debug.LogWarning("[PlayerFsm] FSMManager 참조가 없습니다. 상태 전환을 호출할 수 없습니다.");
        }
    }

    // 중앙 상태 관리를 위해 PlayerFsm 내부의 상태 변수는 제거하고 FSMManager.currentState를 기준으로 합니다.
    public void TransitionToState(State newState, bool force = false)
    {
        // FSMManager의 현재 상태가 Die이면 다른 상태로 전환하지 않습니다.
        if (fsmManager.currentState == FSMManager.PlayerState.Die && newState != State.Die && !force) return;

        // 현재 FSMManager의 상태와 새 상태가 같다면 전환하지 않습니다.
        if (stateToFSMMapping.ContainsKey(newState) && fsmManager.currentState == stateToFSMMapping[newState])
            return;

        // 상태 진입 처리
        if (stateEnterActions.ContainsKey(newState))
        {
            stateEnterActions[newState]?.Invoke();
        }

        // FSMManager의 상태를 업데이트 (중앙 관리)
        if (stateToFSMMapping.ContainsKey(newState))
        {
            FSMManager.PlayerState newFsmState = stateToFSMMapping[newState];
            fsmManager.HandlePlayerStateChanged(newFsmState);
        }
        else
        {
            Debug.LogWarning("[PlayerFsm] stateToFSMMapping에 해당 상태가 없습니다.");
        }

        // FixedUpdate 최적화를 위해 현재 상태의 물리 처리 함수를 currentPhysicsAction에 업데이트
        if (statePhysicsActions.ContainsKey(newState))
        {
            currentPhysicsAction = statePhysicsActions[newState];
        }
        else
        {
            currentPhysicsAction = null;
        }
    }

    // MovePlayer() 메서드를 수정하여 Rigidbody.MovePosition() 사용 및 Time.fixedDeltaTime 보정
    private void MovePlayer()
    {
        // 이동할 위치 계산 (y축은 Gravity에 의해 제어하므로 0으로 고정)
        Vector3 moveVelocity = movementInput * currentSpeed;
        Vector3 targetPosition = rb.position + new Vector3(moveVelocity.x, 0f, moveVelocity.z) * Time.fixedDeltaTime;

        // MovePosition을 통해 이동 (물리적 충돌과의 자연스러운 상호 작용)
        rb.MovePosition(targetPosition);

        // 회전 처리 (방향 전환)
        if (movementInput.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movementInput);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * LERP_SPEED);
        }
    }

    private void EnterIdleState()
    {
        animator.SetFloat("Speed", 0f);
        InvokeFsmStateChange(FSMManager.PlayerState.Idle);
    }

    private void EnterMovingState()
    {
        float speed = movementInput.magnitude * currentSpeed;
        animator.SetFloat("Speed", speed);
        InvokeFsmStateChange(FSMManager.PlayerState.Moving);
    }

    // 공통 공격 상태 처리 함수 (TriggerName과 FSM 상태를 매개변수로 받음)
    private void HandleAttackState(string triggerName, FSMManager.PlayerState state)
    {
        animator.SetTrigger(triggerName);
        isAttackLocked = true; // 공격 중 입력 잠금
        InvokeFsmStateChange(state);

        string[] comboSoundClips = { "player_Twohandattack1", "player_Twohandattack2" };
        StartCoroutine(PlayDelayedSoundWithSoundManager(comboSoundClips, soundDelay));
    }

    private void EnterHitState()
    {
        animator.SetTrigger(ANIM_HIT);
        InvokeFsmStateChange(FSMManager.PlayerState.Hit);
    }

    private void EnterDieState()
    {
        isDead = true;
        animator.SetTrigger(ANIM_DIE);
        Invoke("DisablePlayer", dieAnimationDuration);
        InvokeFsmStateChange(FSMManager.PlayerState.Die);
    }

    private void DisablePlayer()
    {
        gameObject.SetActive(false);
    }

    // 공격을 받았을 때 호출
    public void TakeDamage()
    {
        if (isDead || fsmManager.currentState == FSMManager.PlayerState.Hit) return;
        TransitionToState(State.Hit);
    }

    public void Die()
    {
        if (!isDead)
        {
            TransitionToState(State.Die);
        }
    }

    // Attack 및 스킬 애니메이션 종료 이후 처리
    public void OnAnimationEnd(string animationName)
    {
        if (animationEndActions != null && animationEndActions.ContainsKey(animationName))
        {
            animationEndActions[animationName].Invoke();
        }
        else
        {
            Debug.LogWarning($"[PlayerFsm] 알 수 없는 애니메이션 종료: {animationName}");
        }
    }

    private Vector3 CalculateMovementDirection(float inputX, float inputZ)
    {
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        return (camForward * inputZ + camRight * inputX).normalized;
    }

    // 이동 및 공격을 잠금
    public void LockMovementAndAttack()
    {
        isMovementLocked = true;
        isAttackLocked = true;
        animator.SetFloat("Speed", 0f);
        rb.velocity = Vector3.zero;

        TransitionToState(State.Idle);
    }

    // 이동 및 공격을 잠금 해제
    public void UnlockMovementAndAttack()
    {
        isMovementLocked = false;
        isAttackLocked = false;
    }

    // 공격 입력만 잠금
    public void LockAttack()
    {
        isAttackLocked = true;
    }

    // 공격 입력 잠금 해제
    public void UnlockAttack()
    {
        isAttackLocked = false;
    }

    // 씬 이동을 위함 -지원
    public void MoveMembersToRoom(string sceneName)
    {
        if (FirebaseManager.Instance.CurrentPartyData.party_Owner.user_Id ==
            FirebaseManager.Instance.CurrentUserData.user_Id)
        {
            foreach (UserData member in FirebaseManager.Instance.CurrentPartyData.party_Members)
            {
                foreach (Photon.Realtime.Player photonPlayer in PhotonNetwork.PlayerList)
                {
                    if (photonPlayer.NickName == member.user_Name)
                    {
                        photonView.RPC("RPC_MoveToScene", photonPlayer, sceneName);
                        break;
                    }
                }
            }
        }
    }

    [PunRPC]
    private void RPC_MoveToScene(string sceneName)
    {
        if (FirebaseManager.Instance.CurrentPartyData.party_Owner.user_Id ==
            FirebaseManager.Instance.CurrentUserData.user_Id)
        {
            FirebaseManager.Instance.UploadPartyDataToLoadScene(sceneName);
        }

        FirebaseManager.Instance.UpdateCurrentPartyDataAndLoadScene(sceneName);
    }

    public void InstantiatePlayerPrefabs()
    {
        GameManager.Instance.ChangeScene(GameManager.Instance.CurrentScene + 1);
        photonView.RPC("RPC_InstantiatePlayerPrefabs", RpcTarget.Others);
    }

    [PunRPC]
    private void RPC_InstantiatePlayerPrefabs()
    {
        GameManager.Instance.ChangeScene(GameManager.Instance.CurrentScene + 1);
        GameManager.Instance.PlayerSpawn();
    }

    public void ReturnToTown()
    {
        if (FirebaseManager.Instance.CurrentPartyData.party_Owner.user_Id ==
            FirebaseManager.Instance.CurrentUserData.user_Id)
        {
            photonView.RPC("RPC_ReturnToTown", RpcTarget.All);
        }
        else
        {
            RPC_ReturnToTown();
        }
    }

    [PunRPC]
    private void RPC_ReturnToTown()
    {
        player.currentHp = player.maxHp;
        FirebaseManager.Instance.CurrentUserData.UpdateUserData(hp: player.currentHp);
        FirebaseManager.Instance.UploadCurrentUserData();
        GameManager.Instance.ChangeScene(1);
        ServerManager.LeaveAndLoadScene("LJW_TownScene");
    }

    // 사운드 재생 코루틴 (SoundManager 방식을 사용)
    private IEnumerator PlayDelayedSoundWithSoundManager(string[] soundClips, float delay)
    {
        if (soundClips == null || soundClips.Length == 0)
        {
            Debug.LogError("[PlayerFsm] soundClips 배열이 비어 있습니다!");
            yield break;
        }

        yield return new WaitForSeconds(delay);

        int randomIndex = UnityEngine.Random.Range(0, soundClips.Length);
        string soundName = soundClips[randomIndex];
        Debug.Log($"[PlayerFsm] SoundManager를 통해 사운드 재생 시도: {soundName}");
        SoundManager.Instance?.PlaySFX(soundName, gameObject);
    }

    // 플레이어의 현재 이동 방향을 반환하는 메서드
    public Vector3 GetCurrentMovementDirection()
    {
        return currentMovementDirection;
    }
}
