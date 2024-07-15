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
    [SerializeField] private LLMClient llm;
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
        const string worldInfo = "The following is a information about the Cika village. This is a small settlement with basic amenities. " +
                           "The village consists of five houses, a market for trading goods, a chief's house which serves as the administrative center, " +
                           "a town well that provides water for the villagers, and a tavern where villagers gather to relax and socialize.";
        prompt = worldInfo;
        Inference();
    }
    
    private void PerformAction(ResponseAction action)
    {
        switch (action.Type)
        {
            case "equip":
                inventory.EquipItem(action.ActionDetail.Source.ToLower());
                Debug.Log("Equip");
                break;
            case "unequip":
                inventory.UnequipItem();
                Debug.Log("unequip");
                break;
            case "drink":
                Debug.Log("drink");
                npcController.DrinkItem(action.ActionDetail.Source.ToLower());
                break;
            case "drop":
                Debug.Log("drop");
                inventory.DropItem(action.ActionDetail.Source.ToLower());
                break;
            case "pickup":
                Debug.Log("pickup");
                inventory.PickupItem(action.ActionDetail.Source.ToLower());
                break;
            case "transfer":
                Debug.Log("transfer");
                inventory.TransferLiquid(action.ActionDetail.Target.ToLower(), action.ActionDetail.Source.ToLower(), 1);
                break;
            case "eat":
                Debug.Log("eat");
                inventory.EatFood(action.ActionDetail.Source.ToLower());
                break;
            case "useitem":
                Debug.Log("useitem");
                energyControll.EnterRest(action.ActionDetail.Source.ToLower());
                break;
            case "talkto":
                Debug.Log("talkto");
                SetRecipient(action.ActionDetail.Target.ToLower());
                break;
            case "goto":
                Debug.Log("goto");
                pathfinding.GoTo(action.ActionDetail.Target.ToLower());
                break;
            default:
                recipient = null;
                Debug.LogError("Unknown action type: " + action.Type);
                break;
        }
    }
    public void Inference()
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
        Response response = ResponseParser.ParseResponse(reply);
        npcTextBox.text = response.ResponseText;
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
