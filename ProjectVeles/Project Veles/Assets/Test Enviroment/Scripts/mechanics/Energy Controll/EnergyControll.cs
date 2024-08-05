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
    public float energyLevel;
    public float restEnergyRate = 1f;
    public float walkEnergyRate = 2f;
    public float runEnergyRate = 5f;
    public State currentState = State.Resting;
    private Transform restPoint;
    private NpcController npcController;
    private int lastRecordedEnergy = -1;

    private void Start()
    {
        npcController = GetComponent<NpcController>();
        energyLevel = maxEnergy;
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
        CheckEnergyThreshold();
    }

    void CheckEnergyThreshold()
    {
        var currentEnergy = (int)energyLevel;
        if (currentEnergy % 25 == 0 && currentEnergy != lastRecordedEnergy)
        {
            lastRecordedEnergy = currentEnergy;
            npcController.memoryDb.SaveBasicNeedsObservation(transform.name,"Energy" ,currentEnergy);
        }
    }

    public void EnterRest(string itemName)
    {
        var item = npcController.itemEnvironmentControll.GetItemByNameInRange(itemName, npcController.itemInteractCollider) as RestingItem;
        if (!item)
        {
            npcController.memoryDb.genericObsevation(transform.name, itemName + " not found", 2f);
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
            transform.rotation = closestSlot.position.rotation;
            ChangeState(State.Resting);
            npcController.memoryDb.genericObsevation(transform.name, "Entered rest at " + Time.time + "on " + restPoint.name, 2f);
        }
        else
        {
            npcController.memoryDb.genericObsevation(transform.name, itemName + "is full", 2f);
        }
    }

    public void LeaveRest()
    {
        var moveDistance = 1f;

        transform.position += transform.forward * moveDistance + transform.up * moveDistance/2;

        ChangeState(State.Walking);
        npcController.memoryDb.genericObsevation(transform.name, "Left rest at " + Time.time, 2f);
    }

    private void DecreaseEnergy(float amount)
    {
        energyLevel -= amount;

        if (energyLevel < 0)
        {
            energyLevel = 0;
        }
    }

    private void Recharge(float amount)
    {
        energyLevel += amount;
        if (energyLevel > maxEnergy)
        {
            energyLevel = maxEnergy;
        }
    }

    private void ChangeState(State newState)
    {
        currentState = newState;
    }

}
