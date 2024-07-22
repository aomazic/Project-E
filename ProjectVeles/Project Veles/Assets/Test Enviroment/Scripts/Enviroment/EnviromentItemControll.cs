using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnviromentItemControll : MonoBehaviour
{
    private Dictionary<string, Item> items;

    private void Start()
    {
        items = new Dictionary<string, Item>();
        foreach (var item in FindObjectsOfType<Item>())
        {
            items[item.name] = item;
        }
    }

    public IEnumerable<Item> GetAllItemsInRange(Vector3 position, float range)
    {
        return items.Values.Where(item => item.gameObject.activeInHierarchy && Vector3.Distance(position, item.transform.position) <= range);
    }

    public Item GetItemByNameInRange(string itemName, Collider npcCollider)
    {
        if (items.TryGetValue(itemName, out var item) && item.gameObject.activeInHierarchy)
        {
            var itemCollider = item.gameObject.GetComponent<Collider>();
            if (npcCollider.bounds.Intersects(itemCollider.bounds))
            {
                return item;
            }
        }
        return null;
    }

    public Item GetItemByName(string name)
    {
        items.TryGetValue(name, out var item);
        return item;
    }
}
