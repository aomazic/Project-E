using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirstControll : MonoBehaviour
{
    [SerializeField] private float thirst = 100f;
    [SerializeField] private float thirstDecreaseRate = 1f;

    // Expose thirst level as a public property
    public float ThirstLevel => thirst;

    private void Update()
    {
        // Decrease the thirst level over time
        thirst += thirstDecreaseRate * Time.deltaTime;
    }

    public void DrinkWater(float amount)
    {
        thirst -= amount;
    }
}
