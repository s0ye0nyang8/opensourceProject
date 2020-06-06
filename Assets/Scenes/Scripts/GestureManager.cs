using AOT;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

public class GestureManager : MonoBehaviour
{
    #region SingletonPattern
    private static GestureManager instance;
    public static GestureManager Instance
    {
        get
        {
            if (instance == null)
            {
                var obj = FindObjectOfType<GestureManager>();
                if (obj != null)
                {
                    instance = obj;
                }
                else
                {
                    var newSingleton = new GameObject("GestureManager").AddComponent<GestureManager>();
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
        var objs = FindObjectsOfType<GestureManager>();

        if (objs.Length != 1)
        {
            Destroy(gameObject);

            return;
        }

        DontDestroyOnLoad(gameObject);
    }
    #endregion

    private GCHandle handle;
    private GestureRecognition gr = new GestureRecognition();
    private List<string> gestureList = null;    //list for created gestures. it will include gestures from save files.
    private bool isPerforming = false;  //for distinguish "continue" gesture step. 3steps : "start" -> "continue" -> "end"
    private bool canSave = false;   //notify state that is able to save the file when train is complete.
    enum GestureID { None = -1 }
    private int currentGestureID = (int)GestureID.None;
    
    public int CurrentGestureID { get { return currentGestureID; } }
    public string CurrentGestureName { get { return gr.getGestureName(currentGestureID); } }
    public string GetGestureName(int index) => gr.getGestureName(index);
    public int CurrentSampleCount { get { return gr.getGestureNumberOfSamples(currentGestureID); } }
    public int GestureCount { get { return gr.numberOfGestures(); } }


    #region Section for demo. Afterwards, need to delete.
    private System.Random random = new System.Random();
    private readonly string[] tempGestureSuggestions = new string[]
    {   //temp list for demo test. it will be replaced by user suggestions(using textbox input).
        "Circle", "Square", "Star", "TiltLeft", "TiltRight", "UpDown", "Check"
    };
    private int anythingIndex = 1;

    //Afterwards, need to input gesture suggest by user, not system.
    //So, ex) SaveList, LoadList, Register(by user input)... functions will be required.
    private string GetGestureSuggestion()   
    {
        if (gestureList == null)
        {
            gestureList = new List<string>();
            for (int i = 0; i < tempGestureSuggestions.Length; i++)
            {
                gestureList.Add(tempGestureSuggestions[i]);
            }
        }

        if (gestureList.Count == 0)
        {
            return "Anything" + anythingIndex++;
        }
        int index = random.Next(0, gestureList.Count);
        string randomWord = gestureList[index];
        gestureList.RemoveAt(index);

        return randomWord;
    }
    #endregion

    private void Start()
    {
        Input.gyro.enabled = true;
        handle = GCHandle.Alloc(this);
        gr.setTrainingUpdateCallback(trainingUpdateCallback);
        gr.setTrainingUpdateCallbackMetadata((IntPtr)handle);
        gr.setTrainingFinishCallback(trainingFinishCallback);
        gr.setTrainingFinishCallbackMetadata((IntPtr)handle);
        Initialize();
    }

    private void Update()
    {
        if (isPerforming)   //true from StartRead(), false from EndRead().
        {
            Vector3 p = Input.gyro.userAcceleration;
            Quaternion q = Quaternion.FromToRotation(new Vector3(0, 1, 0), Input.gyro.gravity);
            gr.contdStrokeQ(p, q);  //startStroke() (from StartRead()) -> contdStrokeQ() -> endStroke() (from EndRead())

            Debug.Log($"acc = {p.x:0.00} {p.y:0.00} {p.z:0.00}\n"
                        + $"grav = {q.x:0.00} {q.y:0.00} {q.z:0.00}");
        }
    }

    public void Initialize()
    {
        isPerforming = false;
        canSave = false;
        gr.deleteAllGestures();
        currentGestureID = (int)GestureID.None;
        anythingIndex = 1;

        gestureList = new List<string>();
        for (int i = 0; i < tempGestureSuggestions.Length; i++)
        {
            gestureList.Add(tempGestureSuggestions[i]);
        }
    }

    public void Register(string name)  //it may need a strnig as parameter when user input implements.
    {
        // currentGestureID = gr.createGesture(GetGestureSuggestion());
        currentGestureID = gr.createGesture(name);

        if (currentGestureID < 0)   //when function returns negative, creation fails. retry.
            Register(name);
    }

    public void StartRead(bool isIdentificationMode = false)    //true : just read, false : for a sample.
    {
        isPerforming = true;
        Vector3 p = new Vector3(0.0f, 0.0f, 0.0f);
        Quaternion q = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);

        if (isIdentificationMode)
            gr.startStroke(p, q);
        else
            gr.startStroke(p, q, currentGestureID);
    }

    public double EndRead()
    {
        double similarity = 0;

        isPerforming = false;
        currentGestureID = gr.endStroke(ref similarity);

        return similarity;  //will be used by external class when identifying the gesture.
    }

    public void DeleteLastSample()
    {
        gr.deleteGestureSample(currentGestureID, CurrentSampleCount - 1);
    }

    public bool TryTrain() //depending on samples, it can fail.
    {
        gr.setMaxTrainingTime(20);
        if (gr.startTraining())
            return true;
        else
            // handling in case "failed" needs to implement. use callback???
            return false;
    }

    public bool Save()
    {
        string trainedData = "gestureSuggestions.dat";
        string trainedDataPath;

        if (canSave)
        {
#if UNITY_EDITOR
            trainedDataPath = "Assets/Data";
#elif UNITY_ANDROID
            trainedDataPath = Application.persistentDataPath;
#else
            trainedDataPath = Application.streamingAssetsPath;
#endif
            if (gr.saveToFile($"{trainedDataPath}/{trainedData}"))
            {
                canSave = false;
                Debug.Log($"Save completed.\n{trainedDataPath}/{trainedData} is created.");
                return true;
            }
            else
            {
                Debug.Log("Save failed.");
                return false;
            }
        }
        else
        {
            Debug.Log("Current state can't be saved.");
            return false;
        }
    }
    public bool LoadDefault()
    {
        string trainedData = "gestureSuggestions.dat";
        string trainedDataPath;

#if UNITY_EDITOR
        trainedDataPath = "Assets/StreamingAssets";
#elif UNITY_ANDROID
        var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        var unityWebRequest = UnityWebRequest.Get($"{Application.streamingAssetsPath}/{trainedData}");
        trainedDataPath = activity.Call<AndroidJavaObject>("getCacheDir").Call<string>("getCanonicalPath");
        unityWebRequest.SendWebRequest();

        while (!unityWebRequest.isDone)
        {
            // wait for file extraction to finish
        }
        if (unityWebRequest.isNetworkError)
        {
            Debug.Log("Load failed. Network must be connected for loading the default data.");
            return false;
        }
        File.WriteAllBytes($"{trainedDataPath}/{trainedData}", unityWebRequest.downloadHandler.data);
#else
        trainedDataPath = Application.streamingAssetsPath;
#endif
        if (gr.loadFromFile($"{trainedDataPath}/{trainedData}"))
        {
            Debug.Log("Load completed");
            return true;
        }
        else
        {
            Debug.Log("Load failed");
            return false;
        }
    }

    public bool Load()
    {
        string trainedData = "gestureSuggestions.dat";
        string trainedDataPath;

#if UNITY_EDITOR
        trainedDataPath = "Assets/Data";
#elif UNITY_ANDROID
        trainedDataPath = Application.persistentDataPath;
#else
        trainedDataPath = Application.streamingAssetsPath;
#endif
        if (gr.loadFromFile($"{trainedDataPath}/{trainedData}"))
        {
            Debug.Log("Load completed");
            return true;
        }
        else
        {
            Debug.Log("Load failed");
            return false;
        }
    }

    [MonoPInvokeCallback(typeof(GestureRecognition.TrainingCallbackFunction))]
    public static void trainingUpdateCallback(double performance, IntPtr ptr)
    {
        // Get the script/scene object back from metadata.
        GCHandle obj = (GCHandle)ptr;
        GestureManager me = obj.Target as GestureManager;
        // Update the performance indicator with the latest estimate.
        //me.last_performance_report = performance;
    }

    [MonoPInvokeCallback(typeof(GestureRecognition.TrainingCallbackFunction))]
    public static void trainingFinishCallback(double performance, IntPtr ptr)
    {
        var gestureManager = ((GCHandle)ptr).Target as GestureManager;
        // Update the performance indicator with the latest estimate.
        //me.last_performance_report = performance;
        // Signal that training was finished.
        //me.recording_gesture = -3;

        gestureManager.canSave = true;
    }
}
