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

    public void GoTo(string destinationName)
    {
        GameObject destination = GameObject.Find(destinationName);
        if (destination)
        {
            Vector3 offset = transform.forward * -2f;
            Vector3 destinationPosition = destination.transform.position + offset;

            // Try to find a point on the NavMesh within a certain radius
            Vector3 randomPoint = destinationPosition + UnityEngine.Random.insideUnitSphere * 10f;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 10f, NavMesh.AllAreas))
            {
                // If a point is found, move to that point
                Move(hit.position);
            }
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
