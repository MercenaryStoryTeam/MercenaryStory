using UnityEngine;

public class PlayerInputManager : MonoBehaviour
{
    public static System.Action<Vector2> OnMoveInput;
    public static System.Action OnAttackInput;
    public static System.Action OnSkillInput;
    public static System.Action OnRightClickInput;
    public static System.Action OnShiftLeftClickInput;
    public static System.Action OnShiftRightClickInput;
    public static System.Action OnBInput; 

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

        if (Input.GetKeyDown(KeyCode.I))
        {
            UIManager.Instance.inventory.TryOpenInventory();
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            UIManager.Instance.shop.TryOpenShop();
        }
    }
}
