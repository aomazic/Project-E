using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField]
    protected float weight;
    
    public float Weight
    {
        get => weight;
        set => weight = value;
    }

    public Item(float weight)
    {
        this.weight = weight;
    }
}
