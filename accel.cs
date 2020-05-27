using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Threading;
using UnityEngine;


public class accel : MonoBehaviour
{
    public Vector3 move,vel,real;
    public float accellimit = 0.1f;
    public float vellimit = 0.03f;
    public float a;
    float alpha = 0.8f,tempX,tempY,tempZ;
    // Start is called before the first frame update
    void Start()
    {

    }
    // Update is called once per frame
    void Update()
    {
        tempX = alpha * tempX + (1 - alpha) * Input.acceleration.x;
        tempY = alpha * tempY + (1 - alpha) * Input.acceleration.y;
        tempZ = alpha * tempZ + (1 - alpha) * Input.acceleration.z;
        move = new Vector3(Input.acceleration.x - tempX, Input.acceleration.y - tempY, Input.acceleration.z - tempZ);

        if (move.x < accellimit && move.x > -accellimit) move.x = 0;
        if (move.y < accellimit && move.y > -accellimit) move.y = 0;
        if (move.z < accellimit && move.z > -accellimit) move.z = 0;

        vel = vel + move * Time.deltaTime;
        if (vel.x < vellimit && vel.x > -vellimit) vel.x = 0;
        if (vel.y < vellimit && vel.y > -vellimit) vel.y = 0;
        if (vel.z < vellimit && vel.z > -vellimit) vel.z = 0;
        if (move == new Vector3(0, 0, 0)) vel = new Vector3(0, 0, 0);
        if (vel.x > 0) real.x = 1;
        else if (vel.x < 0) real.x = 2;
        else real.x = 0;
        if (vel.y > 0) real.y = 1;
        else if (vel.y < 0) real.y = 2;
        else real.y = 0;
        if (vel.z > 0) real.z = 1;
        else if (vel.z < 0) real.z = 2;
        else real.z = 0;
        a = real.x * 9 + real.y * 3 + real.z;

        GetComponent<Rigidbody>().velocity = vel*10;
    }
}
