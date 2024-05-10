using System;
using UnityEngine.AI;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;
    [SerializeField] PoiController poiController;

    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        poiController = FindObjectOfType<PoiController>();
    }

    public void goTo(String destinationName)
    {
        var poi = poiController.GetPoiByName(destinationName);
        if (poi)
        {
            Move(poi);
        }
    }

    private void Move(PontOfInterest poi)
    {
        navMeshAgent.SetDestination(poi.poiLocation.position);
        transform.LookAt(poi.poiLocation.position);
    }

    public void Stop()
    {
        navMeshAgent.isStopped = true;
    }
}
