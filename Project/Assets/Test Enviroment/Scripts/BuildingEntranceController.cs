using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingEntranceController : MonoBehaviour
{
    public Transform targetLocation; // The location inside the building where the player will be teleported to

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("npc"))
        {
            other.gameObject.transform.position = targetLocation.position;
        }
    }
}
