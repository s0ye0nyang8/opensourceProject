using AOT;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.AI;
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

    private List<Gesture> gestureList;

    public ReadOnlyCollection<Gesture> GestureList { get { return gestureList.AsReadOnly(); } }
    public Gesture GetRecentGesture { get { return gestureList[gestureList.Count - 1]; } }

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
    }

    public void Initialize()
    {
        gestureList = new List<Gesture>();
        isPerforming = false;
        canSave = false;

        gr.deleteAllGestures();
    }

    public void Register(string name)  //it may need a strnig as parameter when user input implements.
    {
        gr.createGesture(name);
        gestureList.Add(new Gesture(name));
    }

    //isIdentificationMode : true, just read. false, for a sample.
    //index : if it is positive, add new sample to already existed gesture in index.
    public void StartRead()
    {
        isPerforming = true;
        Vector3 p = new Vector3(0.0f, 0.0f, 0.0f);
        Quaternion q = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);

        //if (isIdentificationMode)
        gr.startStroke(p, q);
        //else
        //{
        //    int index = gestureList.Count - 1;
        //    gr.startStroke(p, q, index); //for the gesture created recently.
        //    gestureList[index].SampleCount++;

        //}
    }

    public void StartReadAt(int index) //for collecting samples of gestures from specific index.
    {
        if (index < 0 || index >= gr.numberOfGestures())
        {
            Debug.Log($"There's not the corresponding index {index} to add the sample.");
            return;
        }

        isPerforming = true;
        Vector3 p = new Vector3(0.0f, 0.0f, 0.0f);
        Quaternion q = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);

        gr.startStroke(p, q, index);
        gestureList[index].SampleCount++;
    }

    private Vector3 recentP;
    private Quaternion recentQ;
    private void ContinueRead()
    {
        Vector3 p = Input.gyro.userAcceleration;
        Quaternion q = Quaternion.FromToRotation(new Vector3(0, 1, 0), Input.gyro.gravity);


        // Linear interpolation
        if (recentP != null)
        {
            if (recentP != p)
            {
                // n : Count of data points
                int n = 10;
                float t = 1.0f / n;
                for (float i = t; i < 0.99f; i += t)
                {
                    var addP = Vector3.Lerp(recentP, p, i);
                    var addQ = Quaternion.Lerp(recentQ, q, i);
                    gr.contdStrokeQ(addP, addQ);

                    Debug.Log($"acc = {addP.x:0.00} {addP.y:0.00} {addP.z:0.00}\n"
                        + $"grav = {addQ.x:0.00} {addQ.y:0.00} {addQ.z:0.00}");
                }

                recentP = p;
                recentQ = q;
            }
        }

        gr.contdStrokeQ(p, q);  //startStroke() (from StartRead()) -> contdStrokeQ() -> endStroke() (from EndRead())

        Debug.Log($"acc = {p.x:0.00} {p.y:0.00} {p.z:0.00}\n"
                    + $"grav = {q.x:0.00} {q.y:0.00} {q.z:0.00}");
    }

    public int EndRead()
    {
        double similarity = 0;
        int identifiedGestureIndex = gr.endStroke(ref similarity);
        isPerforming = false;

        //increase recognition rate using similarity??

        return identifiedGestureIndex;  //will be used by external class when identifying the gesture.
    }

    public bool Delete(int index)
    {
        bool isDeleted = gr.deleteGesture(index);
        if (isDeleted)
            gestureList.RemoveAt(index);

        return isDeleted;
    }

    public bool DeleteLastSample(int index)
    {
        bool isDeleted = gr.deleteGestureSample(index, gr.getGestureNumberOfSamples(index) - 1);
        if (isDeleted)
            gestureList[index].SampleCount--;

        Debug.Log($"{gr.getGestureNumberOfSamples(index)}");

        return isDeleted;
    }

    public bool DeleteAllSamples(int index)
    {
        bool isDeleted = gr.deleteAllGestureSamples(index);
        if (isDeleted)
            gestureList[index].SampleCount = 0;

        return isDeleted;
    }

    public bool TryTrain() //depending on samples, it can fail.
    {
        gr.setMaxTrainingTime(30);
        if (gr.startTraining())
        {
            return true;
        }
        else
            // handling in case "failed" needs to implement. use callback???
            return false;
    }
    public bool SaveTrainedData()
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

    public bool LoadDeafaultTrainedData()   //load default gesture suggestions file in Assets/StreamingAssets/
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
            gestureList = new List<Gesture>();
            for (int i = 0; i < gr.numberOfGestures(); i++)
            {
                gestureList.Add(new Gesture(gr.getGestureName(i), gr.getGestureNumberOfSamples(i)));
            }

            return true;
        }
        else
        {
            Debug.Log("Load failed");
            return false;
        }
    }

    public bool LoadCustomTrainedData(string path)
    {
        if (gr.loadFromFile(path))
        {
            Debug.Log("Load completed");
            gestureList = new List<Gesture>();
            for (int i = 0; i < gr.numberOfGestures(); i++)
            {
                gestureList.Add(new Gesture(gr.getGestureName(i), gr.getGestureNumberOfSamples(i)));
            }

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

        Debug.Log($"Training in progress... {rate * 100:00.00}%");
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
