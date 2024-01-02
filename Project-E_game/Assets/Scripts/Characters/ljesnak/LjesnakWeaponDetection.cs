using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class LjesnakWeaponDetection : MonoBehaviour
{
    [Header("Weapon Settings")]
    public float weaponDamage;
    [SerializeField] private float baseForce = 100f;

    [Header("Weight Settings")]
    [SerializeField] private float targetShardWeight = 1f;
    [SerializeField] private float damageWeight = 1f;

    [Header("Caching")]
    private Dictionary<Collider2D, CharacterStats> statsCache = new Dictionary<Collider2D, CharacterStats>();
    private CharacterStats characterStats;

    [SerializeField] ljesnakController ljesnakController;
    private bool attackHit = false;

    private void Start()
    {
        characterStats = GetComponentInParent<CharacterStats>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!statsCache.TryGetValue(other, out CharacterStats targetStats))
        {
            targetStats = other.GetComponent<CharacterStats>();
            if (targetStats != null)
            {
                statsCache[other] = targetStats;
            }
        }

        if (targetStats != null)
        {
            attackHit = true;
            targetStats.ApplyDamage(weaponDamage);
            targetStats.ApplyPostureDamage(weaponDamage);
            if(!targetStats.IsKnockbackImmune(CharacterStats.DamageType.Physical, weaponDamage))
            {
                ApplyKnockback(other, targetStats);
            }
            if (targetStats.health <= 0)
            {
                float shardPowerGain = targetStats.shardPower * 0.1f;
                float healthGain = targetStats.shardPower;

                if (characterStats != null)
                {
                    characterStats.GainShardPowerAndHealth(shardPowerGain, healthGain);
                }
                ljesnakController.OnEnemySlain(shardPowerGain);
            }
            ljesnakController.OnAttackHitTarget();
        }
    }

    void ApplyKnockback(Collider2D other, CharacterStats targetStats)
    {
        Debug.Log("Knockback!!!");
        float attackImpact = CalculateAttackImpact(targetStats.shardPower, weaponDamage);
        if (targetStats.rb != null)
        {
            Vector2 forceDirection = (other.transform.position - transform.position).normalized;
            float forceStrength = CalculateKnockbackForce(targetStats.poise, attackImpact);
            targetStats.rb.AddForce(forceDirection * forceStrength);
        }
        targetStats.Stunned(CalculateKnockbackStunDuration(targetStats.poise, attackImpact));
    }

    float CalculateKnockbackForce(float targetPoise, float attackImpact)
    {
        float poiseFactor = Mathf.Clamp(1 - (targetPoise / 100f), 0.1f, 1f);
        return Mathf.Log(1 + baseForce * poiseFactor * attackImpact);
    }

    float CalculateKnockbackStunDuration(float targetPoise, float attackImpact)
    {
        float baseDuration = 0.05f;
        float durationFactor = 0.1f;
        float excessImpact = attackImpact - targetPoise;
        float additionalDuration = Mathf.Max(0, Mathf.Log(1 + excessImpact) * durationFactor);
        return baseDuration + additionalDuration;
    }

    float CalculateAttackImpact(float shardPower, float weaponDamage)
    {
        // Normalized weights
        float normalizedShardWeight = targetShardWeight / (targetShardWeight + damageWeight);
        float normalizedDamageWeight = damageWeight / (targetShardWeight + damageWeight);

        // Weighted impact
        return (shardPower * normalizedShardWeight) + (weaponDamage * normalizedDamageWeight);
    }

    public void EndAttack()
    {
        if (transform.parent != null)
        {
            transform.parent.gameObject.SetActive(false);
        }
        if (!attackHit)
        {
            ljesnakController.OnAttackMissed();
        }
        attackHit = false;
    }
}
