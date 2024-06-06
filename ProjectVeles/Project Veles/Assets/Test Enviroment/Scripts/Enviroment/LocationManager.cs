using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationManager : MonoBehaviour
{
    public string currentRegion;
    
    public void setCurrentRegion(string region)
    {
        currentRegion = region;
    }
}
