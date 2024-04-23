using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using Unity.Sentis;

public class LMMController : MonoBehaviour
{
    private class ResponseAction
    {
        public string Type { get; set; }
        public ItemData Item { get; set; }
        public int Duration { get; set; }
    }

    private class ParsedObject
    {
        public string Response { get; set; }
        public ResponseAction Action { get; set; }
    }

    private class ItemData
    {
        public string Source { get; set; }
        public string Target { get; set; }
    }

    private string testEquipContainerInput = @"{
    ""response"": ""A moment's respite is due. A visit to the tavern with a trusted companion may yield fruitful conversation."",
    ""action"": {
        ""type"": ""equip"",
        ""item"": {
            ""source"": ""waterContainer"",
            ""target"": null
            },
        ""duration"": 1
        }
    }";

    private string testUnequipContainerInput = @"{
    ""response"": ""The weight of the container is too much to bear. It is best to unequip it."",
    ""action"": {
        ""type"": ""unequip"",
        ""item"": {
            ""source"": ""equipedItem"",
            ""target"": null
            },
        ""duration"": 1
        }
    }";

    private string testDrinkWaterInput = @"{
    ""response"": ""The water is refreshing. It is best to drink it slowly."",
    ""action"": {
        ""type"": ""drink"",
        ""item"": {
            ""source"": ""waterContainer"",
            ""target"": null
            },
        ""duration"": 1
        }
    }";

    private string testDropContainerInput = @"{
    ""response"": ""The container is dropped."",
    ""action"": {
        ""type"": ""drop"",
        ""item"": {
            ""source"": ""waterContainer"",
            ""target"": null
            },
        ""duration"": 1
        }
    }";

    private string testPickupContainer = @"{
    ""response"": ""I should pick up the container."",
    ""action"": {
        ""type"": ""pickup"",
        ""item"": {
            ""source"": ""waterContainer"",
            ""target"": null
            },
        ""duration"": 1
        }
    }";

    private string testFillContainer = @"{
    ""response"": ""The container is filled with water."",
    ""action"": {
        ""type"": ""transfer"",
        ""item"": {
            ""source"": ""waterSource"",
            ""target"": ""waterContainer""
            },
        ""duration"": 2
        }
    }";



    private List<string> testInputs;
    private NpcController npcController;
    private Inventory inventory;
    private ParsedObject ParseResponseData(string jsonString)
    {
        return JsonConvert.DeserializeObject<ParsedObject>(jsonString);
    }


    private void Start()
    {
        inventory = GetComponent<Inventory>();
        npcController = GetComponent<NpcController>();
        testInputs = new List<string>
        {
            testEquipContainerInput,
            testUnequipContainerInput,
            testDrinkWaterInput,
            testDropContainerInput,
            testPickupContainer,
            testFillContainer
        };
        StartCoroutine(testRun());
        StartCoroutine(ParseRandomInput());

    }
    private IEnumerator testRun()
    {
        yield return new WaitForSeconds(2);
        inventory.PickupItem("waterContainer");
        inventory.EquipItem("waterContainer");
        inventory.TransferLiquid("waterContainer", "waterSource", 1);
    }

    private IEnumerator ParseRandomInput()
    {
        while (true)
        {
            yield return new WaitForSeconds(5);

            var randomInput = testInputs[UnityEngine.Random.Range(0, testInputs.Count)];

            ParsedObject data = ParseResponseData(randomInput);
            PerformAction(data.Action);

        }
    }

    private void PerformAction(ResponseAction action)
    {
        switch (action.Type)
        {
            case "equip":
                inventory.EquipItem(action.Item.Source);
                Debug.Log("Equip");
                break;
            case "unequip":
                inventory.UnequipItem();
                Debug.Log("Unequip");
                break;
            case "drink":
                Debug.Log("Drink");
                npcController.DrinkItem(action.Item.Source);
                break;
            case "drop":
                Debug.Log("Drop");
                inventory.DropItem(action.Item.Source);
                break;
            case "pickup":
                Debug.Log("Pickup");
                inventory.PickupItem(action.Item.Source);
                break;
            case "transfer":
                Debug.Log("Transfer");
                inventory.TransferLiquid(action.Item.Target, action.Item.Source, 1);
                break;
            default:
                Debug.LogError("Unknown action type: " + action.Type);
                break;
        }
    }

}
