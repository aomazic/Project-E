using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vision : MonoBehaviour
{
    public NpcController npcController;
    public float fieldOfViewAngle = 110f;
    public float visionDistance = 10f;
    public int numberOfRays = 10;
    public float saveCooldown = 1f; // Cooldown period in seconds
    private Dictionary<string, float> lastSeenTimes;

    private void Start()
    {
        npcController = GetComponent<NpcController>();
        lastSeenTimes = new Dictionary<string, float>();
    }

    void Update()
    {
        var stepAngleSize = fieldOfViewAngle / numberOfRays;
        for (int i = 0; i <= numberOfRays; i++)
        {
            var angle = transform.eulerAngles.y - fieldOfViewAngle / 2 + stepAngleSize * i;
            var direction = new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle * Mathf.Deg2Rad));
            direction = transform.TransformDirection(direction) * visionDistance;
            if (Physics.Raycast(transform.position, direction, out RaycastHit hit, visionDistance))
            {
                var objectName = hit.transform.name;
                if (!lastSeenTimes.ContainsKey(objectName) || Time.time - lastSeenTimes[objectName] >= saveCooldown)
                {
                    Region region = null;
                    if (hit.transform.parent)
                    {
                        region = hit.transform.parent.GetComponent<Region>();
                    }
                    npcController.memoryDb.SaveVisionObservation(transform.name, objectName, region);
                    lastSeenTimes[objectName] = Time.time;
                }
            }
            Debug.DrawRay(transform.position, direction, Color.green);
        }
    }
}
