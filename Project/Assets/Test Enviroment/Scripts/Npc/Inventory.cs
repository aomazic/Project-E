using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    private Dictionary<Item, int> items;
    [SerializeField] private float weightLimit;
    private Transform npcTransform;
    private Item eqipedItem;
    [SerializeField] private EnviromentItemControll environment;
    [SerializeField] private float itemInteractionRange = 2f;

    private void Start()
    {
        items = new Dictionary<Item, int>();
        npcTransform = GetComponent<Transform>();
    }

    private void Update()
    {
        if (eqipedItem is not null)
        {
            var offset = npcTransform.forward * 1.5f;
            eqipedItem.transform.position = npcTransform.position + offset;
        }
    }

    private void AddItem(Item item, int amount)
    {
        var totalWeight = TotalWeight();
        if (totalWeight + item.Weight > weightLimit)
        {
            return;
        }

        items.TryAdd(item, amount);
        item.gameObject.SetActive(false); // Disable the game object when the item is added to the inventory
    }

    public void DropItem(string itemName)
    {
        var item = GetItemByName(itemName);
        if (item is null)
        {
            return;
        }
        item.transform.position = npcTransform.position + npcTransform.forward;
        item.gameObject.SetActive(true);
    }

    public void EquipItem(String itemName)
    {
        var item = GetItemByName(itemName);
        if (item is null)
        {
            return;
        }
        item.gameObject.SetActive(true);
        eqipedItem = item;
    }

    private float TotalWeight()
    {
        var totalWeight = items.Sum(entry => entry.Key.Weight * entry.Value);
        return totalWeight;
    }

    public void PickupItem(string itemName)
    {
        var itemsInRange = environment.GetItemsByNameInRange(itemName, npcTransform.position, itemInteractionRange);
        var firstItem = itemsInRange.FirstOrDefault();

        if (firstItem)
        {
            AddItem(firstItem, 1);
        }
    }

    public void TransferLiquid(string targetLiquid, string sourceLiquid, float volume)
    {
        var target = environment.GetItemByName(targetLiquid) as LiquidStorage;
        var source = environment.GetItemByName(sourceLiquid) as LiquidStorage;

        if (!target || !source)
        {
            Debug.LogError("Source or target is not of type LiquidStorage");
            return;
        }

        if (eqipedItem != source && eqipedItem != target)
        {
            Debug.LogError("At least one of the items should be equipped");
            return;
        }

        if (ItemExistsInInventory(sourceLiquid) && !ItemExistsInRange(sourceLiquid, 2f) &&
            ItemExistsInInventory(targetLiquid) && !ItemExistsInRange(targetLiquid, 2f))
        {
            Debug.LogError("At least one of the items should be in the environment and within range");
            return;
        }

        source.TransferLiquid(target, volume);
    }

    public void UnequipItem()
    {
        eqipedItem = null;
    }

    public Item GetItemByName(string itemName)
    {
        return items.Keys.FirstOrDefault(item => item.ItemName == itemName);
    }

    private bool ItemExistsInInventory(string itemName)
    {
        return items.Keys.Any(item => item.ItemName == itemName);
    }

    private bool ItemExistsInRange(string itemName, float range)
    {
        var itemsInRange = environment.GetItemsByNameInRange(itemName, npcTransform.position, range);
        return itemsInRange.Any();
    }
}
