//using MonstersNamespace;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TMPro.Examples;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] HPBar hpBar;
    [SerializeField] TextMeshProUGUI messageText;

    Monsters _monsters;
    public void Init(Monsters monsters)
    {
        _monsters = monsters;
        UpdateData();
        SetMessage("");
        _monsters.OnHPChanged += UpdateData;
    }

    void UpdateData()
    {
        nameText.text = _monsters.Base.Name;
        levelText.text = "Lv" + _monsters.Level;
        hpBar.SetHP((float)_monsters.HP / _monsters.MaxHp);
    }

    //public void SetSelected(bool selected)
    //{
    //    if(selected)
    //    {
    //        nameText.color =GobalSetting.i.Highlightcolor;
    //    }
    //    else
    //    {
    //        nameText.color = Color.black;
    //    }
    //}

    public void SetMessage(string message)
    {
        messageText.text = message;
    }
}
