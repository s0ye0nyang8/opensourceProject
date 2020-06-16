using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GestureEditor : MonoBehaviour
{
    public GameObject gestureItemPrefab;

    void Start()
    {
        gestureItemPrefab = Resources.Load<GameObject>("Prefabs/GestureItem");
    }

    public void LoadGestureListView()
    {
        var listViewContent = GameObject.Find("Content").GetComponent<RectTransform>().transform;
        var gesture = GestureManager.Instance.GestureList;
        int count = GestureManager.Instance.GestureList.Count;

        for (int i = 0; i < count; i++)
        {
            var gestureItem = Instantiate(gestureItemPrefab).transform;
            var name = gestureItem.Find("Text_Name").GetComponent<Text>().text;
            var sampleCount = gestureItem.Find("Text_SampleCount").GetComponent<Text>().text;
            var addSample = gestureItem.Find("Button_AddSample").GetComponent<Button>();
            var delete = gestureItem.Find("Button_Delete").GetComponent<Button>();

            name = gesture[i].Name;
            sampleCount = gesture[i].SampleCount.ToString();
            addSample.onClick.AddListener(() =>
            {

            });

            gestureItem.SetParent(listViewContent);
        }
    }
}
