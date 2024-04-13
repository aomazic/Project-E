using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Storage : MonoBehaviour
{
    private List<object> items = new List<object>();

    public void AddItem(object item)
    {
        items.Add(item);
    }

    public bool RemoveItem(object item)
    {
        return items.Remove(item);
    }

    public List<object> GetItems()
    {
        return new List<object>(items);
    }
}
