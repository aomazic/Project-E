using UnityEngine;
using Random = UnityEngine.Random;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class Npc : Agent
{
    public GameObject area;
    private TestArea testArena;
    private Rigidbody npcRigidBody;
    [SerializeField] private float moveSpeed = 1000f;

    public override void Initialize()
    {
        npcRigidBody = GetComponent<Rigidbody>();
        testArena = area.GetComponent<TestArea>();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(testArena.goal.transform.position);
        sensor.AddObservation(transform.InverseTransformDirection(npcRigidBody.velocity));
    }

    private void MoveAgent(ActionSegment<int> act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        var action = act[0];
        switch (action)
        {
            case 1:
                dirToGo = transform.forward * 1f;
                break;
            case 2:
                dirToGo = transform.forward * -1f;
                break;
            case 3:
                rotateDir = transform.up * 1f;
                break;
            case 4:
                rotateDir = transform.up * -1f;
                break;
            case 5:
                dirToGo = Vector3.zero;
                break;
        }
        transform.Rotate(rotateDir, Time.deltaTime * 150f);
        npcRigidBody.AddForce(dirToGo * moveSpeed, ForceMode.Force);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        if (Physics.Raycast(transform.position, -Vector3.up, out RaycastHit hit, 1f))
        {
            if (hit.collider.gameObject.CompareTag("road"))
            {
                SetReward(0.1f); // small reward for being on the road
            }
        }
        AddReward(-1f / MaxStep);
        MoveAgent(actionBuffers.DiscreteActions);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = 5;

        if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[0] = 3;
        }
        else if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = 1;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[0] = 4;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            discreteActionsOut[0] = 2;
        }
    }

    public override void OnEpisodeBegin()
    {
        testArena.ResetArea();

        npcRigidBody.velocity = Vector3.zero;
        transform.rotation = Quaternion.Euler(new Vector3(0f, Random.Range(0, 360)));
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("goal"))
        {
            return;
        }
        SetReward(2f);
        EndEpisode();
    }
}
