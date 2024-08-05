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
    private bool canTalk = true;
    private float delay = 5f;
    List<string> importantRecords = new List<string>();
    
    private void Start()
    {
        npcController = GetComponent<NpcController>();
        inventory = GetComponent<Inventory>();
        energyControll = GetComponent<EnergyControll>();
        pathfinding = GetComponent<Pathfinding>();
        locationManager = GetComponent<LocationManager>();
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
            case "enterrest":
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
    
    private void Inference()
    {
        importantRecords = npcController.memoryDb.FetchImportantRecords(2, npcController.name);
        prompt = ResponseParser.ConstructPrompt(new Prompt(
            locationManager.currentRegion, timeManager.GetGameDateTime(), 
            npcController.description, systemPrompt, importantRecords[0], 
            importantRecords[1]));
        npcTextBox.text = "";
        _ = llm.Chat(prompt, HandleReply, ReplyCompleted);
    }

    private void SetRecipient(string recipientName)
    {
        var recipientObject = GameObject.Find(recipientName).GetComponent<ModelInference>();
        if (!recipientObject)
        {
            npcController.memoryDb.genericObsevation(npcController.name, $"Tried to talk to {recipientName} but they do not exist", 10f);
            return;
        }

        var recipientCollider = recipientObject.GetComponent<NpcController>().itemInteractCollider;
        if (!npcController.itemInteractCollider.bounds.Intersects(recipientCollider.bounds))
        {
            npcController.memoryDb.genericObsevation(npcController.name, $"Tried to talk to {recipientName} but they are too far away", 10f);
            return;
        }
        recipient = recipientObject;
        recipient.StopCurrentAction();
    }
    void ReplyCompleted(){
        if (!canTalk)
        {
            return;
        }
        systemPrompt = "Hmmm what to do next?";
        Debug.Log("The AI replied");
        Response response = ResponseParser.ParseResponse(reply); ;
        PerformAction(response.ResponseAction);
        if (recipient)
        { 
            delay = 10f;
            recipient.ReceiveMessage(response.ResponseText, npcController.name);
        }
        else
        {
            delay = 5f;
            StartCoroutine(DelayedInference());
        }
    }

    void HandleReply(string reply){
        npcTextBox.text = reply;
        this.reply = reply;
    }
    
    
    private void ReceiveMessage(string message, string sender)
    { 
        systemPrompt = $"{sender} said {message}, what is your response to {sender}?";
        delay = 5f;
        StartCoroutine(DelayedInference());
    }
    
    private void StopCurrentAction()
    {
        canTalk = false;
        StopAllCoroutines();
    }
    private IEnumerator DelayedInference()
    {
        canTalk = true;
        yield return new WaitForSeconds(delay);       
        Inference();
    }

}
