using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ItemCategory { Items, Monsterballs, Tms}
public class Inventory : MonoBehaviour, ISavable
{
    [SerializeField] List<ItemSlot> slots;
    [SerializeField] List<ItemSlot> monsterballSlots;
    [SerializeField] List<ItemSlot> tmSlots;

    List<List<ItemSlot>> allSlots;

    public event Action OnUpdated;
    private void Awake()
    {
        allSlots = new List<List<ItemSlot>>()
        {
            slots, monsterballSlots, tmSlots
        };
    }
    public static List<string> ItemCategories {  get; set; } = new List<string>()
    {
        "ITEMS","MONSTER BALLS","TMs & HMs"
    };

    public List<ItemSlot> GetSlotsbyCategory(int categoryIndex)
    {
        return allSlots[categoryIndex];
    }
    public ItemBase GetItem(int itemIndex, int categoryIndex)
    {
        var currentSlots = GetSlotsbyCategory(categoryIndex);
        return currentSlots[itemIndex].Item;
    }

    public ItemBase UseItem(int itemIndex, Monsters selectedMonster, int selectedCategory)
    {
        var item = GetItem(itemIndex, selectedCategory);
        return UseItem(item, selectedMonster);
    }
    public ItemBase UseItem(ItemBase item, Monsters selectedMonster)
    {
        bool itemUsed = item.Use(selectedMonster);
        if (itemUsed)
        {
            if (!item.IsReusable)
            {
                RemoveItem(item);
            }
            return item;
        }

        return null;
    }

    public void AddItem(ItemBase item, int count = 1)
    {
        int category = (int)GetCategoryFromItem(item);
        var currentSlots = GetSlotsbyCategory(category);

        var itemSlot = currentSlots.FirstOrDefault(slot => slot.Item == item);
        if (itemSlot != null)
        {
            itemSlot.Count += count;
        }
        else
        {
            currentSlots.Add(new ItemSlot()
            {
                Item = item,
                Count = count
            });
        }

        OnUpdated?.Invoke();
    }
    public void RemoveItem(ItemBase item)
    {
        if (item == null) return;

        int category = (int)GetCategoryFromItem(item);
        var currentSlots = GetSlotsbyCategory(category);

        var itemSlot = currentSlots.First(slot => slot.Item == item);
        itemSlot.Count--;

        if (itemSlot.Count == 0)
        {
            currentSlots.Remove(itemSlot);
        }

        OnUpdated?.Invoke();
    }

    public bool HasItem(ItemBase item)
    {
        int category = (int)GetCategoryFromItem(item);
        var currentSlots = GetSlotsbyCategory(category);

        return currentSlots.Exists(slot => slot.Item == item);
    }

    private ItemCategory GetCategoryFromItem(ItemBase item)
    {
        if (item is RecoveryItem || item is EvolutonItem)
        {
            return ItemCategory.Items;
        }
        else if (item is MonsterballItem)
        {
            return ItemCategory.Monsterballs;
        }
        else
        {
            return ItemCategory.Tms;
        }
    }

    public static Inventory GetInventory()
    {
        return FindObjectOfType<PlayerController>().GetComponent<Inventory>();
    }

    public object CaptureState()
    {
        var saveData = new InventorySaveData()
        {
            items = slots.Select(i => i.GetSaveData()).ToList(),
            monsterballs = monsterballSlots.Select(i => i.GetSaveData()).ToList(),
            tms = tmSlots.Select(i => i.GetSaveData()).ToList(),
        };

        return saveData;
    }

    public void RestoreState(object state)
    {
        var saveData = state as InventorySaveData;

        slots = saveData.items.Select(i => new ItemSlot(i)).ToList();
        monsterballSlots = saveData.monsterballs.Select(i => new ItemSlot(i)).ToList();
        tmSlots = saveData.tms.Select(i => new ItemSlot(i)).ToList();

        allSlots = new List<List<ItemSlot>>()
        {
            slots, monsterballSlots, tmSlots
        };

        OnUpdated?.Invoke();
    }
}

[Serializable]
public class ItemSlot
{
    [SerializeField] ItemBase item;
    [SerializeField] int count;

    public ItemSlot()
    {

    }
    public ItemSlot(ItemSaveData saveData)
    {
        item = ItemDB.GetItemByname(saveData.name);
        count = saveData.count;
    }

    public ItemSaveData GetSaveData()
    {
        var saveData = new ItemSaveData()
        {
            name = item.Name,
            count = count
        };

        return saveData;
    }

    public ItemBase Item
    {
        get => item;
        set => item = value;
    }
    public int Count
    {
        get => count;
        set => count = value;
    }
}

[Serializable]
public class ItemSaveData
{
    public string name;
    public int count;
}

[Serializable]
public class InventorySaveData
{
    public List<ItemSaveData> items;
    public List<ItemSaveData> monsterballs;
    public List<ItemSaveData> tms;
}