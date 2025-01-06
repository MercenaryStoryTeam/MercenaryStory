using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Droppable : MonoBehaviour, IPointerEnterHandler, IDropHandler, IPointerExitHandler
{
    private Image slotImage;
    private RectTransform rect;

    private void Awake()
    {
        slotImage = GetComponentInChildren<Image>();
        rect = GetComponent<RectTransform>();
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        SetAlpha(slotImage, 0.8f);
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            eventData.pointerDrag.transform.SetParent(transform);
            eventData.pointerDrag.GetComponent<RectTransform>().position = rect.position;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetAlpha(slotImage, 1f);
    }

    private void SetAlpha(Image image, float alpha)
    {
        Color colorAlpha = image.color;
        colorAlpha.a = alpha;
        image.color = colorAlpha;
    }
}
