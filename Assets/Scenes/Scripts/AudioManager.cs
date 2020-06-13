using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioClip[] clip;
    public AudioSource audioSource;
    //public GestureManager gm = new GestureManager();
    Dictionary<string, AudioClip> dic_audio = new Dictionary<string, AudioClip>();

    // default 
    //dic_audio[audioname] = Resources.Load(string.Format("Sounds/{0}", audioname)) as AudioClip;


    // Gesture is list bc gestures can come upto 5 simultaniously

    void Start()
    {

    }

    //g is GetRecentGesture
    public void PlaySound(Gesture g, float volume) {

        if (!dic_audio.ContainsKey(g.Audio))
        {
            CashAudioclip(g.Audio);
        }
        
        audioSource.PlayOneShot(dic_audio[g.Audio], volume);

    }
 

    public void CashAudioclip(string audioname) {
        //clip = Resources.LoadAll<AudioClip>("Sounds");

        dic_audio[audioname] = Resources.Load(string.Format("Sounds/{0}", audioname)) as AudioClip;
  
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
