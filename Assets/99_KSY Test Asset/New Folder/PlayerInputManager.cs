using UnityEngine;

public class PlayerInputManager : MonoBehaviour
{
    public static System.Action<Vector2> OnMoveInput;
    public static System.Action OnAttackInput;
    public static System.Action OnSkillInput;

    void Update()
    {
        Vector2 movement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (movement.sqrMagnitude > 0)
        {
            OnMoveInput?.Invoke(movement);
        }

        if (Input.GetMouseButtonDown(0)) // 일반 공격 입력
        {
            OnAttackInput?.Invoke();
        }

        if (Input.GetButtonDown("Jump")) // 스페이스 입력
        {
            OnSkillInput?.Invoke();
        }
    }
}
