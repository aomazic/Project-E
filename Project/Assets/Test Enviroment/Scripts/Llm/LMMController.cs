using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Sentis;

public class LMMController : MonoBehaviour
{
    public class ActionData
    {
        public string type;
        public string item;
    }

    public class ResponseData
    {
        public string response;
        public ActionData action;
    }

    private string testEquipContainerInput = @"{
    ""response"": ""A moment's respite is due. A visit to the tavern with a trusted companion may yield fruitful conversation."",
    ""action"": {
        ""type"": ""equip"",
        ""item"": ""waterContainer""
        }
    }";

    private string testUnequipContainerInput = @"{
    ""response"": ""The weight of the container is too much to bear. It is best to unequip it."",
    ""action"": {
        ""type"": ""unequip"",
        ""item"": ""waterContainer""
        }
    }";

    private string testDrinkWaterInput = @"{
    ""response"": ""The water is refreshing. It is best to drink it slowly."",
    ""action"": {
        ""type"": ""drink"",
        ""item"": ""waterContainer""
        }
    }";

    private string testDropContainerInput = @"{
    ""response"": ""The container is dropped."",
    ""action"": {
        ""type"": ""drop"",
        ""item"": ""waterContainer""
        }
    }";

    private string testPickupContainer = @"{
    ""response"": ""Ti should pick up the container."",
    ""action"": {
        ""type"": ""pickup"",
        ""item"": ""waterContainer""
        }
    }";


    private List<string> testInputs;
    private NpcController npcController;
    private Inventory inventory;
    private ResponseData ParseResponseData(string jsonString)
    {
        return JsonUtility.FromJson<ResponseData>(jsonString);
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
            testPickupContainer
        };
        StartCoroutine(testRun());
   //     StartCoroutine(ParseRandomInput());

    }
    private IEnumerator testRun()
    {
        yield return new WaitForSeconds(2);
        inventory.PickupItem("waterContainer");
        inventory.EquipItem("waterContainer");
        npcController.DrinkItem("waterContainer");
    }

    private IEnumerator ParseRandomInput()
    {
        while (true)
        {
            yield return new WaitForSeconds(2);

            string randomInput = testInputs[UnityEngine.Random.Range(0, testInputs.Count)];

            ResponseData data = ParseResponseData(randomInput);
            PerformAction(data.action);

        }
    }

    private void PerformAction(ActionData action)
    {
        switch (action.type)
        {
            case "equip":
                inventory.EquipItem(action.item);
                break;
            case "unequip":
                Debug.Log("Unequip");
                break;
            case "drink":
                npcController.DrinkItem(action.item);
                break;
            case "drop":
                Debug.Log("Drop");
                break;
            case "pickup":
                inventory.PickupItem(action.item);
                break;
            default:
                Debug.LogError("Unknown action type: " + action.type);
                break;
        }
    }

}
