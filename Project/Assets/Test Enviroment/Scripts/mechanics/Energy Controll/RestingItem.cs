using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestingItem : Item
{
    public float energyRechargeRate;
    public List<RestingSlot> restingSlots;

    public RestingItem(float weight) : base(weight)
    {
        restingSlots = new List<RestingSlot>();
    }

}


