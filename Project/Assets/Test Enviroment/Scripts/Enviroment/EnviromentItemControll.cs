using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class EnviromentItemControll : MonoBehaviour
{
    private List<Item> items;

    private void Start()
    {
        items = new List<Item>(FindObjectsByType<Item>(FindObjectsSortMode.None));
    }

    public IEnumerable<Item> GetAllItemsInRange(Vector3 position, float range)
    {
        return items.Where(item => item.gameObject.activeInHierarchy && Vector3.Distance(position, item.transform.position) <= range);
    }

    public IEnumerable<Item> GetItemsByNameInRange(string itemName, Vector3 position, float range)
    {
        return items.Where(item => item.ItemName == itemName && item.gameObject.activeInHierarchy && Vector3.Distance(position, item.transform.position) <= range);
    }

    public Item GetItemByName(string name)
    {
        return items.FirstOrDefault(item => item.ItemName == name);
    }

}
