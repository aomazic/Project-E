using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingEntranceController : MonoBehaviour
{
    public Transform targetLocation;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("npc"))
        {
            other.gameObject.transform.position = targetLocation.position;
        }
    }
}
