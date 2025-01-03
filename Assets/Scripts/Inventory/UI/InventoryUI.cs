using GDE.GenericSelectionUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : SelectionUI<TextSlot>
{
    [SerializeField] GameObject itemList;
    [SerializeField] ItemSlotUI itemSlotUI;

    [SerializeField] Image itemIcon;
    [SerializeField] TextMeshProUGUI itemDescription;
    [SerializeField] TextMeshProUGUI categoryText;

    [SerializeField] Image upArrown;
    [SerializeField] Image downArrown;

    int slectedCategory = 0;

    const int itemInViewport = 8;

    List<ItemSlotUI> slotUIList;
    Inventory inventory;
    RectTransform itemListRect;

    private void Awake()
    {
        inventory = Inventory.GetInventory();
        itemListRect = itemList.GetComponent<RectTransform>();
    }

    private void Start()
    {
        UpdateItemList();

        inventory.OnUpdated += UpdateItemList;
    }
    void UpdateItemList()
    {
        // Clear all the existing item
        foreach(Transform child in itemList.transform)
        {
            Destroy(child.gameObject);
        }
        slotUIList = new List<ItemSlotUI>();
        foreach(var itemSlot in inventory.GetSlotsbyCategory(slectedCategory))
        {
            var slotUIObj = Instantiate(itemSlotUI, itemList.transform);
            slotUIObj.SetData(itemSlot);

            slotUIList.Add(slotUIObj);
        }

        SetItems(slotUIList.Select(s => s.GetComponent<TextSlot>()).ToList());

        UpdateSelectionInUI();
    }

    public override void HandleUpdate()
    {
        int prevCategory = slectedCategory;
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ++slectedCategory;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --slectedCategory;
        }

        if (slectedCategory > Inventory.ItemCategories.Count - 1)
        {
            slectedCategory = 0;
        }
        else if (slectedCategory < 0)
        {
            slectedCategory = Inventory.ItemCategories.Count - 1;
        }

        if (prevCategory != slectedCategory)
        {
            ResetSelection();
            categoryText.text = Inventory.ItemCategories[slectedCategory];
            UpdateItemList();
        }

        base.HandleUpdate();
    }
    public override void UpdateSelectionInUI()
    {
        base.UpdateSelectionInUI();

        var slots = inventory.GetSlotsbyCategory(slectedCategory);

        if (slots.Count > 0)
        {
            var item = slots[selectedItem].Item;
            itemIcon.sprite = item.Icon;
            itemDescription.text = item.Description;
        }

        HadleScrolling();
    }

    void HadleScrolling()
    {
        if(slotUIList.Count <= itemInViewport )
        {
            return;
        }

        float scrollPos = Mathf.Clamp(selectedItem - itemInViewport,0, selectedItem) * slotUIList[0].Height;
        itemListRect.localPosition = new Vector2(itemListRect.localPosition.x, scrollPos);

        bool showUpArrow = selectedItem > itemInViewport / 2;
        upArrown.gameObject.SetActive(showUpArrow);

        bool showDownArrown = selectedItem + itemInViewport / 2 < slotUIList.Count;
        downArrown.gameObject.SetActive(showDownArrown);
    }

    void ResetSelection()
    {
        selectedItem = 0;

        upArrown.gameObject.SetActive(false);
        downArrown.gameObject.SetActive(false);

        itemIcon.sprite = null;
        itemDescription.text = "";
    }

    public ItemBase SelectedItem => inventory.GetItem(selectedItem, slectedCategory);
    public int SelectedCategory => slectedCategory;
}
