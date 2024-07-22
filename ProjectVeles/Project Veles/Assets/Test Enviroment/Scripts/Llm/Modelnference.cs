using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using LLMUnity;
using NUnit.Framework;
using Test_Enviroment.Scripts.Llm;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModelInference : MonoBehaviour
{
    [SerializeField] private LLMCharacter llm;
    [SerializeField] TextMeshProUGUI npcTextBox;
    private string prompt = "";
    public ModelInference recipient;
    private string reply;
    private NpcController npcController;
    private Inventory inventory;
    private EnergyControll energyControll;
    private Pathfinding pathfinding;
    private LocationManager locationManager;
    [SerializeField] private string systemPrompt = "Hmmm what to do next?";
    [SerializeField] private TimeManager timeManager;
    [SerializeField] MemoryDatabase memoryDb;
    List<string> importantRecords = new List<string>();
    [TextArea] 
    [SerializeField] private string worldInfo = "Key locations include the Kitchen, Main Hall, Main Hallway,Hallway to Ban House, Hallway to Jan house,Jan House , and Ban House.";

    public ModelInference(TextMeshProUGUI npcTextBox, LLMClient llm)
    {
        this.npcTextBox = npcTextBox;
        this.llm = llm;
    }

    private void Start()
    {
        npcController = GetComponent<NpcController>();
        inventory = GetComponent<Inventory>();
        energyControll = GetComponent<EnergyControll>();
        pathfinding = GetComponent<Pathfinding>();
        locationManager = GetComponent<LocationManager>();
        prompt = worldInfo;
        WorldInfoInference();
        Inference();
    }
    
    private void PerformAction(ResponseAction action)
    {
        string target = action.Target.ToLower();
        switch (action.Type)
        {
            case "equip":
                inventory.EquipItem(target.ToLower());
                Debug.Log("Equip");
                break;
            case "unequip":
                inventory.UnequipItem();
                Debug.Log("unequip");
                break;
            case "drink":
                Debug.Log("drink");
                npcController.DrinkItem(target.ToLower());
                break;
            case "drop":
                Debug.Log("drop");
                inventory.DropItem(target.ToLower());
                break;
            case "pickup":
                Debug.Log("pickup");
                inventory.PickupItem(target.ToLower());
                break;
            case "eat":
                Debug.Log("eat");
                inventory.EatFood(target.ToLower());
                break;
            case "rest":
                Debug.Log("rest");
                energyControll.EnterRest(target.ToLower());
                break;
            case "leaveRest":
                Debug.Log("leaveRest");
                energyControll.LeaveRest();
                break;
            case "talkto":
                Debug.Log("talkto");
                SetRecipient(target.ToLower());
                break;
            case "goto":
                Debug.Log("goto");
                pathfinding.GoTo(target.ToLower());
                break;
            default:
                recipient = null;
                Debug.LogError("Unknown action type: " + action.Type);
                break;
        }
    }
    
    private void WorldInfoInference()
    {
        prompt = worldInfo;
        npcTextBox.text = "";
        _ = llm.Chat(prompt, HandleReply, ReplyCompleted);
    }
    private void Inference()
    {
        importantRecords = memoryDb.FetchImportantRecords(3, npcController.name);
        prompt = ResponseParser.ConstructPrompt(new Prompt(
            locationManager.currentRegion, timeManager.GetGameDateTime(), npcController.name, 
            npcController.description, systemPrompt, importantRecords[0], 
            importantRecords[1], importantRecords[2]));
        npcTextBox.text = "";
        _ = llm.Chat(prompt, HandleReply, ReplyCompleted);
    }

    public void SetRecipient(string recipientName)
    {
        var recipientObject = GameObject.Find(recipientName).GetComponent<ModelInference>();
        recipient = recipientObject;
    }
    
    void ReplyCompleted(){
        Debug.Log("The AI replied");
        Response response = ResponseParser.ParseResponse(reply); ;
        PerformAction(response.ResponseAction);
        if (recipient)
        { 
            SendMessageToRecipient(response.ResponseText);
        }
        else
        {
            prompt = "Hmmm what to do next?";
            StartCoroutine(DelayedInference(5f));
        }
    }

    void HandleReply(string reply){
        npcTextBox.text = reply;
        this.reply = reply;
    }

    private void SendMessageToRecipient(string message)
    {
        recipient.ReceiveMessage(message);
    }

    public void ReceiveMessage(string message)
    {
        // Process the received message
       prompt = systemPrompt;
       StartCoroutine(DelayedInference(1f));
    }
    
    private IEnumerator DelayedInference(float delay)
    {
        yield return new WaitForSeconds(delay);
        Inference();
    }

}
