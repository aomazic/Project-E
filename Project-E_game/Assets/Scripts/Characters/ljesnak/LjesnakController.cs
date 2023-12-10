using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ljesnakController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.0f;
    [SerializeField] private LayerMask characterLayer;

    [Header("Detection")]
    private Dictionary<Collider2D, CharacterStats> characterStatsCache = new Dictionary<Collider2D, CharacterStats>();
    private float detectionRadius;

    [Header("Attack Parameters")]
    [SerializeField] private float swipeRange = 1.5f;
    [SerializeField] private float swipeDamage = 15.0f;
    [SerializeField] private float swipeCooldown = 2.0f;

    [SerializeField] private float stabRange = 1.0f;
    [SerializeField] private float stabDamage = 20.0f;
    [SerializeField] private float stabCooldown = 1.5f;

    [Header("References")]
    [SerializeField] AttackPool swipeAttackPool;
    [SerializeField] AttackPool stabAttackPool;
    private CharacterStats ljesnakStats;
    private Rigidbody2D rb;

    private float lastAttackTime = 0;
    private Animator animator;

    private enum AttackType {swipe, stab}


    void Start()
    {
        ljesnakStats = GetComponent<CharacterStats>();
        rb = GetComponent<Rigidbody2D>();
        StartCoroutine(DetectionCoroutine());
        detectionRadius = CalculateDetectionRadius(ljesnakStats.shardPower);
    }

    IEnumerator DetectionCoroutine()
    {
        while (true)
        {
            DetectAndMove();
            yield return new WaitForSeconds(0.5f); // Adjust frequency as needed
        }
    }

    void DetectAndMove()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius, characterLayer);
        Transform target = FindHighestShardPower(hitColliders, detectionRadius);

        if (target != null)
        {
            MoveTowards(target);
        }
    }

    float CalculateDetectionRadius(float power)
    {
        return Mathf.Clamp(power / 2, 1, 10); 
    }

    Transform FindHighestShardPower(Collider2D[] hitColliders, float ownRadius)
    {
        Transform highestShardPowerTarget = null;
        float highestShardPower = 0.0f;

        foreach (var hitCollider in hitColliders)
        {
            if (!characterStatsCache.TryGetValue(hitCollider, out var characterStats))
            {
                characterStats = hitCollider.GetComponent<CharacterStats>();
                if (characterStats != null)
                {
                    characterStatsCache[hitCollider] = characterStats;
                }
            }

            if (characterStats != null)
            {
                float targetRadius = CalculateDetectionRadius(characterStats.shardPower);
                if (IsWithinInteractionRange(transform.position, hitCollider.transform.position, ownRadius, targetRadius)
                    && characterStats.shardPower > highestShardPower)
                {
                    highestShardPower = characterStats.shardPower;
                    highestShardPowerTarget = hitCollider.transform;
                }
            }
        }

        return highestShardPowerTarget;
    }

    bool IsWithinInteractionRange(Vector2 positionA, Vector2 positionB, float radiusA, float radiusB)
    {
        float distance = Vector2.Distance(positionA, positionB);
        return distance <= radiusA + radiusB;
    }

    void MoveTowards(Transform target)
    {
        Vector2 direction = (target.position - transform.position).normalized;
        rb.MovePosition(rb.position + direction * moveSpeed * Time.deltaTime);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

    void SwipeAttack(Transform target)
    {
        if (Time.time - lastAttackTime >= swipeCooldown)
        {
            InstantiateAttackObject(AttackType.swipe, target.position);
            lastAttackTime = Time.time;
        }
    }

    void StabAttack(Transform target)
    {
        if (Time.time - lastAttackTime >= stabCooldown)
        {
            InstantiateAttackObject(AttackType.stab, target.position);
            lastAttackTime = Time.time;
        }
    }

    private void InstantiateAttackObject(AttackType attackType, Vector3 position)
    {
        GameObject attackObject;

        if (attackType.Equals(AttackType.swipe))
        {
            attackObject = swipeAttackPool.GetAttackObject();
        }
        else
        {
            attackObject = stabAttackPool.GetAttackObject();
        }

        attackObject.transform.position = position;
        attackObject.SetActive(true);


        StartCoroutine(ReturnAttackObjectToPool(attackObject, attackType));
    }

    private IEnumerator ReturnAttackObjectToPool(GameObject attackObject, AttackType attackType)
    {
        yield return new WaitForSeconds(0.5f); 

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
