using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CharacterStats : MonoBehaviour
{
    public event Action<float> OnHealthChanged;
    public event Action<float> OnPostureChanged;
    public event Action<CharacterStats> OnDeath;
    [Header("Events")]
    public UnityEvent OnStunned;
    public UnityEvent OnStunRecovered;

    [Header("Basic Stats")]
    public float shardPower = 10;
    public float poise = 20;
    public float health;
    [SerializeField] private float maxHealth = 100f;

    [Header("Detection")]
    public float detectionRadius;
    public float maxDetectionRadius = 50f;

    [Header("Posture Stats")]
    [SerializeField] private float posture = 40;
    [SerializeField] private float maxPosture = 100f;
    [SerializeField] private float postureRegenerationRate = 1f;
    [SerializeField] private float recoveryBaseDelay = 1f;

    [Header("Stun Settings")]
    [SerializeField] private float poiseFactor = 0.1f;
    [SerializeField] private float baseStunDuration = 0.1f;

    [Header("Components")]
    public Rigidbody2D rb;
    public Animator animator;

    public enum DamageType { Physical, Magical }
    private bool isPostureBroken = false;

    void Start()
    {
        health = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        StartCoroutine(PostureRegeneration());
        detectionRadius = CalculateDetectionRadius(shardPower);
    }
    float CalculateDetectionRadius(float power)
    {
        power = Mathf.Abs(power);

        float baseRadius = 1f;

        float scaledRadius = baseRadius + Mathf.Log(power);

        return scaledRadius;
    }
    private IEnumerator PostureRegeneration()
    {
        while (true)
        {
            if (posture < maxPosture && !isPostureBroken)
            {
                float healthFactor = health / maxHealth;
                float poiseFactor = 1 + (poise / 2); // Adjust this to balance the influence of poise
                posture = Mathf.Min(posture + (postureRegenerationRate * healthFactor * poiseFactor * Time.deltaTime), maxPosture);
            }
            yield return null;
        }
    }

    public void ApplyPostureDamage(float amount)
    {
        posture -= amount;
        OnPostureChanged?.Invoke(posture);
        if (posture < 0)
            posture = 0;
        if (posture <= 0)
        {
            TriggerPostureBreak();
            StartCoroutine(PostureRecoveryDelay());
        }
    }

    private IEnumerator PostureRecoveryDelay()
    {
        isPostureBroken = true;

        // Calculate delay based on health and poise
        float healthFactor = health / maxHealth;
        float poiseFactor = 1 + (poise / 2); 
        float finalDelay = recoveryBaseDelay / (healthFactor * poiseFactor);

        yield return new WaitForSeconds(finalDelay);
        isPostureBroken = false;
        StartCoroutine(PostureRegeneration());
    }

    private void TriggerPostureBreak()
    {
        Debug.Log(gameObject.name + " posture broken.");
        Stun(CalculateStunDurationBasedOnPoise());
    }

    public void Stun(float duration)
    {
        OnStunned?.Invoke(); // Trigger event when stunned

        if (animator != null)
        {
            animator.SetTrigger("Stunned");
        }
        StartCoroutine(EndStun(duration));
    }

    private float CalculateStunDurationBasedOnPoise()
    {
        float stunDuration = baseStunDuration + (poiseFactor * poise);
        return stunDuration;
    }


    private IEnumerator EndStun(float duration)
    {
        yield return new WaitForSeconds(duration);
        OnStunRecovered?.Invoke();
    }


    public bool IsKnockbackImmune(DamageType damageType, float damageAmount)
    {
        return damageType == DamageType.Physical ? poise >= damageAmount * 0.8f : poise >= damageAmount;
    }

    public void ApplyDamage(float damageAmount)
    {
        health -= damageAmount;
        OnHealthChanged?.Invoke(health);

        if (health <= 0)
        {
            Die();
        }
        else
        {
            TriggerHitReaction();
        }
    }

    private void Die()
    {
        OnDeath?.Invoke(this);
        Debug.Log(gameObject.name + " has died.");
        Destroy(gameObject);
    }

    private void TriggerHitReaction()
    {
        Debug.Log(gameObject.name + " took damage.");
    }
    public void UpdateShardPower(float newShardPower)
    {
        shardPower = newShardPower;
        // Recalculate detection radius based on new shard power
        detectionRadius = CalculateDetectionRadius(shardPower);
    }

    public void GainShardPowerAndHealth(float powerGain, float healthGain)
    {
        shardPower += powerGain;
        health = Mathf.Min(health + healthGain, maxHealth);
  
        detectionRadius = CalculateDetectionRadius(shardPower);
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
