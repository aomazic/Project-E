using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnergyControll : MonoBehaviour
{
    public enum State
    {
        Resting,
        Walking,
        Running
    }

    public float maxEnergy = 100f;
    public float currentEnergy;
    public float restEnergyRate = 1f;
    public float walkEnergyRate = 2f;
    public float runEnergyRate = 5f;
    public State currentState = State.Resting;
    private Transform restPoint;
    private NpcController npcController;

    private void Start()
    {
        npcController = GetComponent<NpcController>();
        currentEnergy = maxEnergy;
    }

    private void Update()
    {
        switch (currentState)
        {
            case State.Resting:
                transform.position = restPoint.position;
                Recharge(restEnergyRate * Time.deltaTime);
                break;
            case State.Running:
                DecreaseEnergy(runEnergyRate * Time.deltaTime);
                break;
            case State.Walking:
                DecreaseEnergy(walkEnergyRate * Time.deltaTime);
                break;
            default:
                DecreaseEnergy(walkEnergyRate * Time.deltaTime);
                break;
        }
    }

    public void EnterRest(string itemName)
    {
        var item = npcController.itemEnvironmentControll.GetItemByNameInRange(itemName, transform.position, npcController.itemInteractionRange) as RestingItem;
        if (!item)
        {
            return;
        }

        var closestSlot = item.restingSlots
            .Where(slot => !slot.isOccupied)
            .OrderBy(slot => Vector3.Distance(transform.position, slot.position.position))
            .FirstOrDefault();

        if (closestSlot)
        {
            closestSlot.isOccupied = true;
            restPoint = closestSlot.position;
            transform.rotation = closestSlot.position.rotation; // Rotate the NPC in the same direction as the resting slot
            ChangeState(State.Resting);
        }
    }

    public void LeaveRest()
    {
        var moveDistance = 1f;

        transform.position += transform.forward * moveDistance + transform.up * moveDistance/2;

        ChangeState(State.Walking);
    }

    private void DecreaseEnergy(float amount)
    {
        currentEnergy -= amount;

        if (currentEnergy < 0)
        {
            currentEnergy = 0;
        }
    }

    private void Recharge(float amount)
    {
        currentEnergy += amount;
        if (currentEnergy > maxEnergy)
        {
            currentEnergy = maxEnergy;
        }
    }

    private void ChangeState(State newState)
    {
        currentState = newState;
    }

}
