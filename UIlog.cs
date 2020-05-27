using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Security;
using UnityEngine;
using UnityEngine.UI;


public class UIlog : MonoBehaviour
{
    float[] a = new float[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    int i;
    public Text nowText;
    // Start is called before the first frame update
    void SetCountText()
    {
         nowText.text = a[0].ToString()+ a[1].ToString() + a[2].ToString() + a[3].ToString() + a[4].ToString() + a[5].ToString() + a[6].ToString() + a[7].ToString() + a[8].ToString() + a[9].ToString();
    }
    void Start()
    {
        SetCountText();
    }

    // Update is called once per frame
    void Update()
    {
        accel accel = GameObject.Find("Cube").GetComponent<accel>();
        if (accel.a != 0)
        {
            for (i = 0; i < 9; i++) a[i] = a[i + 1];
            a[9] = accel.a;
        }
        SetCountText();
    }
    
}
