using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    private Dictionary<Item, int> items;
    [SerializeField] private float weightLimit;
    private Transform npcTransform;
    private Item eqipedItem;
    [SerializeField] private EnviromentItemControll environment;

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

    public void AddItem(Item item, int amount)
    {
        var totalWeight = TotalWeight();
        if (totalWeight + item.Weight > weightLimit)
        {
            return;
        }

        items.TryAdd(item, amount);
        item.gameObject.SetActive(false); // Disable the game object when the item is added to the inventory
    }

    public void DropItem(Item item)
    {
        if (!items.ContainsKey(item))
        {
            return;
        }

        items[item] -= 1;
        if (items[item] == 0)
        {
            items.Remove(item);
        }
        item.transform.position = npcTransform.position + npcTransform.forward;
        item.gameObject.SetActive(true);
    }

    public void EquipItem(String itemName)
    {
        var item = GetItemByName(itemName);
        eqipedItem = item;
    }

    private float TotalWeight()
    {
        var totalWeight = items.Sum(entry => entry.Key.Weight * entry.Value);
        return totalWeight;
    }

    public void PickupItem(String itemName)
    {
        var itemsInRange = environment.GetItemsInRange(npcTransform.position, 2f);

        var itemsWithName = itemsInRange.Where(item => item.Name == itemName);

        if (itemsWithName.Any())
        {
            AddItem(itemsWithName.First(), 1);
        }
    }

    public Item GetItemByName(string itemName)
    {
        return items.Keys.FirstOrDefault(item => item.Name == itemName);
    }
}
