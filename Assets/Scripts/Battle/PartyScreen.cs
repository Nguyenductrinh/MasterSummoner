using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using GDE.GenericSelectionUI;
using System.Linq;

public class PartyScreen : SelectionUI<TextSlot>
{
    [SerializeField] TextMeshProUGUI messageText;

    PartyMemberUI[] memberSlots;

    List<Monsters> monsters;
    MonsterParty party;

    public Monsters SelectedMember => monsters[selectedItem];

    public void Init()
    {
        memberSlots = GetComponentsInChildren<PartyMemberUI>(true);
        SetSelectionSettings(SelectionType.Grid, 2);

        party = MonsterParty.GetPlayerParty();
        SetPartyData(party.Monster);

        party.OnUpdated += OnPartyUpdated;
    }
    public void SetPartyData(List<Monsters> monsters = null)
    {
        this.monsters = monsters ?? party.Monster; // Nếu `monsters` là null, lấy từ `party.Monster`.

        for (int i = 0; i < memberSlots.Length; i++)
        {
            if (i < this.monsters.Count)
            {
                memberSlots[i].gameObject.SetActive(true);
                memberSlots[i].Init(this.monsters[i]);
            }
            else
            {
                memberSlots[i].gameObject.SetActive(false);
            }
        }

        var textSlot = memberSlots.Select(m => m.GetComponent<TextSlot>()).ToList();
        SetItems(textSlot.Take(this.monsters.Count).ToList());

        messageText.text = "Choose a Monster";
    }
    public void ShowIfIsUsable(TmItems tmItem)
    {
        for(int i = 0; i<monsters.Count;i++)
        {
            string message = tmItem.CanBeTaught(monsters[i]) ? "ABLE!" : "NOT ABLE!";
            memberSlots[i].SetMessage(message);
        } 
    }
    public void ClearMemberSlotMessage()
    {
        for (int i = 0; i < monsters.Count; i++)
        {
            memberSlots[i].SetMessage("");
        }
    }
    public void SetMessageText(string message)
    {
        messageText.text = message;
    }
    void OnPartyUpdated()
    {
        // Cập nhật lại khi party được cập nhật
        SetPartyData(party.Monster);
    }
}
