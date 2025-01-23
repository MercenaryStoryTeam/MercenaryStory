using UnityEngine;
using UnityEngine.EventSystems;

// 화면 터치 및 드래그 처리
public class TouchDragHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("버튼의 상호작용 상태")]
    public bool isPressed = false; // 버튼이 눌렸는지 상태 확인

    // 버튼을 터치했을 때 호출 (터치 시작)
    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        Debug.Log("버튼 터치 시작");
    }

    // 버튼을 터치한 상태에서 드래그할 때 호출
    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("버튼 드래그 중: " + eventData.position);
    }

    // 버튼에서 손을 뗐을 때 호출 (터치 종료)
    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        Debug.Log("버튼 터치 종료");
    }
}
