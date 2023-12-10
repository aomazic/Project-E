using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LjesnakWeaponDetection : MonoBehaviour
{
    public float weaponDamage;

    void OnTriggerEnter2D(Collider2D other)
    {
        CharacterStats targetStats = other.GetComponent<CharacterStats>();
        if (targetStats != null)
        {
            // Apply damage
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
        if (targetStats.rb != null)
        {
            Vector2 forceDirection = (other.transform.position - transform.position).normalized;
            float forceStrength = CalculateKnockbackForce(targetStats);
            targetStats.rb.AddForce(forceDirection * forceStrength);
        }

        if (targetStats.animator != null)
        {
            targetStats.animator.SetTrigger("Interrupt");
        }
    }

    float CalculateKnockbackForce(CharacterStats targetStats)
    {

        return 500f / targetStats.poise; 
    }
}
