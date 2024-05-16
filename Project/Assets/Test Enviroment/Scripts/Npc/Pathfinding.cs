using System;
using UnityEngine.AI;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;

    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    public void goTo(string destinationName)
    {
        GameObject destination = GameObject.Find(destinationName);
        if (destination)
        {
            Vector3 offset = transform.forward * -2f;
            Move(destination.transform.position + offset);
        }
    }

    private void Move(Vector3 destination)
    {
        navMeshAgent.SetDestination(destination);
        transform.LookAt(destination);
    }

    public void Stop()
    {
        navMeshAgent.isStopped = true;
    }
}
