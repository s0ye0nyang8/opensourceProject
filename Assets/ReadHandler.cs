using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ReadHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    bool isButtonDown = false;

    void Update()
    {
        if (isButtonDown)
        {
            GestureManager.Instance.ContinueRead();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (GestureManager.Instance.StartRead(false))
            isButtonDown = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isButtonDown = false;
        GestureManager.Instance.EndRead(true);
    }
}
