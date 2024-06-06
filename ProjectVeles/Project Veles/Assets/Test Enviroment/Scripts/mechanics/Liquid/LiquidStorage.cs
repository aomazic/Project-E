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

    public void TransferLiquid(LiquidStorage target, float volume)
    {
        target.AddLiquid(liquid, volume);
        RemoveLiquid(volume);
    }

    private void AddLiquid(Liquid newLiquid, float volume)
    {
        if (!liquid || liquid.Type == newLiquid.Type)
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

    public float RemoveLiquid(float volume)
    {
        if (liquidLevel >= volume)
        {
            weight -= volume;
            liquidLevel -= volume;
            if (liquidLevel == 0)
            {
                liquid = null;
            }
            return volume;
        }
        Debug.LogError("Not enough liquid in the storage");
        return 0;
    }

}
