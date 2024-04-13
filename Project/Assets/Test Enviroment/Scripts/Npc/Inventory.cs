using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public Dictionary<Item, int> items;
    [SerializeField] float weightLimit;
    private Transform npcTransform;

    private void Start()
    {
        items = new Dictionary<Item, int>();
        npcTransform = GetComponent<Transform>();
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

    public void EquipItem(Item item)
    {
        if (!items.ContainsKey(item))
        {
            return;
        }

        item.gameObject.SetActive(true); // Enable the game object when the item is equipped
    }

    private float TotalWeight()
    {
        var totalWeight = items.Sum(entry => entry.Key.Weight * entry.Value);
        return totalWeight;
    }
}
