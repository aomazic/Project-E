using UnityEngine;

public class TextBox : MonoBehaviour
{
    [SerializeField] RectTransform panel; // Reference to the RectTransform of the panel
    Transform npc;
    [SerializeField] Camera playerCamera;
    [SerializeField] Vector3 offset = new Vector3(0, 2, 0); // Offset to position the panel above the NPC

    void Start()
    {
        npc = transform;
    }

    void Update()
    {
        if (panel && npc && playerCamera)
        {
            // Calculate the position of the panel in world space
            Vector3 worldPosition = npc.position + offset;

            // Set the position of the panel
            panel.position = playerCamera.WorldToScreenPoint(worldPosition);
        }
    }
}