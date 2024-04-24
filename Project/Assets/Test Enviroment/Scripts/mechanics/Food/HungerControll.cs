using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HungerControll : MonoBehaviour
{
    public float hunger = 100f;
    public float maxHunger = 100f;
    public float hungerRate = 1f;

    void Start()
    {
        hunger = maxHunger;
    }

    void Update()
    {
        IncreaseHunger();
    }

    void IncreaseHunger()
    {
        hunger += hungerRate * Time.deltaTime;

        if (hunger > maxHunger)
        {
            hunger = maxHunger;
        }
    }

    public void Eat(FoodItem food)
    {
        hunger -= food.nutrition;
        if (hunger < 0)
        {
            hunger = 0;
        }
        Destroy(food.gameObject);
    }

}
