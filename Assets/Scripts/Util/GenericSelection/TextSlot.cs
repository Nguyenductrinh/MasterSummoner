using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextSlot : MonoBehaviour, ISlectableItem
{
    [SerializeField] TextMeshProUGUI text;
    Color originalColor;

    public void Init()
    {
        originalColor = text.color;
    }

    public void Clear()
    {
        text.color = originalColor;
    }

    public void OnSelectionChanged(bool selected)
    {
        text.color = (selected)? GobalSetting.i.Highlightcolor : originalColor;
    }

    public void SetText(string s)
    {
        text.text = s;
    }
}
