using UnityEngine;

public class PlayerInputManager : MonoBehaviour
{
    public delegate void InputEvent(Vector2 input);
    public static event InputEvent OnMoveInput;
    public static event System.Action<int> OnAttackInput; // Action<int>로 변경
    public static event System.Action OnSkillInput;

    private int comboIndex = 1; // 공격 콤보 인덱스 관리
    private float lastAttackTime;
    private float comboResetTime = 1.0f; // 콤보 초기화 시간

    void Update()
    {
        Vector2 movement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (movement.sqrMagnitude > 0)
        {
            OnMoveInput?.Invoke(movement);
        }

        if (Input.GetMouseButtonDown(0)) // 일반 공격 입력
        {
            // 콤보 초기화 시간 체크
            if (Time.time - lastAttackTime > comboResetTime)
            {
                comboIndex = 1; // 콤보 초기화
            }

            OnAttackInput?.Invoke(comboIndex); // 콤보 인덱스 전달
            comboIndex = Mathf.Clamp(comboIndex + 1, 1, 3); // 콤보 인덱스 증가 (1~3 사이로 제한)
            lastAttackTime = Time.time;
        }

        if (Input.GetButtonDown("Jump")) // 스킬 입력
        {
            OnSkillInput?.Invoke();
        }
    }
}
