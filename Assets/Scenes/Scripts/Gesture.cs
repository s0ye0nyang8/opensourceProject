using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gesture
{
    public int Index;
    public string Name;
    public string Audio;

    public Gesture(int index = -1, string name = "", string audio = "")
    {
        Index = index;
        Name = name;
        Audio = audio;
    }
}
