using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestingSlot : MonoBehaviour
{
    public bool isOccupied;
    public Transform position;

    void Start()
    {
        position = GetComponent<Transform>();
    }
}
