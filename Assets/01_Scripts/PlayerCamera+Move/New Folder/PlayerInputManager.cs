using System;
using UnityEngine;

// 이벤트 처리의 장점: 명확한 입력 구분 -> 애니메이터에서 중복 트리거 방지

public class PlayerInputManager : MonoBehaviour
{
    public static System.Action<Vector2> OnMoveInput;
    public static System.Action OnAttackInput;
    public static System.Action OnSkillInput;
    public static System.Action OnRightClickInput;
    public static System.Action OnShiftLeftClickInput;
    public static System.Action OnShiftRightClickInput;
    public static System.Action OnBInput;
    public static System.Action OnKInput; // K 입력 메서드 추가

    private Player player;

    private void Start()
    {
        player = GetComponent<Player>();
    }

    void Update()
    {
        Vector2 movement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (movement.sqrMagnitude > 0)
        {
            OnMoveInput?.Invoke(movement);
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                OnShiftLeftClickInput?.Invoke();
            }
            else
            {
                OnAttackInput?.Invoke();
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                OnShiftRightClickInput?.Invoke();
            }
            else
            {
                OnRightClickInput?.Invoke();
            }
        }

        if (Input.GetButtonDown("Jump"))
        {
            OnSkillInput?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            OnBInput?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.K)) 
        {
            OnKInput?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            UIManager.Instance.inventory.TryOpenInventory();
        }

        if (Input.GetKeyDown(KeyCode.O)) // 상점 테스트용
        {
            UIManager.Instance.shop.TryOpenShop();
        }
        
        player.DropItemInteraction(); // 드랍템 상호작용 메서드
    }
}

//
