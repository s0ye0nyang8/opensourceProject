using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gesture
{
    public int SampleCount;
    public string Name;
    public string Audio;

    public Gesture(string name = "", int sampleCount = 0, string audio = "")
    {
        SampleCount = sampleCount;
        Name = name;
        Audio = audio;
    }
}
