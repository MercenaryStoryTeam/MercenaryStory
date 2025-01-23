using UnityEngine;
using UnityEngine.EventSystems;

// 조이스틱 
public class VirtualJoystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    [Header("조이스틱 배경 RectTransform")]
    public RectTransform joystickBackground;

    [Header("조이스틱 핸들 RectTransform")]
    public RectTransform joystickHandle;

    [Header("조이스틱 핸들의 이동 범위")]
    public float joystickRange = 35f;

    [Header("Dead Zone")]
    public float deadZone = 0.2f;

    // 입력 벡터 (조이스틱의 방향과 크기)
    private Vector2 inputVector;

    // 입력 벡터를 외부에서 접근할 수 있도록 공개
    public Vector2 InputVector => inputVector;

    // 조이스틱 핸들러 이동
    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pos;
        // 입력된 화면 위치를 조이스틱 배경 안의 로컬 위치로 변환
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            joystickBackground, eventData.position, eventData.pressEventCamera, out pos))
        {
            // 조이스틱 배경 크기를 기준으로 위치를 정규화
            pos.x /= joystickBackground.sizeDelta.x;
            pos.y /= joystickBackground.sizeDelta.y;

            // 입력 벡터 계산 (정규화된 위치를 기반으로 계산)
            inputVector = new Vector2(pos.x * 2, pos.y * 2);
            inputVector = (inputVector.magnitude > 1.0f) ? inputVector.normalized : inputVector;

            // Dead Zone 적용, 작은 입력값 무시 -> 핸들러 원점 근처에서 불필요한 입력을 발생시키는 문제를 방지
            if (inputVector.magnitude < deadZone)
            {
                inputVector = Vector2.zero;
            }

            // 조이스틱 핸들의 위치를 joystickRange 범위 안으로 제한
            joystickHandle.anchoredPosition = new Vector2(
                Mathf.Clamp(inputVector.x * joystickRange, -joystickRange, joystickRange),
                Mathf.Clamp(inputVector.y * joystickRange, -joystickRange, joystickRange));
        }
    }

    // 조이스틱 핸들러 이동 
    public void OnPointerDown(PointerEventData eventData)
    {
        // 드래그 이벤트 호출
        OnDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // 입력 벡터를 초기화하고 조이스틱 핸들의 위치를 원점으로 되돌림
        inputVector = Vector2.zero;
        joystickHandle.anchoredPosition = Vector2.zero;
    }
}

// 완성 
