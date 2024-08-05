using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CameraControll : MonoBehaviour
{
    public List<Transform> targets;
    public Vector3 offset = new Vector3(0, 10, -10); 
    public float smoothSpeed = 0.125f;
    private int currentTargetIndex = 0;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) 
        {
            currentTargetIndex += 1;
            if (currentTargetIndex >= targets.Count)
            {
                currentTargetIndex = 0;
            }
        }
    }

    void LateUpdate()
    {
        if (targets.Count > 0)
        {
            Transform target = targets[currentTargetIndex];
            Vector3 desiredPosition = target.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;

            // Make the camera look at the player
            transform.LookAt(target);
        }
    }
}