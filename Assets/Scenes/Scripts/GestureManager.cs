using System.Collections.Generic;
using UnityEngine;

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

    private GestureRecognition gr = new GestureRecognition();
    private List<string> gestureList = null;    //list for created gestures. it will include gestures from save files.
    private bool isPerforming = false;  //for distinguish "continue" gesture step. 3steps : "start" -> "continue" -> "end"
    enum GestureID { None = -1 }
    private int currentGestureID = (int)GestureID.None;
    
    public string CurrentGestureName { get { return gr.getGestureName(currentGestureID); } }
    public string GetGestureName(int index) => gr.getGestureName(index);
    public int CurrentSampleCount { get { return gr.getGestureNumberOfSamples(currentGestureID); } }
    public int GestureCount { get { return gr.numberOfGestures(); } }


    #region Section for demo. Afterwards, need to delete.
    private System.Random random = new System.Random();
    private readonly string[] tempGestureSuggestions = new string[]
    {   //temp list for demo test. it will be replaced by user suggestions(using textbox input).
        "Circle", "Square", "Star", "TiltLeft", "TiltRight", "UpDown"
    };

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
            return random.Next(0, 1000).ToString();
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
        gr.deleteAllGestures();
        currentGestureID = (int)GestureID.None;

        gestureList = new List<string>();
        for (int i = 0; i < tempGestureSuggestions.Length; i++)
        {
            gestureList.Add(tempGestureSuggestions[i]);
        }
    }

    public void Register()  //it may need a strnig as parameter when user input implements.
    {
        currentGestureID = gr.createGesture(GetGestureSuggestion());
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
}
