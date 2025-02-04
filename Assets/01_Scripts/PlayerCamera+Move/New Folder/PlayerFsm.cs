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

    // 현재 이동 속도
    private float currentSpeed;

    // 상태 유형
    public enum State
    {
        Idle,
        Moving,
        Attack1,
        Attack2,
        Hit,
        Die
    }

    public State currentState = State.Idle;
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

    // Player 스크립트 참조: 이동 속도, HP 관리 등 플레이어 전반의 데이터를 가져오기 위함
    private Player player;

    // FSMManager 참조 추가
    private FSMManager fsmManager;

    // 이동 및 공격 잠금 상태
    private bool isMovementLocked = false;
    private bool isAttackLocked = false;

    // 상태 변경 시 이벤트
    public event Action<FSMManager.PlayerState> OnStateChanged;

    private bool isMobile = false;

    // 현재 이동 방향 저장
    private Vector3 currentMovementDirection = Vector3.zero;

    // ===== 딕셔너리 활용한 상태 전환 관련 변수 추가 =====
    private Dictionary<State, Action> stateEnterActions;
    private Dictionary<State, Action> statePhysicsActions;
    private Dictionary<State, FSMManager.PlayerState> stateToFSMMapping;
    // =======================================================

    private void Awake()
    {
        InitializeComponents();
        InitializeCamera();
        InitializeStateDictionaries(); 
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

        if (!isDead)
        {
            HandleComboReset();
        }
    }

    private void FixedUpdate()
    {
        // pun 동기화를 위함. 지우지 마시오!! - 지원
        if (!photonView.IsMine) return;

        // 딕셔너리로 현재 상태의 물리 처리 함수 호출
        if (statePhysicsActions.ContainsKey(currentState))
        {
            statePhysicsActions[currentState]?.Invoke();
        }
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
            { State.Attack1, EnterAttack1State },
            { State.Attack2, EnterAttack2State },
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

        // 물리 처리 함수 매핑
        statePhysicsActions = new Dictionary<State, Action>
        {
            { State.Idle, () =>
                {
                    Vector3 idleVelocity = rb.velocity;
                    idleVelocity.x = Mathf.Lerp(idleVelocity.x, 0f, Time.fixedDeltaTime * 10f);
                    idleVelocity.z = Mathf.Lerp(idleVelocity.z, 0f, Time.fixedDeltaTime * 10f);
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
    }
    // ======================================================

    // 씬 로딩 시 호출되는 메서드
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InitializeComponents();
        InitializeCamera();
    }

    private void HandleMoveInput(Vector2 movement)
    {
        if (isDead || isMovementLocked) return;

        // 현재 FSM 상태 확인
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

        if (movementInput.sqrMagnitude > moveThreshold)
        {
            if (currentState != State.Moving)
                TransitionToState(State.Moving);
        }
        else
        {
            if (currentState != State.Idle)
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

        // 콤보 타이밍 체크
        if (Time.time - lastAttackTime > comboResetTime)
        {
            currentComboState = ComboState.None;
        }

        lastAttackTime = Time.time;

        // 공격 시작 시 movementInput 초기화
        movementInput = Vector3.zero;

        // 콤보 단계에 따라 공격 처리
        switch (currentComboState)
        {
            case ComboState.None:
                currentComboState = ComboState.Attack1;
                TransitionToState(State.Attack1);
                break;
            case ComboState.Attack1:
                currentComboState = ComboState.Attack2;
                TransitionToState(State.Attack2);
                break;
            case ComboState.Attack2:
                currentComboState = ComboState.None;
                break;
        }

        // 콤보 타이머 재시작
        StartCoroutine(ComboResetCoroutine());
    }

    // 콤보 타이머를 리셋하는 코루틴
    private IEnumerator ComboResetCoroutine()
    {
        yield return new WaitForSeconds(comboResetTime);
        currentComboState = ComboState.None;
    }

    // 콤보 리셋을 확인하는 함수
    private void HandleComboReset()
    {
        if (Time.time - lastAttackTime > comboResetTime &&
            currentComboState != ComboState.None)
        {
            currentComboState = ComboState.None;
        }
    }

    public void TransitionToState(State newState, bool force = false)
    {
        if (isDead && newState != State.Die && !force) return;
        if (currentState == newState) return;

        currentState = newState;

        // FSMManager.PlayerState로 변환 (딕셔너리 사용)
        if (stateToFSMMapping.ContainsKey(newState))
        {
            FSMManager.PlayerState fsmState = stateToFSMMapping[newState];
            // 상태 변경 이벤트 호출
            OnStateChanged?.Invoke(fsmState);
        }

        // 상태 진입 처리 (딕셔너리 사용)
        if (stateEnterActions.ContainsKey(newState))
        {
            stateEnterActions[newState]?.Invoke();
        }
    }

    private void MovePlayer()
    {
        Vector3 moveVelocity = movementInput * currentSpeed;
        rb.velocity = new Vector3(moveVelocity.x, rb.velocity.y, moveVelocity.z);

        if (movementInput.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movementInput);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * 10f);
        }
    }

    private void EnterIdleState()
    {
        animator.SetFloat("Speed", 0f);
        if (fsmManager != null)
        {
            fsmManager.HandlePlayerStateChanged(FSMManager.PlayerState.Idle);
        }
    }

    private void EnterMovingState()
    {
        float speed = movementInput.magnitude * currentSpeed;
        animator.SetFloat("Speed", speed);
        if (fsmManager != null)
        {
            fsmManager.HandlePlayerStateChanged(FSMManager.PlayerState.Moving);
        }
    }

    private void EnterAttack1State()
    {
        animator.SetTrigger("Attack1");
        isAttackLocked = true; // 공격 중 입력 잠금
        if (fsmManager != null)
        {
            fsmManager.HandlePlayerStateChanged(FSMManager.PlayerState.Attack1);
        }

        // 사운드 재생: SoundManager 방식을 사용
        string[] comboSoundClips = { "player_Twohandattack1", "player_Twohandattack2" };
        StartCoroutine(PlayDelayedSoundWithSoundManager(comboSoundClips, soundDelay));
    }

    private void EnterAttack2State()
    {
        animator.SetTrigger("Attack2");
        isAttackLocked = true; // 공격 중 입력 잠금
        if (fsmManager != null)
        {
            fsmManager.HandlePlayerStateChanged(FSMManager.PlayerState.Attack2);
        }

        // 사운드 재생: SoundManager 방식을 사용
        string[] comboSoundClips = { "player_Twohandattack1", "player_Twohandattack2" };
        StartCoroutine(PlayDelayedSoundWithSoundManager(comboSoundClips, soundDelay));
    }

    private void EnterHitState()
    {
        animator.SetTrigger("Hit");
        if (fsmManager != null)
        {
            fsmManager.HandlePlayerStateChanged(FSMManager.PlayerState.Hit);
        }
    }

    private void EnterDieState()
    {
        isDead = true;
        animator.SetTrigger("Die");
        Invoke("DisablePlayer", dieAnimationDuration);
        if (fsmManager != null)
        {
            fsmManager.HandlePlayerStateChanged(FSMManager.PlayerState.Die);
        }
    }

    private void DisablePlayer()
    {
        gameObject.SetActive(false);
    }

    // 공격을 받았을 때 호출
    public void TakeDamage()
    {
        if (isDead || currentState == State.Hit) return;
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
        switch (animationName)
        {
            case "Attack1":
            case "Attack2":
                if (fsmManager != null)
                {
                    fsmManager.HandlePlayerStateChanged(FSMManager.PlayerState.Idle);
                }
                isAttackLocked = false; // 공격 종료 시 입력 잠금 해제
                break;
            case "Hit":
                if (fsmManager != null)
                {
                    fsmManager.HandlePlayerStateChanged(FSMManager.PlayerState.Idle);
                }
                break;
            case "Rush":
            case "Parry":
            case "Skill1":
            case "Skill2":
                if (fsmManager != null)
                {
                    fsmManager.HandlePlayerStateChanged(FSMManager.PlayerState.Idle);
                }
                break;
            case "Die":
                Debug.Log($"[PlayerFsm] {animationName} 애니메이션 종료 후 별도의 처리가 필요합니다.");
                break;
            default:
                Debug.LogWarning($"[PlayerFsm] 알 수 없는 애니메이션 종료: {animationName}");
                break;
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
