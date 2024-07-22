using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Storage : MonoBehaviour
{
    private List<object> items = new List<object>();
    [SerializeField] private float maxWeight;
    private float currentWeight;

    public void AddItem(Item item)
    {
        currentWeight += item.Weight;
        if (currentWeight > maxWeight)
        {
            currentWeight -= item.Weight;
            return;
        }
        items.Add(item);
    }

    public bool RemoveItem(Item item)
    {
        currentWeight -= item.Weight;
        return items.Remove(item);
    }

    public List<object> GetItems()
    {
        return new List<object>(items);
    }
}
