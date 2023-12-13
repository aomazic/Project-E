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
    [SerializeField] private float attackOffset = 1.5f;

    [Header("References")]
    [SerializeField] GameObject attackPoolManager;
    [SerializeField] SpriteRenderer spriteRenderer;
    private AttackPool swipeAttackPool;
    private AttackPool stabAttackPool;
    private CharacterStats ljesnakStats;
    private Rigidbody2D rb;
    private Dictionary<Collider2D, CharacterStats> characterStatsCache = new Dictionary<Collider2D, CharacterStats>();
    private float lastAttackTime = 0;

    private enum AttackType {swipe, stab}
    private Vector3 directionToTarget;
  

    void Start()
    {
        AttackPool[] pools = attackPoolManager.GetComponents<AttackPool>();
        foreach (var pool in pools)
        {
            if (pool.attackPrefabName == "swipe")
            {
                swipeAttackPool = pool;
            }
            else if (pool.attackPrefabName == "stab")
            {
                stabAttackPool = pool;
            }
        }
        ljesnakStats = GetComponent<CharacterStats>();
        rb = GetComponent<Rigidbody2D>();
        StartCoroutine(DetectionCoroutine());
    }

    IEnumerator DetectionCoroutine()
    {
        while (true)
        {
            DetectAndMove();
            yield return new WaitForSeconds(0.5f); // Adjust frequency as needed
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
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, ljesnakStats.maxDetectionRadius, characterLayer);
        Transform target = FindHighestShardPower(hitColliders, ljesnakStats.detectionRadius);

        if (target != null && target != transform)
        {
            directionToTarget = (target.position - transform.position).normalized;
            if (directionToTarget.x > 0)
            {
                spriteRenderer.flipX = false;
            }
            else
            {
                spriteRenderer.flipX = true;
            }
            MoveTowards();
            DecideAttack();
        }
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


    void MoveTowards()
    {
        
        rb.MovePosition(rb.position + (Vector2)directionToTarget * moveSpeed * Time.deltaTime);
    }

    void SwipeAttack()
    {
        if (Time.time - lastAttackTime >= swipeCooldown)
        {
            InstantiateAttackObject(AttackType.swipe, directionToTarget);
            lastAttackTime = Time.time;
        }
    }

    void StabAttack()
    {
        if (Time.time - lastAttackTime >= stabCooldown)
        {
            InstantiateAttackObject(AttackType.swipe, directionToTarget);
            lastAttackTime = Time.time;
        }
    }

    private void InstantiateAttackObject(AttackType attackType, Vector3 directionToTarget)
    {
        GameObject attackObject;

        if (attackType.Equals(AttackType.swipe))
        {
            attackObject = swipeAttackPool.GetAttackObject();
        }
        else // if stab
        {
            attackObject = stabAttackPool.GetAttackObject();
        }

        Quaternion rotation = Quaternion.LookRotation(Vector3.forward, directionToTarget);
        attackObject.transform.rotation = rotation;

        // Set parent, activate and start coroutine to return to pool
        attackObject.transform.SetParent(transform, false);
        attackObject.SetActive(true);
        StartCoroutine(ReturnAttackObjectToPool(attackObject, attackType));
    }

    private IEnumerator ReturnAttackObjectToPool(GameObject attackObject, AttackType attackType)
    {
        float waitTime = attackObject.GetComponentInChildren<LjesnakWeaponDetection>().duration;
        yield return new WaitForSeconds(waitTime);
        attackObject.transform.SetParent(null);
        if (attackType == AttackType.swipe)
        {
            swipeAttackPool.ReturnToPool(attackObject);
        }
        else
        {
            stabAttackPool.ReturnToPool(attackObject);
        }
    }
}