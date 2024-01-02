using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using static UnityEngine.GraphicsBuffer;

public class ljesnakController : Agent
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.0f;
    [SerializeField] private LayerMask characterLayer;

    [Header("Attack Parameters")]
    [SerializeField] private float swipeCooldown = 2.0f;
    [SerializeField] private float stabCooldown = 1.5f;
    private GameObject attackObject;
    [SerializeField] private float engagementDistance = 1.5f;

    [Header("References")]
    [SerializeField] GameObject swipeAttack;
    [SerializeField] GameObject stabAttack;
    private CharacterStats ljesnakStats;
    private Dictionary<Collider2D, CharacterStats> characterStatsCache = new Dictionary<Collider2D, CharacterStats>();
    private float lastAttackTime = 0;

    private RayPerceptionSensorComponent2D raySensor;

    private enum AttackType {swipe, stab}

    private bool isStuned = false;
    private Transform highestShardPowerTarget;
    private float previousDistanceToTarget;

    private Vector3 startPosition;
    private int initialLayer;
    private Vector3 attackDirection;

    private Vector2 lastDirection = Vector2.zero;
    private float lastMoveTime = 0f;
    private float moveCooldown = 0.5f;

    private float lastDistanceToTarget = float.MaxValue;

    private float movingTime = 0f;
    private float idleTime = 0f;
    private const float desiredMovementRatio = 0.4f;
    public override void Initialize()
    {
        highestShardPowerTarget = null;
        raySensor = GetComponent<RayPerceptionSensorComponent2D>();
        ljesnakStats = GetComponent<CharacterStats>();
        if (ljesnakStats != null)
        {
            ljesnakStats.OnStunned.AddListener(HandleStun);
            ljesnakStats.OnStunRecovered.AddListener(HandleStunRecovered);
        }
        swipeAttack.SetActive(false);
        stabAttack.SetActive(false);
        startPosition = transform.position;
    }

    public override void OnEpisodeBegin()
    {
        
        transform.position = startPosition; 
        transform.rotation = Quaternion.identity;

        // Reset internal state variables
        lastAttackTime = 0;
        isStuned = false;
        highestShardPowerTarget = null;
        previousDistanceToTarget = 0;
        gameObject.layer = initialLayer;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Add observations
        raySensor.RayLength = ljesnakStats.detectionRadius;
        sensor.AddObservation(ljesnakStats.shardPower);
        sensor.AddObservation(ljesnakStats.health);
        sensor.AddObservation(ljesnakStats.posture);
        sensor.AddObservation(startPosition);

        // Detect enemies
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, ljesnakStats.maxDetectionRadius, characterLayer);
        highestShardPowerTarget = null;
        float highestShardPower = 0.0f;
        CharacterStats highestShardPowerStats= null;

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.transform == transform) continue;

            CharacterStats characterStats;
            if (!characterStatsCache.TryGetValue(hitCollider, out characterStats))
            {
                characterStats = hitCollider.GetComponent<CharacterStats>();
                if (characterStats != null)
                {
                    characterStatsCache[hitCollider] = characterStats;
                }
            }

            if (characterStats != null)
            {
                sensor.AddObservation(characterStats.shardPower);
                float targetRadius = characterStats.detectionRadius;
                if (ShardRadiiReachEachOther(transform.position, hitCollider.transform.position, ljesnakStats.detectionRadius, targetRadius)
                    && characterStats.shardPower > highestShardPower)
                {
                    highestShardPowerStats = characterStats;
                    highestShardPower = characterStats.shardPower;
                    highestShardPowerTarget = hitCollider.transform;
                }
            }
            else
            {
                sensor.AddObservation(0f);
            }
        }

        bool hasTarget = highestShardPowerTarget != null && highestShardPowerTarget != transform;

        if (hasTarget)
        {
            Vector3 relativePosition = transform.InverseTransformPoint(highestShardPowerTarget.position);
            sensor.AddObservation(hasTarget);
            sensor.AddObservation(relativePosition.x);
            sensor.AddObservation(relativePosition.y);
            sensor.AddObservation(relativePosition.z);
            sensor.AddObservation(highestShardPowerStats.health);
            sensor.AddObservation(highestShardPowerStats.posture);
            sensor.AddObservation(highestShardPowerStats.shardPower);

            previousDistanceToTarget = Vector3.Distance(transform.position, highestShardPowerTarget.position);
        }
        else
        {
            // Add zeros if no high shard power target is present
            sensor.AddObservation(0);
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
        }
    }

    void MoveAgent(ActionSegment<int> actions)
    {
        var moveAction = actions[0];

        Vector2 dirToGo = Vector2.zero;

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

        if (Time.time - lastMoveTime < moveCooldown && lastDirection != dirToGo)
        {
            AddReward(-0.01f); // Penalize rapid direction changes
        }

        lastDirection = dirToGo; // Update last direction
        lastMoveTime = Time.time; // Update last move time

        // Apply movement
        ljesnakStats.rb.velocity = dirToGo * moveSpeed;

        // Set animation based on the direction
        SetAnimationBasedOnDirection(dirToGo);
    }

    void SetAnimationBasedOnDirection(Vector2 direction)
    {
        if (direction != Vector2.zero)
        {
            // Determine the primary direction of movement
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                // Horizontal movement
                ljesnakStats.animator.SetInteger("Direction", direction.x > 0 ? 2 : 3); // Right : Left
            }
            else
            {
                // Vertical movement
                ljesnakStats.animator.SetInteger("Direction", direction.y > 0 ? 1 : 0); // Up : Down
            }
        }
        else
        {
            // Agent is idle
            ljesnakStats.animator.SetInteger("Direction", -1); // Idle
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        MoveAgent(actionBuffers.DiscreteActions);

        if (highestShardPowerTarget == null)
        {
            float distanceFromStart = Vector3.Distance(transform.position, startPosition);
            if (highestShardPowerTarget == null && distanceFromStart > 5f)
            {
                // Penalize for straying too far from the start or strategic point
                AddReward(-0.01f * (distanceFromStart - 5f));
            }
            else if (distanceFromStart < 5f)
            {
                // Reward for patrolling within a certain radius
                AddReward(0.02f);
            }

            if (actionBuffers.DiscreteActions[0] == 9) // Idle action
            {
                idleTime += Time.fixedDeltaTime;
            }
            else
            {
                movingTime += Time.fixedDeltaTime;
            }

            // Check the ratio and adjust behavior
            float totalActiveTime = movingTime + idleTime;
            if (totalActiveTime > 0) // Avoid division by zero
            {
                float currentMovementRatio = movingTime / totalActiveTime;
                if (currentMovementRatio < desiredMovementRatio)
                {
                    // Encourage movement if below desired ratio
                    AddReward(0.01f);
                }
                else
                {
                    // Encourage idling if above desired ratio
                    AddReward(0.005f);
                }
            }
        }
        if (highestShardPowerTarget != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, highestShardPowerTarget.position);
            if (distanceToTarget < lastDistanceToTarget && distanceToTarget > engagementDistance)
            {
                // Reward for closing in on the target
                AddReward(0.1f * (1 - distanceToTarget / lastDistanceToTarget));
            }
            else if (distanceToTarget < engagementDistance)
            {
                // Small reward for maintaining optimal engagement distance
                AddReward(0.05f);
            }
            else
            {
                // Penalty for not closing in or losing the target
                AddReward(-0.05f);
            }
            lastDistanceToTarget = distanceToTarget;
        }

        attackDirection = Vector2.zero;
        float angle = actionBuffers.ContinuousActions[0];
        attackDirection = AngleToDirection(angle);

        var attackAction = actionBuffers.DiscreteActions[1];
        HandleAttack(attackAction);;

        if (highestShardPowerTarget != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, highestShardPowerTarget.position);
            if (distanceToTarget < previousDistanceToTarget)
            {
                AddReward(0.1f);
            }
            else
            {
                AddReward(-0.1f);
            }
            previousDistanceToTarget = distanceToTarget;
        }
    }
    void HandleAttack(int attackType)
    {
        switch (attackType)
        {
            case 1:
                SwipeAttack();
                break;
            case 2:
                StabAttack();
                break;
            case 3:
                break;
        }
    }

    bool ShardRadiiReachEachOther(Vector2 positionA, Vector2 positionB, float radiusA, float radiusB)
    {
        float distance = Vector2.Distance(positionA, positionB);
        return distance <= radiusA || distance <= radiusB;
    }
    Vector2 AngleToDirection(float angle)
    {
        // Convert the angle to a direction vector
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }

    void SwipeAttack()
    {
        if (Time.time - lastAttackTime >= swipeCooldown && !isStuned)
        {
            attackObject = swipeAttack;
            InstantiateAttackObject(attackDirection);
            lastAttackTime = Time.time;
        }
    }

    void StabAttack()
    {
        if (Time.time - lastAttackTime >= stabCooldown  && !isStuned)
        {
            attackObject = stabAttack;
            InstantiateAttackObject(attackDirection);
            lastAttackTime = Time.time;
        }
    }

    public void OnAttackHitTarget()
    {
        AddReward(1.0f); 
    }
    public void OnAttackMissed()
    {
        AddReward(-0.5f);
    }

    public void OnEnemySlain(float shardPowergain)
    {
        AddReward(5f * shardPowergain);
    }
    private void InstantiateAttackObject(Vector3 directionToTarget)
    {
       
        Quaternion rotation = Quaternion.LookRotation(Vector3.forward, directionToTarget);
        attackObject.transform.rotation = rotation;

        attackObject.SetActive(true);
    }

    private void HandleStun()
    {
        isStuned = true;
    }

    private void HandleStunRecovered()
    {
        isStuned = false;
    }


    void OnDestroy()
    {
        if (ljesnakStats != null)
        {
            ljesnakStats.OnStunned.RemoveListener(HandleStun);
            ljesnakStats.OnStunRecovered.RemoveListener(HandleStunRecovered);
        }
    }
}