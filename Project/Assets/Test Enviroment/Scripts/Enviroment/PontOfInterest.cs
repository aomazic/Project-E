
using UnityEngine;

public class PontOfInterest : MonoBehaviour
{
    public string poiName;
    public Transform poiLocation;


    public void Start()
    {
        poiLocation = gameObject.transform;
    }
}
