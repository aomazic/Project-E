using System;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public float hoursPerDay = 24f; // In-game hours per day
    public float realSecondsPerGameDay = 7200f; // 2 real-world hours per in-game day
    public int daysPerMonth = 30; // In-game days per month
    public int monthsPerYear = 12; // In-game months per year

    private float currentTimeOfDay; // Current in-game time (0.0 - 1.0 represents 0:00 - 23:59)
    private int currentDay = 1; // Current in-game day
    private int currentMonth = 1; // Current in-game month
    private int currentYear = 1000; // Current in-game year

    void Update()
    {
        // Calculate the in-game time
        currentTimeOfDay += Time.deltaTime / realSecondsPerGameDay;
        if (currentTimeOfDay > 1) // A new day
        {
            currentTimeOfDay -= 1;
            currentDay++;
            if (currentDay > daysPerMonth) // A new month
            {
                currentDay = 1;
                currentMonth++;
                if (currentMonth > monthsPerYear) // A new year
                {
                    currentMonth = 1;
                    currentYear++;
                }
            }
        }
    }

    public string GetGameDateTime()
    {
        int hours = (int)(currentTimeOfDay * hoursPerDay);
        int minutes = (int)((currentTimeOfDay * hoursPerDay - hours) * 60);
        DateTime gameDateTime = new DateTime(currentYear, currentMonth, currentDay, hours, minutes, 0);
        
        return gameDateTime.ToString("dd/MM/yyyy HH:mm");
    }
}