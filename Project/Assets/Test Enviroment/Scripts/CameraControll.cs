using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControll : MonoBehaviour
{
    public Transform target; // The target object that the camera will follow
    private Vector3 offset; // The initial offset between the camera and the target

    // Start is called before the first frame update
    void Start()
    {
        // Calculate the initial offset between the camera and the target
        offset = transform.position - target.position;
    }

    // Update is called once per frame
    void Update()
    {
        // Update the camera's position to follow the target
        transform.position = target.position + offset;
    }
}
