using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueController : MonoBehaviour
{
    [SerializeField] GameObject dialogeBox;
    [SerializeField] Canvas canvas;
    [SerializeField] Camera mainCamera;
    NpcController npcController;

    private void Start()
    {
        npcController = GetComponent<NpcController>();
    }

    public void Speak(string dialogue, string listenerName)
    {
        var screenPosition = mainCamera.WorldToScreenPoint(transform.position + Vector3.up * 2);

        var dialoguePanel = Instantiate(dialogeBox, screenPosition, Quaternion.identity, canvas.transform);

        var dialogueText = dialoguePanel.GetComponentInChildren<TextMeshProUGUI>();
        dialogueText.text = dialogue;

        dialoguePanel.transform.rotation = Quaternion.identity;

        dialoguePanel.transform.eulerAngles += new Vector3(0, 0, -20);

        Destroy(dialoguePanel, 5f);

        // Save the observation to the database
        if (npcController.memoryDb)
        {
            npcController.memoryDb.genericObsevation(name, $"told {listenerName}: {dialogue}", 5f);
        }

        // Find the listener game object
        GameObject listener = GameObject.Find(listenerName);
        if (listener)
        {
            DialogueController listenerDialogueController = listener.GetComponent<DialogueController>();
            if (listenerDialogueController)
            {
                listenerDialogueController.Listen(dialogue, name);
            }
        }
    }

    public void Listen(string dialogue, string speaker)
    {
        var screenPosition = mainCamera.WorldToScreenPoint(transform.position + Vector3.up * 2);

        var dialoguePanel = Instantiate(dialogeBox, screenPosition, Quaternion.identity, canvas.transform);

        var dialogueText = dialoguePanel.GetComponentInChildren<TextMeshProUGUI>();
        dialogueText.text = dialogue;

        dialoguePanel.transform.rotation = Quaternion.identity;

        dialoguePanel.transform.eulerAngles += new Vector3(0, 0, 20);

        Destroy(dialoguePanel, 5f);

        if (npcController.memoryDb)
        {
            npcController.memoryDb.genericObsevation(name, $"Heard: {dialogue} from {speaker}", 5f);
        }
    }
}
