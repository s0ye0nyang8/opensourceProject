using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GestureManager : MonoBehaviour
{
    #region Singleton Pattern
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

    private readonly string[] tempGestureSuggestions = new string[]
    {
        "Circle", "Square", "Star", "TiltLeft", "TiltRight", "UpDown"
    };
    private List<string> gestureList = null;
    private System.Random rnd = new System.Random();

    private Text HUDText;

    private void Start()
    {
        HUDText = GameObject.Find("HUDText").GetComponent<Text>();
        Input.gyro.enabled = true;
    }

    //temp
    private string GetRandomWord()
    {
        if (gestureList == null)
        {
            gestureList = new List<string>();
            for (int i = 0; i < tempGestureSuggestions.Length; i++)
            {
                gestureList.Insert(0, tempGestureSuggestions[i]);
            }
        }
        if (gestureList.Count == 0)
        {
            return rnd.Next(0, 1000).ToString();
        }
        int index = rnd.Next(0, gestureList.Count);
        string random_word = gestureList[index];
        gestureList.RemoveAt(index);
        return random_word;
    }

    

    enum GestureID { None = -1 }
    private int recordingGestureID = (int)GestureID.None;
    
    public void CreateGesture()
    {
        recordingGestureID =  gr.createGesture(GetRandomWord());    //현재 기록될 제스쳐 인덱스 저장
        HUDText.text = $"Selected Gesture : {gr.getGestureName(recordingGestureID)}";
    }

    public bool StartRead(bool isRecordMode = false)
    {
        Vector3 p = new Vector3(0.0f, 0.0f, 0.0f);
        Quaternion q = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);

        //제스쳐 추가가 아닌 기존 제스쳐에 샘플 추가 할 때 케이스를 고려해야함.
        if (isRecordMode)
        {
            if (recordingGestureID == (int)GestureID.None)
                return false;   //기록된 gesture가 없으면 false 리턴

            gr.startStroke(p, q, recordingGestureID);
        }
        else
        {
            gr.startStroke(p, q);
        }

        return true;
    }

    public void ContinueRead()
    {
        Vector3 p = Input.gyro.userAcceleration;
        Quaternion q = Quaternion.FromToRotation(new Vector3(0, 1, 0), Input.gyro.gravity);
        gr.contdStrokeQ(p, q);

        HUDText.text = "acc=\n" + Input.gyro.userAcceleration.x.ToString("0.00") + " " + Input.gyro.userAcceleration.y.ToString("0.00") + " " + Input.gyro.userAcceleration.z.ToString("0.00") + "\n"
                         + "grav=\n" + Input.gyro.gravity.x.ToString("0.00") + " " + Input.gyro.gravity.y.ToString("0.00") + " " + Input.gyro.gravity.z.ToString("0.00");
    }

    public void EndRead(bool isIdentifyMode = false)
    {
        int gestureID = gr.endStroke();

        if (isIdentifyMode)
        {
            HUDText.text = $"Identified Gesture : {gr.getGestureName(gestureID)}";
        }
        else
        {
            HUDText.text = $"Gesture Name : {gr.getGestureName(gestureID)}\n" +
                            $"Sample count : {gr.getGestureNumberOfSamples(gestureID)}";
        }
    }

    public void Train()
    {
        gr.setMaxTrainingTime(20);
        gr.startTraining();
    }
}
