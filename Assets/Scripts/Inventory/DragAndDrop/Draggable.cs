using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Draggable : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    private Transform canvas;
    private Transform previousParent;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvas = FindObjectOfType<Canvas>().transform;
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    
    public void OnBeginDrag(PointerEventData eventData)
    {
        previousParent = transform.parent;
        
        transform.SetParent(canvas);
        transform.SetAsLastSibling();
        
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }
   
    public void OnDrag(PointerEventData eventData)
    {
        print("드래그 중");
        rectTransform.position = eventData.position;
    }
    

    public void OnEndDrag(PointerEventData eventData)
    {
        if (this.transform.parent == canvas)
        {
                transform.SetParent(previousParent);
                rectTransform.position = previousParent.GetComponent<RectTransform>().position;
        }
        
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }
}
