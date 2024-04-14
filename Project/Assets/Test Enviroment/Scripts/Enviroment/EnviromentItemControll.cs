using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnviromentItemControll : MonoBehaviour
{
    private List<Item> items;

    private void Start()
    {
        items = new List<Item>(FindObjectsByType<Item>(FindObjectsSortMode.None));
    }

    public IEnumerable<Item> GetItemsInRange(Vector3 position, float range)
    {
        return items.Where(item => item.gameObject.activeInHierarchy && Vector3.Distance(position, item.transform.position) <= range);
    }

    public IEnumerable<Item> GetItemsByName(string name)
    {
        return items.Where(item => item.Name == name);
    }

}
