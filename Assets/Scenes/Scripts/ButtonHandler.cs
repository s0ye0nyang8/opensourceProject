using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonHandler : MonoBehaviour
{
    private Button button_Perform, button_Undo, button_Create, button_Train;    //for toggle "interactable"
    private Text text_Notification, text_GestureList;
    private Text paneltext;
    
    private GameObject panel_CreateDialog;
    private GameObject load_panel;
    private bool testMode = false;

    private delegate void ModeChangedHandler();
    private event ModeChangedHandler ModeChanged;

    public Image defaultimg;
    public Sprite offimage;
    public Sprite onimage;

    private void Start()
    {
        ModeChanged += OnModeChanged;

        button_Perform = GameObject.Find("Button_Perform").GetComponent<Button>();
        button_Perform.interactable = false;
        button_Undo = GameObject.Find("Button_Undo").GetComponent<Button>();
        button_Undo.interactable = false;
        button_Create = GameObject.Find("Button_Create").GetComponent<Button>();
        button_Train = GameObject.Find("Button_Train").GetComponent<Button>();
        button_Train.interactable = false;
        text_Notification = GameObject.Find("Text_Notification").GetComponent<Text>();
        text_GestureList = GameObject.Find("Text_GestureList").GetComponent<Text>();

        panel_CreateDialog = GameObject.Find("Canvas").transform.Find("Panel_CreateDialog").gameObject;
        load_panel = GameObject.Find("load_panel");

        //paneltext = GameObject.Find("PText").GetComponent<Text>();
        //motions = GameObject.Find("PText2").GetComponent<Text>();

        //CreateGesture();    //creates an initial gesture. it will be removed when user input implements.

        text_Notification.text = "Register new gesture through [Create].\n\n" +
                                    "The sample will be recorded\n" +
                                    "while [Perform] is pressed.";
        text_GestureList.text = "";
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
        GestureManager.Instance.StartRead(testMode);
    }

    public void StopGesture()
    {
        if (!button_Perform.interactable)
            return;
        ChangeImage(false);
        double similarity = GestureManager.Instance.EndRead();

        if (testMode)
        {
            if (GestureManager.Instance.CurrentGestureID >= 0)
                text_Notification.text = $"Identified Gesture : {GestureManager.Instance.CurrentGestureName}\n" +
                                    $"Confidence {similarity * 100:0.00}%";
            else
                text_Notification.text = "Identification fails.\nPlease retry.";
        }
        else
        {
            button_Undo.interactable = true;
            button_Create.interactable = true;
            button_Train.interactable = true;

            text_Notification.text = $"Target Gesture : {GestureManager.Instance.CurrentGestureName}\n" +
                                    $"Samples : {GestureManager.Instance.CurrentSampleCount}\n\n" +
                                    "(At least 20 are recommended.)";
            SaveGesturesToFile();
        }
    }

    public void UndoLastGesture()
    {
        if (GestureManager.Instance.CurrentSampleCount > 0)
        {
            GestureManager.Instance.DeleteLastSample();
            text_Notification.text = $"Target Gesture : {GestureManager.Instance.CurrentGestureName}\n" +
                                        $"Samples : {GestureManager.Instance.CurrentSampleCount}\n\n" +
                                        "(At least 20 are recommended.)";
        }

        if (GestureManager.Instance.CurrentSampleCount == 0)
        {
            button_Undo.interactable = false;

            if (GestureManager.Instance.GestureCount <= 1)
            {
                button_Create.interactable = false;
                button_Train.interactable = false;
            }
        }
    }

    public void OpenCreateDialog()
    {
        if (panel_CreateDialog != null)
        {
            panel_CreateDialog.SetActive(true);
        }
    }

    public void Confirm()
    {
        var inputfield_GestureName = GameObject.Find("InputField_GestureName").GetComponent<InputField>();

        GestureManager.Instance.Register(inputfield_GestureName.text);
        inputfield_GestureName.text = "";
        //CloseCreateDialog();

        button_Perform.interactable = true;
        testMode = false;
        ModeChanged();
        text_Notification.text = $"Target Gesture : {GestureManager.Instance.CurrentGestureName}\n" +
                                    $"Samples : {GestureManager.Instance.CurrentSampleCount}\n\n" +
                                    "(At least 20 are recommended.)";
    }
    public void closePanel()
    {
        if (panel_CreateDialog != null)
        {
            panel_CreateDialog.SetActive(false);
        }

        if (load_panel != null)
        {
            load_panel.SetActive(false);
        }
    }

    public void ShowRecords()
    {
        // another panel;;;
        text_GestureList.text = "Recorded Gesture :\n";
        for (int i = 0; i < GestureManager.Instance.GestureCount; i++)
            text_GestureList.text += $"{GestureManager.Instance.GetGestureName(i)}, ";
        text_GestureList.text = text_GestureList.text.TrimEnd(',', ' ');
    }

    public void TrainGesture()
    {
        if (GestureManager.Instance.TryTrain())
        {
            testMode = true;
            ModeChanged();

            button_Undo.interactable = false;
            button_Train.interactable = false;

            paneltext.text = "..Training finished. CONTINUE?\n(At least 20 are recommended)";

        }
        else
        {
            text_Notification.text = "Training failed.";
        }
    }

    public void Reset()
    {
        GestureManager.Instance.Initialize();
        Start();
    }

    public void SaveGesturesToFile()
    {
        if (GestureManager.Instance.Save())
        {
            text_Notification.text = "Saved.";
        }
        else
        {
            text_Notification.text = "Failed.";
        }
    }

    public void LoadGesturesFromFile()
    {
        if (GestureManager.Instance.Load())
        {
            testMode = true;
            ModeChanged();

            text_Notification.text = "Loading trained data succeeded.\n";
            text_GestureList.text = "Recorded Gesture :\n";
            for (int i = 0; i < GestureManager.Instance.GestureCount; i++)
                text_GestureList.text += $"{GestureManager.Instance.GetGestureName(i)}, ";
            text_GestureList.text = text_GestureList.text.TrimEnd(',', ' ');
        }
        else
        {
            text_Notification.text = "Trained data to load does not exist.";
        }
    }

    public void LoadGesturesFromDefaultFile()
    {
        if (GestureManager.Instance.LoadDefault())
        {
            testMode = true;
            ModeChanged();

            text_Notification.text = "Loading trained data succeeded.\n";
            text_GestureList.text = "Recorded Gesture :\n";
            for (int i = 0; i < GestureManager.Instance.GestureCount; i++)
                text_GestureList.text += $"{GestureManager.Instance.GetGestureName(i)}, ";
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

        if (testMode)
        {
            buttonPerformText.text = "Test";
        }
        else
        {
            buttonPerformText.text = "Perform";
        }
    }
}
