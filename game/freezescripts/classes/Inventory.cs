
// License by paralax (6/04/2023)

using System.Collections.Generic;

public class Inventory 
{
    public List<Item> Items { get; set; }
    public int Capacity { get; set; }
    public IPlayer Player { get; set; }

    public Inventory(int capacity) 
    {
        Items = new List<Item>();
        Capacity = capacity;
        Player = null;
    }

    public bool AddItem(Item item)
    {
        if (item == null)
        {
            return false;
        }

        // Проверяем, есть ли уже такой предмет в инвентаре
        Item existingItem = null;
        foreach (var _item in Items)
        {
            if (_item.ItemName == item.ItemName && _item.ItemCount != _item.ItemMaxCount)
                existingItem = _item;
        }

        if (existingItem != null)
        {
            // Если уже есть такой предмет, то добавляем количество добавляемых предметов к уже существующему предмету, если это возможно без превышения максимального количества
            int availableCount = existingItem.ItemMaxCount - existingItem.ItemCount;
            if (availableCount >= item.ItemCount)
            {
                existingItem.ItemCount += item.ItemCount;
                item.RemoveFromWorld();
                return true;
            }
            else
            {
                existingItem.ItemCount += availableCount;
                item.ItemCount -= availableCount;
            }
        }

        // Если количество добавляемых предметов еще осталось, то добавляем их в следующий свободный слот в инвентаре, если он есть
        if (Items.Count < Capacity)
        {
            Items.Add(item);
            item.RemoveFromWorld();
            item.InInventory = true;
            item.Inventory = this;
            item.ItemPosition = Items.Count - 1;
            return true;
        }

        return false;
    }

    public void RemoveItem(Item item) 
    {
        Items.Remove(item);
        item.InInventory = false;
        item.Inventory = null;
    }

    public void SwapItems(Item item1, Item item2) 
    {
        int index1 = Items.IndexOf(item1);
        int index2 = Items.IndexOf(item2);
        Items[index1] = item2;
        Items[index2] = item1;
    }

    public void MoveItem(Item item, int newIndex)
    {
        if (!ContainsItemAt(newIndex, item))
        {
            Items.Insert(newIndex, item);
        }
        else
        {
            SwapItems(item, Items[newIndex]);
        }
    }

    public bool ContainsItemAt(int index, Item item)
    {
        if (index < 0 || index >= Items.Count)
            return false;

        return Items[index] == item;
    }

    public bool ContainsItem(Item item)
    {
        return Items.Contains(item);
    }

    public bool HasFreeSpace() 
    {
        return Items.Count < Capacity;
    }
}