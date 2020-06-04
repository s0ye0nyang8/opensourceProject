using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonHandler : MonoBehaviour
{
    private Button button_Undo, button_Create, button_Train;    //for toggle "interactable"
    private Text text_Notification, text_GestureList;
    private bool testMode = false;

    private void Start()
    {
        button_Undo = GameObject.Find("Button_Undo").GetComponent<Button>();
        button_Undo.interactable = false;
        button_Create = GameObject.Find("Button_Create").GetComponent<Button>();
        button_Create.interactable = false;
        button_Train = GameObject.Find("Button_Train").GetComponent<Button>();
        button_Train.interactable = false;
        text_Notification = GameObject.Find("Text_Notification").GetComponent<Text>();
        text_GestureList = GameObject.Find("Text_GestureList").GetComponent<Text>();

        CreateGesture();    //creates an initial gesture. it will be removed when user input implements.

        text_Notification.text = $"Target Gesture : {GestureManager.Instance.CurrentGestureName}\n" +
                                    $"Samples : {GestureManager.Instance.CurrentSampleCount}\n" +
                                    "(At least 20 samples are recommended)\n\n" +
                                    "The sample will be recorded\n" +
                                    "while [Perform] is pressed.";
        text_GestureList.text = "";
    }

    public void StartGesture()
    {
        GestureManager.Instance.StartRead(testMode);
    }

    public void StopGesture()
    {
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

    public void CreateGesture()
    {
        testMode = false;

        GestureManager.Instance.Register();
        text_Notification.text = $"Target Gesture : {GestureManager.Instance.CurrentGestureName}\n" +
                                    $"Samples : {GestureManager.Instance.CurrentSampleCount}\n\n" +
                                    "(At least 20 are recommended.)";
    }

    public void TrainGesture()
    {
        if (GestureManager.Instance.TryTrain())
        {
            testMode = true;
            button_Undo.interactable = false;
            button_Train.interactable = false;

            text_Notification.text = "Training finished.\n\nPerform : Identify the recorded gesture.\nCreate : Add another gesture.";
            text_GestureList.text = "Recorded Gesture :\n";
            for (int i = 0; i < GestureManager.Instance.GestureCount; i++)
                text_GestureList.text += $"{GestureManager.Instance.GetGestureName(i)}, ";
            text_GestureList.text = text_GestureList.text.TrimEnd(',', ' ');
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
}
