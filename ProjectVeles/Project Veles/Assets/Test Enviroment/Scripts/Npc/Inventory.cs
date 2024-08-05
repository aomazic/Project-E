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
    private NpcController npcController;
    private HungerControll hungerControll;

    private void Start()
    {
        items = new Dictionary<Item, int>();
        npcTransform = GetComponent<Transform>();
        hungerControll = GetComponent<HungerControll>();
        npcController = GetComponent<NpcController>();
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
            Debug.LogError("Item not found");
            npcController.memoryDb.genericObsevation(transform.name, $"Tried to drop {itemName} but it is not in the inventory", 5f);
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
        npcController.memoryDb.genericObsevation(transform.name, $"Equipped {itemName}", 5f);
    }

    private float TotalWeight()
    {
        var totalWeight = items.Sum(entry => entry.Key.Weight * entry.Value);
        return totalWeight;
    }

    public void PickupItem(string itemName)
    {
        var item = npcController.itemEnvironmentControll.GetItemByNameInRange(itemName, npcController.itemInteractCollider);

        if (item)
        {
            AddItem(item, 1);
            npcController.memoryDb.genericObsevation(transform.name, $"Picked up {itemName}", 5f);
        }
        else
        {
            Debug.LogError("Item not found");
            npcController.memoryDb.genericObsevation(transform.name, $"Tried to pick up {itemName} but it is not in the vicinity", 5f);
        }
    }


    public void TransferLiquid(string targetLiquid, string sourceLiquid, float volume)
    {
        var target = npcController.itemEnvironmentControll.GetItemByNameInRange(targetLiquid, npcController.itemInteractCollider) as LiquidStorage;
        var source = npcController.itemEnvironmentControll.GetItemByNameInRange(sourceLiquid, npcController.itemInteractCollider) as LiquidStorage;

        if (!target || !source)
        {
            Debug.LogError("Source or target liquid storage not found");
            return;
        }

        if (eqipedItem != source && eqipedItem != target)
        {
            Debug.LogError("At least one of the items should be equipped");
            return;
        }

        source.TransferLiquid(target, volume);
        npcController.memoryDb.genericObsevation(transform.name, $"Transferred {volume} from {sourceLiquid} to {targetLiquid}", 5f);
    }

    public void EatFood(string itemName)
    {
        var foodItem = npcController.itemEnvironmentControll.GetItemByNameInRange(itemName, npcController.itemInteractCollider) as FoodItem;
        if (!foodItem)
        {
            npcController.memoryDb.genericObsevation(transform.name, $"Tried to eat {itemName} but it is not in the vicinity", 5f);
            Debug.LogError("Food item not found");
            return;
        }
        hungerControll.Eat(foodItem);
    }

    public void UnequipItem()
    {
        if (eqipedItem)
        {
            var itemName = eqipedItem.name;
            eqipedItem = null;
            npcController.memoryDb.genericObsevation(transform.name, $"Unequipped {itemName}", 5f);
        }
    }
    public Item GetItemByName(string itemName)
    {
        return items.Keys.FirstOrDefault(item => item.name == itemName);
    }

    private bool ItemExistsInInventory(string itemName)
    {
        return items.Keys.Any(item => item.name == itemName);
    }

}
