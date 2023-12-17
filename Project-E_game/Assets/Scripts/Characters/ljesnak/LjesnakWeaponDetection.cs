using System.Collections.Generic;
using UnityEngine;


public class LjesnakWeaponDetection : MonoBehaviour
{
    [Header("Weapon Settings")]
    public float weaponDamage;
    [SerializeField] private float baseForce = 100f;

    [Header("Weight Settings")]
    [SerializeField] private float shardWeight = 1f;
    [SerializeField] private float damageWeight = 1f;

    [Header("Caching")]
    private Dictionary<Collider2D, CharacterStats> statsCache = new Dictionary<Collider2D, CharacterStats>();

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
            targetStats.ApplyDamage(weaponDamage);
            targetStats.ApplyPostureDamage(weaponDamage);

            if (!targetStats.IsKnockbackImmune(CharacterStats.DamageType.Physical, weaponDamage))
            {
                ApplyKnockback(other, targetStats);
            }
        }
    }

    void ApplyKnockback(Collider2D other, CharacterStats targetStats)
    {
        float attackImpact = CalculateAttackImpact(targetStats.shardPower, weaponDamage);
        if (targetStats.rb != null)
        {
            Vector2 forceDirection = (other.transform.position - transform.position).normalized;
            float forceStrength = CalculateKnockbackForce(targetStats.poise, attackImpact);
            targetStats.rb.AddForce(forceDirection * forceStrength);
        }
        targetStats.Stun(CalculateKnockbackStunDuration(targetStats.poise, attackImpact));
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
        float normalizedShardWeight = shardWeight / (shardWeight + damageWeight);
        float normalizedDamageWeight = damageWeight / (shardWeight + damageWeight);

        // Weighted impact
        return (shardPower * normalizedShardWeight) + (weaponDamage * normalizedDamageWeight);
    }

    public void EndAttack()
    {
        gameObject.SetActive(false);
    }
}
