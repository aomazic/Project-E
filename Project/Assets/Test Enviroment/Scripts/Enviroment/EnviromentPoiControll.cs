using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoiController : MonoBehaviour
{
    private Dictionary<string, PontOfInterest> pois;

    private void Start()
    {
        pois = new Dictionary<string, PontOfInterest>();
        foreach (var poi in FindObjectsOfType<PontOfInterest>())
        {
            pois[poi.poiName] = poi;
        }
    }

    public PontOfInterest GetPoiByName(string poiName)
    {
        pois.TryGetValue(poiName, out var poi);
        return poi;
    }
}
