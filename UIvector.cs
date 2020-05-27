using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;


public class UIvector : MonoBehaviour
{
    Vector3 mov;
    public Text nowText;
    // Start is called before the first frame update
    void SetCountText()
    {
        nowText.text = "Vector: " + mov.ToString("N4");
    }
    void Start()
    {
        SetCountText();
    }

    // Update is called once per frame
    void Update()
    {
        accel accel = GameObject.Find("Cube").GetComponent<accel>();
        mov = accel.move;
        SetCountText();
    }
    
}
