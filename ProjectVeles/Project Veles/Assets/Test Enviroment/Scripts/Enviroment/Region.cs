using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Region : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        other.GetComponent<LocationManager>().setCurrentRegion(name);
    }

    private void OnTriggerExit(Collider other)
    {
        other.GetComponent<LocationManager>().setCurrentRegion("somewhere");
    }
}
