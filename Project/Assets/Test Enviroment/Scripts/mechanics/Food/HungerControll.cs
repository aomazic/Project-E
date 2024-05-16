using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HungerControll : MonoBehaviour
{
    public float hunger = 100f;
    public float maxHunger = 100f;
    public float hungerRate = 1f;
    private NpcController npcController;
    private int lastRecordedHunger = -1;
    void Start()
    {
        npcController = GetComponent<NpcController>();
        hunger = maxHunger;
    }

    void Update()
    {
        IncreaseHunger();
        CheckHungerThreshold();
    }
    private void IncreaseHunger()
    {
        hunger += hungerRate * Time.deltaTime;

        if (hunger > maxHunger)
        {
            hunger = maxHunger;
        }
    }
    private void CheckHungerThreshold()
    {
        var currentHunger = (int)hunger;
        if (currentHunger % 25 == 0 && !hunger.Equals(lastRecordedHunger))
        {
            lastRecordedHunger = currentHunger;
            npcController.memoryDb.SaveBasicNeedsObservation(transform.name,"Hunger", hunger);
        }
    }

    public void Eat(FoodItem food)
    {
        hunger -= food.nutrition;
        if (hunger < 0)
        {
            hunger = 0;
        }
        npcController.memoryDb.SaveEatingObservation(transform.name, food.name, food.nutrition);
        Destroy(food.gameObject);
    }

}
