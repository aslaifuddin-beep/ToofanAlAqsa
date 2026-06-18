using UnityEngine;
using UnityEngine.Events;

public class HealthSystem : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Armor")]
    public float armor = 0f;
    public float maxArmor = 50f;

    [Header("Death")]
    public bool destroyOnDeath = true;
    public float destroyDelay = 3f;
    public GameObject deathEffectPrefab;

    [Header("Events")]
    public UnityEvent onDamageTaken;
    public UnityEvent onHealed;
    public UnityEvent onDeath;

    public bool IsDead { get; private set; }
    public float HealthPercent => currentHealth / maxHealth;
    public float ArmorPercent => armor / maxArmor;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage, DamageSystem.BodyPart hitPart = DamageSystem.BodyPart.Torso)
    {
        if (IsDead) return;

        float damageMultiplier = DamageSystem.GetBodyPartMultiplier(hitPart);
        float finalDamage = damage * damageMultiplier;

        if (armor > 0)
        {
            float armorAbsorb = Mathf.Min(armor, finalDamage * 0.5f);
            armor -= armorAbsorb;
            finalDamage -= armorAbsorb;
        }

        currentHealth -= finalDamage;
        onDamageTaken.Invoke();

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (IsDead) return;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        onHealed.Invoke();
    }

    public void AddArmor(float amount)
    {
        armor = Mathf.Min(armor + amount, maxArmor);
    }

    void Die()
    {
        IsDead = true;
        onDeath.Invoke();

        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }

        if (destroyOnDeath)
        {
            Destroy(gameObject, destroyDelay);
        }
    }
}
