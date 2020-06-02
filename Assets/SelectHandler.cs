using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectHandler : MonoBehaviour
{
    public void SelectGesture()
    {
        GestureManager.Instance.CreateGesture();
    }
}
