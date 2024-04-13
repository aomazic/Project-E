using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiquidStorage : Item
{
    [SerializeField] Liquid liquid;
    [SerializeField] float liquidLevel = 0f;
    [SerializeField] private float capacity;


    public void Start()
    {
        weight += liquidLevel;
    }

    public LiquidStorage(float weight) : base(weight)
    {

    }

    public void AddLiquid(Liquid newLiquid, float volume)
    {
        if (liquid == null || liquid.Type == newLiquid.Type)
        {
            if (liquidLevel + volume <= capacity)
            {
                weight += volume;
                liquidLevel += volume;
                liquid = newLiquid;
            }
            else
            {
                Debug.LogError("The Liquid storage is full");
            }
        }
        else
        {
            Debug.LogError("Only one type of liquid can be stored in a single barrel");
        }
    }

    public void RemoveLiquid(float volume)
    {
        if (liquidLevel >= volume)
        {
            weight -= volume;
            liquidLevel -= volume;
            if (liquidLevel == 0)
            {
                liquid = null; // Reset the liquid if the barrel is empty
            }
        }
        else
        {
            Debug.LogError("Not enough liquid in the storage");
        }
    }

}
