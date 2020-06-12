using AOT;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        var obj = FindObjectsOfType<GestureManager>();

        if (obj.Length != 1)
        {
            Destroy(gameObject);

            return;
        }

        DontDestroyOnLoad(gameObject);
    }
    #endregion

    private GCHandle handle;
    private GestureRecognition gr = new GestureRecognition();
    private bool isPerforming = false;  //for distinguish "continue" gesture step. 3steps : "start" -> "continue" -> "end"
    private bool canSave = false;   //notify state that is able to save the file when train is complete.
    enum GestureID { None = -1 }
    private int currentGestureID = (int)GestureID.None;
    private List<Gesture> gestureList;

    public ReadOnlyCollection<Gesture> GestureList { get { return gestureList.AsReadOnly(); } }
    public int CurrentGestureID { get { return currentGestureID; } }    //index of recent identified or recording gesture.
    public string CurrentGestureName { get { return gr.getGestureName(currentGestureID); } }
    public string GetGestureName(int index) => gr.getGestureName(index);
    public int CurrentSampleCount { get { return gr.getGestureNumberOfSamples(currentGestureID); } }
    public int GestureCount { get { return gr.numberOfGestures(); } }

    public delegate void TrainingEventDelegate(double performance);
    public event TrainingEventDelegate OnTrainingInProgress;
    public event TrainingEventDelegate OnTrainingCompleted;

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
            ContinueRead();
        }

        if (gr.isTraining())
        {

        }

    }

    public void Initialize()
    {
        gestureList = new List<Gesture>();
        isPerforming = false;
        canSave = false;

        gr.deleteAllGestures();
        currentGestureID = (int)GestureID.None;
    }

    public void Register(string name)  //it may need a strnig as parameter when user input implements.
    {
        int index = gr.createGesture(name);

        //gestureList.Add(new Gesture(index, name));
    }

    //isIdentificationMode : true, just read. false, for a sample.
    //index : if it is positive, add new sample to already existed gesture in index.
    public void StartRead(bool isIdentificationMode = false, int index = (int)GestureID.None)
    {
        isPerforming = true;
        Vector3 p = new Vector3(0.0f, 0.0f, 0.0f);
        Quaternion q = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);

        if (isIdentificationMode)
            gr.startStroke(p, q);
        else
        {
            if (index < 0)
                gr.startStroke(p, q, currentGestureID);
            else if (index < gr.numberOfGestures())
                gr.startStroke(p, q, index);
            else
            {
                isPerforming = false;
                Debug.Log("There's not the corresponding index to add the sample.");
            }
        }
    }

    private void ContinueRead()
    {
        Vector3 p = Input.gyro.userAcceleration;
        Quaternion q = Quaternion.FromToRotation(new Vector3(0, 1, 0), Input.gyro.gravity);
        gr.contdStrokeQ(p, q);  //startStroke() (from StartRead()) -> contdStrokeQ() -> endStroke() (from EndRead())

        Debug.Log($"acc = {p.x:0.00} {p.y:0.00} {p.z:0.00}\n"
                    + $"grav = {q.x:0.00} {q.y:0.00} {q.z:0.00}");
    }

    public double EndRead()
    {
        double similarity = 0;

        isPerforming = false;
        currentGestureID = gr.endStroke(ref similarity);

        return similarity;  //will be used by external class when identifying the gesture.
    }

    public bool Delete(int index)
    {
        bool isDeleted = gr.deleteGesture(index);

        if (isDeleted)
        {
            gestureList.RemoveAt(index);
        }

        return isDeleted;
    }

    public bool DeleteLastSample()
    {
        return gr.deleteGestureSample(currentGestureID, CurrentSampleCount - 1);
    }

    public bool DeleteLastSample(int index)
    {
        return gr.deleteGestureSample(index, CurrentSampleCount - 1);
    }

    public bool TryTrain() //depending on samples, it can fail.
    {
        gr.setMaxTrainingTime(20);
        if (gr.startTraining())
        {
            return true;
        }
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
            Debug.Log("The Current state can't be saved."); //ex) try [save] before training...
            return false;
        }
    }
    public bool LoadDefault()   //load default gesture suggestions file in Assets/StreamingAssets/
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
            //for (int i = 0; i < gr.numberOfGestures(); i++)
            //{
            //    gestureList.Add(new Gesture(i, gr.getGestureName(i)));
            //}

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
            //for (int i = 0; i < gr.numberOfGestures(); i++)
            //{
            //    gestureList.Add(new Gesture(i, gr.getGestureName(i)));
            //}

            return true;
        }
        else
        {
            Debug.Log("Load failed");
            return false;
        }
    }

    [MonoPInvokeCallback(typeof(GestureRecognition.TrainingCallbackFunction))]
    public static void trainingUpdateCallback(double rate, IntPtr ptr)
    {
        var gestureManager = ((GCHandle)ptr).Target as GestureManager;
        gestureManager.OnTrainingInProgress(rate);

        Debug.Log($"Training in progress... {rate}%");
    }

    [MonoPInvokeCallback(typeof(GestureRecognition.TrainingCallbackFunction))]
    public static void trainingFinishCallback(double rate, IntPtr ptr)
    {
        var gestureManager = ((GCHandle)ptr).Target as GestureManager;
        gestureManager.OnTrainingCompleted(rate);
        gestureManager.canSave = true;

        Debug.Log("Training completed.");
    }
}
