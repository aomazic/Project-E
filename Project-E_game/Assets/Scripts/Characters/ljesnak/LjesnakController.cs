using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

public class ljesnakController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.0f;
    [SerializeField] private LayerMask characterLayer;

    [Header("Attack Parameters")]
    [SerializeField] private float swipeCooldown = 2.0f;
    [SerializeField] private float stabCooldown = 1.5f;
    private GameObject attackObject;

    [Header("References")]
    [SerializeField] GameObject swipeAttack;
    [SerializeField] GameObject stabAttack;
    private CharacterStats ljesnakStats;
    private Dictionary<Collider2D, CharacterStats> characterStatsCache = new Dictionary<Collider2D, CharacterStats>();
    private float lastAttackTime = 0;

    private enum AttackType {swipe, stab}
    private Vector3 directionToTarget;

    private float wanderTimer = 0f;
    private float wanderInterval = 5.0f;

    private bool isStuned = false;



    void Start()
    {
        ljesnakStats = GetComponent<CharacterStats>();
        if (ljesnakStats != null)
        {
            ljesnakStats.OnStunned.AddListener(HandleStun);
            ljesnakStats.OnStunRecovered.AddListener(HandleStunRecovered);
        }
        StartCoroutine(DetectionCoroutine());
        swipeAttack.SetActive(false);
        stabAttack.SetActive(false);
    }

    IEnumerator DetectionCoroutine()
    {
        while (true)
        {
            if (wanderTimer > 0)
            {
                wanderTimer -= 0.5f;
            }
            DetectAndMove();
            yield return new WaitForSeconds(0.5f); 
        }
    }
    void DecideAttack()
    {
        int attackChoice = Random.Range(0, 2); // 0 for swipe, 1 for stab

        if (attackChoice == 0)
        {
            SwipeAttack();
        }
        else
        {
            StabAttack();
        }
    }
    void DetectAndMove()
    {
        if (isStuned) return;
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, ljesnakStats.maxDetectionRadius, characterLayer);
        Transform target = FindHighestShardPower(hitColliders, ljesnakStats.detectionRadius);
        if (target != null && target != transform)
        {
            ljesnakStats.animator.SetBool("IsMoving", true);
            directionToTarget = (target.position - transform.position).normalized;
            MoveTowards(directionToTarget, moveSpeed);
            DecideAttack();
        }
        else
        {
            if (wanderTimer <= 0)
            {
                if (ShouldWander())
                {
                    Wander();
                    wanderTimer = wanderInterval; // Reset the timer
                }
                else
                {
                    ljesnakStats.animator.SetBool("IsMoving", false);
                }
                directionToTarget = Vector3.zero;
            }
        }
    }

    bool ShouldWander()
    {
        return Random.value > 0.8f;
    }

    void Wander()
    {
        ljesnakStats.animator.SetBool("IsMoving", true);
        Vector2 wanderDirection = Random.insideUnitCircle.normalized;
        MoveTowards(wanderDirection, moveSpeed/2f);
    }

    Transform FindHighestShardPower(Collider2D[] hitColliders, float ownRadius)
    {
        Transform highestShardPowerTarget = null;
        float highestShardPower = 0.0f;

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
                float targetRadius = characterStats.detectionRadius;
                if (ShardRadiiReachEachOther(transform.position, hitCollider.transform.position, ownRadius, targetRadius)
                    && characterStats.shardPower > highestShardPower)
                {
                    highestShardPower = characterStats.shardPower;
                    highestShardPowerTarget = hitCollider.transform;
                }
            }
        }

        return highestShardPowerTarget;
    }

    bool ShardRadiiReachEachOther(Vector2 positionA, Vector2 positionB, float radiusA, float radiusB)
    {
        float distance = Vector2.Distance(positionA, positionB);
        return distance <= radiusA || distance <= radiusB;
    }


    void MoveTowards(Vector3 destination, float speed)
    {
        if (Mathf.Abs(destination.x) > Mathf.Abs(destination.y))
        {
            // Horizontal movement
            ljesnakStats.animator.SetInteger("Direction", destination.x > 0 ? 2 : 3); // Right : Left
        }
        else if (Mathf.Abs(destination.y) > 0)
        {
            // Vertical movement
            ljesnakStats.animator.SetInteger("Direction", destination.y > 0 ? 1 : 0); // Up : Down
        }

        ljesnakStats.rb.velocity = speed * destination;
    }

    void SwipeAttack()
    {
        if (Time.time - lastAttackTime >= swipeCooldown)
        {
            attackObject = swipeAttack;
            InstantiateAttackObject(directionToTarget);
            lastAttackTime = Time.time;
        }
    }

    void StabAttack()
    {
        if (Time.time - lastAttackTime >= stabCooldown)
        {
            attackObject = stabAttack;
            InstantiateAttackObject(directionToTarget);
            lastAttackTime = Time.time;
        }
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