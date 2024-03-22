using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine.Tilemaps;
public class NpcController : Agent
{
    // General components used by NPC
    private Animator npcAnimator;
    private Rigidbody2D rb;

    // Npc rays reference
    [Header("Rays")]
    [SerializeField] private float rayCircleRadius = 0.5f;
    [SerializeField] Transform raysRotation;

    // NPC and Goal Settings
    [Header("Positioning")]
    [SerializeField] private Transform goal;
    [SerializeField] bool randomPos = false;
    private Vector3 startingTargetPos;
    private Vector3 startingNpcPos;

    // Movement Settings
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1f;
    Vector2 dirToGo;

    // Visual Feedback for NPC Actions
    [Header("Visual Feedback")]
    [SerializeField] private Tilemap wallTilemap;
    [SerializeField] private Color successColor = Color.green;
    [SerializeField] private Color failureColor = Color.red;

    // Arena and Spawning Settings for NPC and Goals
    [Header("Arena Layers")]
    [SerializeField] ArenaController[] arenaLayers;
    private int numberOfLayers;

    [Header("Rendering")]
    SpriteRenderer npcSpriteRenderer;
    SpriteRenderer goalSpriteRenderer;

    [Header("Navigation helper")]
    [SerializeField] GameObject[] waypoints;
    [SerializeField] GameObject[] passways;

    public override void Initialize()
    {
        startingNpcPos = transform.localPosition;
        startingTargetPos = goal.localPosition;
        npcSpriteRenderer = GetComponent<SpriteRenderer>();
        goalSpriteRenderer = goal.GetComponent<SpriteRenderer>();
        npcAnimator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        numberOfLayers = arenaLayers.Length;

    }
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(dirToGo);
        sensor.AddObservation(transform.position);
        sensor.AddObservation(goal.position);
    }


    public override void OnEpisodeBegin()
    {
        // Clear and respawn obstacles for all layers
        foreach (var arena in arenaLayers)
        {
            arena.SpawnObstacles();
        }
        foreach (var waypoint in waypoints)
        {
            waypoint.SetActive(true);
        }
        foreach (var passway in passways)
        {
            passway.SetActive(true);
        }
        if (!randomPos)
        {
            transform.localPosition = startingNpcPos;
            goal.localPosition = startingTargetPos;
        }
        else
        {
            // Select random layers for NPC and goal
            int npcLayerIndex = Random.Range(0, numberOfLayers);
            int goalLayerIndex = Random.Range(0, numberOfLayers);

            transform.SetParent(arenaLayers[npcLayerIndex].transform, false); // Set parent for NPC
            goal.SetParent(arenaLayers[goalLayerIndex].transform, false); // Set parent for goal

            npcSpriteRenderer.sortingLayerName = arenaLayers[npcLayerIndex].sortingLayerName;
            goalSpriteRenderer.sortingLayerName = arenaLayers[goalLayerIndex].sortingLayerName;

            // Set actual layers for NPC and Goal
            gameObject.layer = arenaLayers[npcLayerIndex].gameObject.layer;
            goal.gameObject.layer = arenaLayers[goalLayerIndex].gameObject.layer;

            // Find positions in the selected layers
            Vector3 npcPosition = arenaLayers[npcLayerIndex].FindFreePosition();
            Vector3 goalPosition = arenaLayers[goalLayerIndex].FindFreePosition();

            transform.localPosition = npcPosition;
            goal.localPosition = goalPosition;
        }
    
        // Reset ray rotation
        raysRotation.eulerAngles = new Vector3(0, 0, 180);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        MoveAgent(actionBuffers.DiscreteActions);
    }

    void MoveAgent(ActionSegment<int> actions)
    {
        var moveAction = actions[0];

        // Define movement based on actions
        switch (moveAction)
        {
            case 1: // Move Up
                dirToGo = Vector2.up;
                break;
            case 2: // Move Down
                dirToGo = Vector2.down;
                break;
            case 3: // Move Left
                dirToGo = Vector2.left;
                break;
            case 4: // Move Right
                dirToGo = Vector2.right;
                break;
            case 5: // Move Diagonally Up-Right
                dirToGo = new Vector2(1, 1).normalized;
                break;
            case 6: // Move Diagonally Up-Left
                dirToGo = new Vector2(-1, 1).normalized;
                break;
            case 7: // Move Diagonally Down-Right
                dirToGo = new Vector2(1, -1).normalized;
                break;
            case 8: // Move Diagonally Down-Left
                dirToGo = new Vector2(-1, -1).normalized;
                break;
            case 9:
                dirToGo = Vector2.zero;
                break;
        }
        // Set animation based on the direction
        SetAnimationBasedOnDirection(dirToGo);

        rb.velocity = dirToGo * moveSpeed;

        AdjustRayPositionAndRotation(dirToGo);
    }
    void AdjustRayPositionAndRotation(Vector2 direction)
    {
        if (direction != Vector2.zero)
        {
            // Calculate angle for ray direction
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            raysRotation.eulerAngles = new Vector3(0, 0, angle - 90);

            // Calculate new ray position based on angle and radius
            float dynamicOffset = 0.2f; // Adjust this value as needed for your specific NPC size and ray setup
            Vector2 offset = direction * (rayCircleRadius + dynamicOffset);
            Vector3 newRayPos = transform.position + new Vector3(offset.x, offset.y, 0);
            newRayPos.y += 0.2f;
            raysRotation.position = newRayPos;
        }
    }

    void SetAnimationBasedOnDirection(Vector2 direction)
    {
        if (direction != Vector2.zero)
        {
            npcAnimator.SetBool("IsMoving", true);

            // Determine the primary direction of movement
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                // Horizontal movement
                npcAnimator.SetInteger("Direction", direction.x > 0 ? 2 : 3); // Right : Left
            }
            else
            {
                // Vertical movement
                npcAnimator.SetInteger("Direction", direction.y > 0 ? 1 : 0); // Up : Down
            }
        }
        else
        {
            // Agent is idle
            npcAnimator.SetBool("IsMoving", false); // Idle
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Target"))
        {
            ReachGoal();
        }
        else if (collision.CompareTag("Road"))
        {
            AddReward(0.1f);
            collision.gameObject.SetActive(false);
        }
        else if (collision.CompareTag("Pass"))
        {
            AddReward(0.3f);
            collision.gameObject.SetActive(false);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Pass") && !collision.gameObject.CompareTag("Road") && !collision.gameObject.CompareTag("MovableProp"))
        {
            ChangeTilemapColor(failureColor);
            AddReward(-1f);
            EndEpisode();
        }
    }

    private void ReachGoal()
    {
        AddReward(1f);
        ChangeTilemapColor(successColor);
        EndEpisode();    
    }

    private void ChangeTilemapColor(Color newColor)
    {
        wallTilemap.color = newColor;
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        // Reset to no movement
        discreteActionsOut[0] = 9;

        bool moveUp = Input.GetKey(KeyCode.W);
        bool moveDown = Input.GetKey(KeyCode.S);
        bool moveLeft = Input.GetKey(KeyCode.A);
        bool moveRight = Input.GetKey(KeyCode.D);

        // Check for diagonal movement first
        if (moveUp && moveRight)
        {
            discreteActionsOut[0] = 5; // Move Diagonally Up-Right
        }
        else if (moveUp && moveLeft)
        {
            discreteActionsOut[0] = 6; // Move Diagonally Up-Left
        }
        else if (moveDown && moveRight)
        {
            discreteActionsOut[0] = 7; // Move Diagonally Down-Right
        }
        else if (moveDown && moveLeft)
        {
            discreteActionsOut[0] = 8; // Move Diagonally Down-Left
        }
        // Check for single direction movement
        else if (moveUp)
        {
            discreteActionsOut[0] = 1; // Move Up
        }
        else if (moveDown)
        {
            discreteActionsOut[0] = 2; // Move Down
        }
        else if (moveLeft)
        {
            discreteActionsOut[0] = 3; // Move Left
        }
        else if (moveRight)
        {
            discreteActionsOut[0] = 4; // Move Right
        }
    }

}
