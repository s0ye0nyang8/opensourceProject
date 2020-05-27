using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Security;
using UnityEngine;
using UnityEngine.UI;


public class UIformation : MonoBehaviour
{
    
    public float a;
    public Text nowText;
    // Start is called before the first frame update
    void SetCountText()
    {
        nowText.text = "Formation: " + a.ToString();
    }
    void Start()
    {
        SetCountText();
    }

    // Update is called once per frame
    void Update()
    {
        accel accel = GameObject.Find("Cube").GetComponent<accel>();
        a = accel.a;
        SetCountText();
    }
    
}
