using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class ButtonHandler : MonoBehaviour
{
    public Button Button_Perform, Button_Undo, Button_Edit, Button_Train;    //for toggle "interactable"
    public Text text_Notification, text_GestureList;
    public GameObject Dialog_Add, Dialog_Edit;
    public RectTransform GestureListViewContent;

    private GameObject gestureItemPrefab;
    private bool isIdentificationMode = false;

    private delegate void ModeChangedHandler();
    private event ModeChangedHandler ModeChanged;

    public Image defaultimg;
    public Sprite offimage;
    public Sprite onimage;

    private void Start()
    {
        gestureItemPrefab = Resources.Load<GameObject>("Prefabs/GestureItem");

        ModeChanged += OnModeChanged;
        GestureManager.Instance.OnTrainingInProgress += NotifyTrainingInProgress;
        GestureManager.Instance.OnTrainingCompleted += NotifyTrainingCompleted;

        //Button_Train.interactable = false;
        //Button_Perform.interactable = false;
        //Button_Undo.interactable = false;

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
        if (!Button_Perform.interactable)
            return;
        ChangeImage(true);

        GestureManager.Instance.StartRead(isIdentificationMode);
    }

    public void StartGesture(int index)
    {
        if (!Button_Perform.interactable)
            return;
        ChangeImage(true);

        GestureManager.Instance.StartReadAt(index);
    }

    public void StopGesture()
    {
        if (!Button_Perform.interactable)
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
            Button_Undo.interactable = true;
            Button_Edit.interactable = true;
            Button_Train.interactable = true;

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

    public void OpenEditDialog()
    {
        Dialog_Edit.SetActive(true);
        UpdateGestureListView();
    }

    public void CloseEditDialog() => Dialog_Edit.SetActive(false);

    public void OpenAddDialog() => Dialog_Add.SetActive(true);

    public void CloseAddDialog() => Dialog_Add.SetActive(false);

    private void UpdateGestureListView()
    {
        var gesture = GestureManager.Instance.GestureList;
        int count = GestureManager.Instance.GestureList.Count;

        // First, remove previous children of list.
        foreach (Transform child in GestureListViewContent)
            Destroy(child.gameObject);

        // Then, load the gestures from GM.
        for (int i = 0; i < count; i++)
        {
            var gestureItem = Instantiate(gestureItemPrefab).transform;
            var name = gestureItem.Find("Text_Name").GetComponent<Text>();
            var sampleCount = gestureItem.Find("Text_SampleCount").GetComponent<Text>();
            var button_AddAudio = gestureItem.Find("Button_AddAudio").GetComponent<Button>();
            var button_Select = gestureItem.Find("Button_Select").GetComponent<Button>();
            var button_Delete = gestureItem.Find("Button_Delete").GetComponent<Button>();

            name.text = gesture[i].Name;
            sampleCount.text = $"Samples : {gesture[i].SampleCount}";

            button_AddAudio.onClick.AddListener(() =>
            {

            });

            button_Select.onClick.AddListener(() =>
            {

            });

            button_Delete.onClick.AddListener(() =>
            {

            });

            gestureItem.SetParent(GestureListViewContent, false);
        }
    }

    public void AddGesture()
    {
        var inputfield_GestureName = Dialog_Add.transform.GetComponentInChildren<InputField>();

        // Not allow null or whitespace as the gesture name.
        if (String.IsNullOrWhiteSpace(inputfield_GestureName.text))
            return;

        GestureManager.Instance.Register(inputfield_GestureName.text);
        inputfield_GestureName.text = "";
        CloseAddDialog();

        UpdateGestureListView();

        //Button_Perform.interactable = true;
        //isIdentificationMode = false;
        //ModeChanged();

        //var currentGesture = GestureManager.Instance.GetRecentGesture;
        //text_Notification.text = $"Target Gesture : {currentGesture.Name}\n" +
        //                            $"Samples : {currentGesture.SampleCount}\n\n" +
        //                            "(At least 20 are recommended.)";
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

        Button_Perform.interactable = false;
        Button_Edit.interactable = false;
        Button_Undo.interactable = false;
        Button_Train.interactable = false;
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

        Button_Perform.interactable = true;
        Button_Edit.interactable = true;

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
            Button_Perform.interactable = true;
            isIdentificationMode = true;
            ModeChanged();

            text_Notification.text = "Loading trained data succeeded.\n";
            //text_GestureList.text = "Recorded Gesture :\n";
            //for (int i = 0; i < GestureManager.Instance.GestureList.Count; i++)
            //    text_GestureList.text += $"{GestureManager.Instance.GestureList[i].Name}, ";
            //text_GestureList.text = text_GestureList.text.TrimEnd(',', ' ');
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
            Button_Perform.interactable = true;
            isIdentificationMode = true;
            ModeChanged();

            text_Notification.text = "Loading trained data succeeded.\n";
            //text_GestureList.text = "Recorded Gesture :\n";
            //for (int i = 0; i < GestureManager.Instance.GestureList.Count; i++)
            //    text_GestureList.text += $"{GestureManager.Instance.GestureList[i].Name}, ";
            //text_GestureList.text = text_GestureList.text.TrimEnd(',', ' ');
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
