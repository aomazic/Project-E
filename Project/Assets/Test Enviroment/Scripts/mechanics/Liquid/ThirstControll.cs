using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirstControll : MonoBehaviour
{
    public float thirst = 100f;
    public float maxThirst = 100f;
    public float thirstDecreaseRate = 1f;
    private NpcController npcController;
    private int lastRecordedThirstLevel = -1;

    private void Start()
    {
        npcController = GetComponent<NpcController>();
        thirst = maxThirst;
    }
    private void Update()
    {
        IncreaseThirst();
        CheckThirstThreshold();
    }

    private void CheckThirstThreshold()
    {
        var currentThirst = (int)thirst;
        if (currentThirst % 25 == 0 && !thirst.Equals(lastRecordedThirstLevel))
        {
            lastRecordedThirstLevel = currentThirst;
            npcController.memoryDb.SaveBasicNeedsObservation(transform.name,  "Thirst",thirst);
        }
    }
    private void IncreaseThirst()
    {
        thirst += thirstDecreaseRate * Time.deltaTime;
        if (thirst > maxThirst)
        {
            thirst = maxThirst;
        }
    }
    public void DrinkWater(float amount)
    {
        thirst -= amount;
        string observationText = "Drinking " + amount + " of water";
        npcController.memoryDb.genericObsevation(transform.name, observationText, 3);
    }
}
