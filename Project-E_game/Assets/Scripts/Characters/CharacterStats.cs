using System;
using System.Collections;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    public event Action<float> OnHealthChanged;
    public event Action<float> OnPostureChanged;
    public event Action<CharacterStats> OnDeath;


    [Header("Basic Stats")]
    public float shardPower;
    public float poise;
    [SerializeField] private float health;
    [SerializeField] private float maxHealth;
    [SerializeField] private float damage;
 
    [Header("Posture Stats")]
    [SerializeField] private float posture;
    [SerializeField] private float maxPosture = 100f;
    [SerializeField] private float postureRegenerationRate = 1f;

    [Header("Components")]
    public Rigidbody2D rb;
    public Animator animator;

    public enum DamageType { Physical, Magical }

    void Start()
    {
        health = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        StartCoroutine(PostureRegeneration());
    }

    private IEnumerator PostureRegeneration()
    {
        while (true)
        {
            if (posture < maxPosture)
            {
                float healthFactor = health / maxHealth;
                posture = Mathf.Min(posture + (postureRegenerationRate * healthFactor * Time.deltaTime), maxPosture);
            }
            yield return null;
        }
    }

    public void ApplyPostureDamage(float amount)
    {
        posture -= amount;
        OnPostureChanged?.Invoke(posture);
        if (posture <= 0)
        {
            TriggerPostureBreak();
            posture = maxPosture;
        }
    }

    private void TriggerPostureBreak()
    {
        Debug.Log(gameObject.name + " posture broken.");
        if (animator != null)
        {
            animator.SetTrigger("Stunned");
        }
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
}
