using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class ButtonHandler : MonoBehaviour
{
    private Button button_Perform, button_Undo, button_Create, button_Train;    //for toggle "interactable"
    public Text text_Notification, text_GestureList;
    
    private GameObject panel_CreateDialog;
    private GameObject panel_LoadDialog;

    private bool isIdentificationMode = false;

    private delegate void ModeChangedHandler();
    private event ModeChangedHandler ModeChanged;

    public Image defaultimg;
    public Sprite offimage;
    public Sprite onimage;

    private void Start()
    {
        ModeChanged += OnModeChanged;
        GestureManager.Instance.OnTrainingInProgress += NotifyTrainingInProgress;
        GestureManager.Instance.OnTrainingCompleted += NotifyTrainingCompleted;

        button_Perform = GameObject.Find("Button_Perform").GetComponent<Button>();
        button_Perform.interactable = false;
        button_Undo = GameObject.Find("Button_Undo").GetComponent<Button>();
        button_Undo.interactable = false;
        button_Create = GameObject.Find("Button_Create").GetComponent<Button>();
        button_Train = GameObject.Find("Button_Train").GetComponent<Button>();
        button_Train.interactable = false;


        panel_CreateDialog = GameObject.Find("Canvas").transform.Find("Panel_CreateDialog").gameObject;
        panel_LoadDialog = GameObject.Find("Canvas").transform.Find("panel_LoadDialog").gameObject;

        /*text_Notification.text = "Register new gesture through [Create].\n\n" +
                                    "The sample will be recorded\n" +
                                    "while [Perform] is pressed.";
        text_GestureList.text = "";*/
    }

    public void ChangeImage(bool a)
    {
        if (a)
        {
            defaultimg.sprite = onimage;
        }
        else
        {
            defaultimg.sprite = offimage;
        }
    }

    public void StartGesture()
    {
        if (!button_Perform.interactable)
            return;
        ChangeImage(true);

        GestureManager.Instance.StartRead(isIdentificationMode);
    }

    public void StopGesture()
    {
        if (!button_Perform.interactable)
            return;
        ChangeImage(false);

        int identifiedIndex = GestureManager.Instance.EndRead();
        if (isIdentificationMode)
        {
            var currentGesture = GestureManager.Instance.GestureList[identifiedIndex];
            if (identifiedIndex >= 0)
                text_Notification.text = $"Identified Gesture : {currentGesture.Name}\n";
            else
                text_Notification.text = "Identification fails.\nPlease retry.";
        }
        else
        {
            button_Undo.interactable = true;
            button_Create.interactable = true;
            button_Train.interactable = true;

            var currentGesture = GestureManager.Instance.GetRecentGesture;
            text_Notification.text = $"Target Gesture : {currentGesture.Name}\n" +
                                    $"Samples : {currentGesture.SampleCount}\n\n" +
                                    "(At least 20 are recommended.)";
        }
    }

    public void UndoLastGesture()
    {
        int lastGestureIndex = GestureManager.Instance.GestureList.Count - 1;
        GestureManager.Instance.DeleteLastSample(lastGestureIndex);

        var currentGesture = GestureManager.Instance.GetRecentGesture;
        text_Notification.text = $"Target Gesture : {currentGesture.Name}\n" +
                                    $"Samples : {currentGesture.SampleCount}\n\n" +
                                    "(At least 20 are recommended.)";
    }

    public void OpenCreateDialog()
    {
        if (panel_CreateDialog != null)
        {
            panel_CreateDialog.SetActive(true);
        }
    }

    public void CloseCreateDialog()
    {
        if (panel_CreateDialog != null)
        {
            panel_CreateDialog.SetActive(false);
        }
    }

    public void ConfirmCreateDialog()
    {
        var inputfield_GestureName = GameObject.Find("InputField_GestureName").GetComponent<InputField>();
        GestureManager.Instance.Register(inputfield_GestureName.text);
        inputfield_GestureName.text = "";

        CloseCreateDialog();

        button_Perform.interactable = true;
        isIdentificationMode = false;
        ModeChanged();

        var currentGesture = GestureManager.Instance.GetRecentGesture;
        text_Notification.text = $"Target Gesture : {currentGesture.Name}\n" +
                                    $"Samples : {currentGesture.SampleCount}\n\n" +
                                    "(At least 20 are recommended.)";
    }
    public void openLoadDialog()
    {
        panel_LoadDialog.SetActive(true);
  
    }

    public void closeLoadDialog()
    {
        panel_LoadDialog.SetActive(false);
    
    }

    public void ShowRecords()
    {
        // another panel;;;
        text_GestureList.text = "Recorded Gesture :\n";
        for (int i = 0; i < GestureManager.Instance.GestureList.Count; i++)
            text_GestureList.text += $"{GestureManager.Instance.GestureList[i].Name}, ";
        text_GestureList.text = text_GestureList.text.TrimEnd(',', ' ');
    }


    public void TrainGesture()
    {
        if (GestureManager.Instance.TryTrain())
        {
        }
        else
        {
            text_Notification.text = "Training failed.";
        }
    }

    public void NotifyTrainingInProgress(double rate)
    {
        MainThreadCaller.Instance.Enqueue(ChangeUIWhenTrainingInProgress(rate));
    }

    private IEnumerator ChangeUIWhenTrainingInProgress(double rate)
    {
        text_Notification.text = $"Training in progress... {rate * 100:00.00}%\nPlease wait.";

        button_Perform.interactable = false;
        button_Create.interactable = false;
        button_Undo.interactable = false;
        button_Train.interactable = false;
        yield return null;
    }

    public void NotifyTrainingCompleted(double rate)
    {
        MainThreadCaller.Instance.Enqueue(ChangeUIWhenTrainingCompleted(rate));
    }

    private IEnumerator ChangeUIWhenTrainingCompleted(double rate)
    {
        isIdentificationMode = true;
        ModeChanged();

        text_Notification.text = "Training finished.\n\nTest : Identify the recorded gesture.\nCreate : Add another gesture.";
        text_GestureList.text = "Recorded Gesture :\n";
        for (int i = 0; i < GestureManager.Instance.GestureList.Count; i++)
            text_GestureList.text += $"{GestureManager.Instance.GestureList[i].Name}, ";
        text_GestureList.text = text_GestureList.text.TrimEnd(',', ' ');

        button_Perform.interactable = true;
        button_Create.interactable = true;

        yield return null;
    }


    public void Reset()
    {
        GestureManager.Instance.Initialize();
        Start();
    }

    public void SaveGesturesToFile()
    {
        if (GestureManager.Instance.SaveTrainedData())
        {
            text_Notification.text = "Saving trained data succeeded.";
        }
        else
        {
            text_Notification.text = "Saving trained data failed.";
        }
    }

    public void LoadGesturesFromFile()
    {
        if (GestureManager.Instance.LoadCustomTrainedData())
        {
            button_Perform.interactable = true;
            isIdentificationMode = true;
            ModeChanged();

            text_Notification.text = "Loading trained data succeeded.\n";
            text_GestureList.text = "Recorded Gesture :\n";
            for (int i = 0; i < GestureManager.Instance.GestureList.Count; i++)
                text_GestureList.text += $"{GestureManager.Instance.GestureList[i].Name}, ";
            text_GestureList.text = text_GestureList.text.TrimEnd(',', ' ');
        }
        else
        {
            text_Notification.text = "Trained data to load does not exist.";
        }
    }

    public void LoadGesturesFromDefaultFile()
    {
        if (GestureManager.Instance.LoadDeafaultTrainedData())
        {
            button_Perform.interactable = true;
            isIdentificationMode = true;
            ModeChanged();

            text_Notification.text = "Loading trained data succeeded.\n";
            text_GestureList.text = "Recorded Gesture :\n";
            for (int i = 0; i < GestureManager.Instance.GestureList.Count; i++)
                text_GestureList.text += $"{GestureManager.Instance.GestureList[i].Name}, ";
            text_GestureList.text = text_GestureList.text.TrimEnd(',', ' ');
        }
        else
        {
            text_Notification.text = "Trained data to load does not exist.";
        }
    }

    private void OnModeChanged()
    {
        var buttonPerformText = GameObject.Find("Button_Perform").GetComponentInChildren<Text>();

        if (isIdentificationMode)
        {
            buttonPerformText.text = "Test";
        }
        else
        {
            buttonPerformText.text = "Perform";
        }
    }
}
