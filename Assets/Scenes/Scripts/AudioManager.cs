using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    #region SingletonPattern
    private static AudioManager instance;
    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                var obj = FindObjectOfType<AudioManager>();
                if (obj != null)
                {
                    instance = obj;
                }
                else
                {
                    var newSingleton = new GameObject("AudioManager").AddComponent<AudioManager>();
                    instance = newSingleton;
                }
            }

            return instance;
        }

        private set
        {
            instance = value;
        }

    }
    private void Awake()
    {
        var obj = FindObjectsOfType<AudioManager>();

        if (obj.Length != 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }
    #endregion

    public AudioClip[] clip;
    private AudioSource audioSource;
    private Dictionary<string, AudioClip> audioDictionary;
    private string audioPath;

    // default 
    //dic_audio[audioname] = Resources.Load(string.Format("Sounds/{0}", audioname)) as AudioClip;
    // Gesture is list bc gestures can come upto 5 simultaniously

    void Start()
    {
        audioPath = "Sounds";
        audioSource = GetComponent<AudioSource>();
        audioDictionary = new Dictionary<string, AudioClip>();

        foreach (AudioClip audio in Resources.LoadAll(audioPath))
        {
            audioDictionary.Add(audio.name, audio);
        }
    }

    public void PlaySound(Gesture g)
    {
        if (audioDictionary.ContainsKey(g.Audio))
        {
            //CashAudioclip(g.Audio);
            audioSource.PlayOneShot(audioDictionary[g.Audio]);
        }
    }
 

    //public void CashAudioclip(string audioname) {
    //    //clip = Resources.LoadAll<AudioClip>("Sounds");

    //    audioDictionary[audioname] = Resources.Load(string.Format("Sounds/{0}", audioname)) as AudioClip;
  
    //}
}
