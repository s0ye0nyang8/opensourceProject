using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIvel: MonoBehaviour
{
    public Text nowText;
    public Vector3 velo;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        accel accel = GameObject.Find("Cube").GetComponent<accel>();
        velo = accel.vel;
        nowText.text = velo.ToString("N3");
    }
}
