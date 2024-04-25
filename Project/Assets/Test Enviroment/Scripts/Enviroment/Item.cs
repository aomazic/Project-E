using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField]
    protected float weight;

    [SerializeField]
    protected string itemName;

    public string ItemName
    {
        get => itemName;
        set => itemName = value;
    }

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
