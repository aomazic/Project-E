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
        public ActionDetail ActionDetail { get; set; }
        public int Duration { get; set; }
    }

    private class ParsedObject
    {
        public string Response { get; set; }
        public ResponseAction Action { get; set; }
    }

    private class ActionDetail
    {
        public string Source { get; set; }
        public string Target { get; set; }
    }

    private string testEquipContainerInput = @"{
    ""response"": ""A moment's respite is due. A visit to the tavern with a trusted companion may yield fruitful conversation."",
    ""action"": {
        ""type"": ""equip"",
        ""actionDetail"": {
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
        ""actionDetail"": {
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
        ""actionDetail"": {
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
        ""actionDetail"": {
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
        ""actionDetail"": {
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
        ""actionDetail"": {
            ""source"": ""waterSource"",
            ""target"": ""waterContainer""
            },
        ""duration"": 2
        }
    }";

    private string testEatFood = @"{
     ""response"": ""Time to eat some food"",
        ""action"": {
            ""type"": ""eat"",
            ""actionDetail"": {
                ""source"": ""batak"",
                ""target"": ""null""
                },
            ""duration"": 1
            }
        }";

    private string testStartRest = @"{
     ""response"": ""Time for a quickNap"",
        ""action"": {
            ""type"": ""useItem"",
            ""actionDetail"": {
                ""source"": ""bench"",
                ""target"": ""null""
                },
            ""duration"": 1
            }
        }";

    private string testDialogue = @"{
     ""response"": ""Are you sure?"",
        ""action"": {
            ""type"": ""speak"",
            ""actionDetail"": {
                ""source"": ""null"",
                ""target"": ""null""
                },
            ""duration"": 2
            }
        }";

    private string testGoTo = @"{
     ""response"": ""I should go to the market"",
        ""action"": {
            ""type"": ""goTo"",
            ""actionDetail"": {
                ""source"": ""tavern"",
                ""target"": ""null""
                },
            ""duration"": 1
            }
        }";
    private List<string> testInputs;
    private NpcController npcController;
    private Inventory inventory;
    private EnergyControll energyControll;
    private DialogueController dialogueController;
    private Pathfinding pathfinding;
    private ParsedObject ParseResponseData(string jsonString)
    {
        return JsonConvert.DeserializeObject<ParsedObject>(jsonString);
    }


    private void Start()
    {
        inventory = GetComponent<Inventory>();
        npcController = GetComponent<NpcController>();
        energyControll = GetComponent<EnergyControll>();
        dialogueController = GetComponent<DialogueController>();
        pathfinding = GetComponent<Pathfinding>();
        testInputs = new List<string>
        {
            testEquipContainerInput,
            testUnequipContainerInput,
            testDrinkWaterInput,
            testDropContainerInput,
            testPickupContainer,
            testFillContainer,
            testEatFood,
            testStartRest,
            testDialogue,
            testGoTo
        };
        StartCoroutine(testRun());
        StartCoroutine(ParseRandomInput());

    }
    private IEnumerator testRun()
    {
        yield return new WaitForSeconds(2);
        dialogueController.Speak(new Dialogue(npcController.Name, "Hello"));
        pathfinding.goTo("market");
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
                inventory.EquipItem(action.ActionDetail.Source);
                Debug.Log("Equip");
                break;
            case "unequip":
                inventory.UnequipItem();
                Debug.Log("Unequip");
                break;
            case "drink":
                Debug.Log("Drink");
                npcController.DrinkItem(action.ActionDetail.Source);
                break;
            case "drop":
                Debug.Log("Drop");
                inventory.DropItem(action.ActionDetail.Source);
                break;
            case "pickup":
                Debug.Log("Pickup");
                inventory.PickupItem(action.ActionDetail.Source);
                break;
            case "transfer":
                Debug.Log("Transfer");
                inventory.TransferLiquid(action.ActionDetail.Target, action.ActionDetail.Source, 1);
                break;
            default:
                Debug.LogError("Unknown action type: " + action.Type);
                break;
        }
    }

}
