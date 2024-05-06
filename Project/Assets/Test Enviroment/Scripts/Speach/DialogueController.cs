using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueController : MonoBehaviour
{
    [SerializeField] GameObject dialogeBox;
    [SerializeField] Canvas canvas;
    [SerializeField] Camera mainCamera;

    public void Speak(Dialogue dialogue)
    {
        var screenPosition = mainCamera.WorldToScreenPoint(transform.position + Vector3.up * 2);

        var dialoguePanel = Instantiate(dialogeBox, screenPosition, Quaternion.identity, canvas.transform);

        var dialogueText = dialoguePanel.GetComponentInChildren<TextMeshProUGUI>();
        dialogueText.text = dialogue.Text;

        dialoguePanel.transform.rotation = Quaternion.identity;

        dialoguePanel.transform.eulerAngles += new Vector3(0, 0, -20);

        Destroy(dialoguePanel, 5f);
    }
}
