using UnityEngine;
using UnityEngine.AI;

public class TankAI : MonoBehaviour
{
    [Header("Tank Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 30f;
    public float stoppingDistance = 30f;
    public float minDistance = 15f;

    [Header("Turret")]
    public Transform turretPivot;
    public Transform cannonBarrel;
    public float turretRotationSpeed = 20f;
    public float turretTrackingAccuracy = 5f;

    [Header("Cannon")]
    public float fireRate = 4f;
    public float cannonDamage = 80f;
    public float cannonRange = 50f;
    public float explosionRadius = 8f;
    public GameObject projectilePrefab;
    public Transform firePoint;

    [Header("Audio")]
    public AudioSource engineAudioSource;
    public AudioSource cannonAudioSource;
    public AudioClip engineSound;
    public AudioClip cannonFireSound;

    [Header("Health")]
    public float maxHealth = 500f;
    private float currentHealth;

    private Transform player;
    private NavMeshAgent agent;
    private float lastFireTime;

    public bool IsAlive { get; private set; } = true;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = moveSpeed;
            agent.stoppingDistance = stoppingDistance;
        }

        currentHealth = maxHealth;
        player = Camera.main?.transform;

        if (engineAudioSource != null && engineSound != null)
        {
            engineAudioSource.clip = engineSound;
            engineAudioSource.loop = true;
            engineAudioSource.Play();
        }
    }

    void Update()
    {
        if (!IsAlive) return;

        if (player == null)
        {
            player = Camera.main?.transform;
            return;
        }

        HandleMovement();
        HandleTurret();
        HandleFiring();
        UpdateAudio();
    }

    void HandleMovement()
    {
        if (agent == null || !agent.isOnNavMesh) return;

        float distToPlayer = Vector3.Distance(transform.position, player.position);

        if (distToPlayer > stoppingDistance)
        {
            agent.SetDestination(player.position);
            agent.isStopped = false;
        }
        else if (distToPlayer < minDistance)
        {
            Vector3 retreatDir = (transform.position - player.position).normalized;
            Vector3 retreatPos = transform.position + retreatDir * 10f;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(retreatPos, out hit, 5f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                agent.isStopped = false;
            }
        }
        else
        {
            agent.isStopped = true;
            FacePlayer();
        }
    }

    void FacePlayer()
    {
        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0;
        if (dir.magnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }
    }

    void HandleTurret()
    {
        if (turretPivot == null || player == null) return;

        Vector3 targetDir = player.position - turretPivot.position;
        Quaternion targetRot = Quaternion.LookRotation(targetDir);

        turretPivot.rotation = Quaternion.RotateTowards(
            turretPivot.rotation,
            targetRot,
            turretRotationSpeed * Time.deltaTime
        );

        if (cannonBarrel != null)
        {
            float angle = Vector3.Angle(turretPivot.forward, targetDir);
            if (angle < turretTrackingAccuracy)
            {
                cannonBarrel.LookAt(player.position);
            }
        }
    }

    void HandleFiring()
    {
        if (cannonBarrel == null) return;
        if (Time.time - lastFireTime < fireRate) return;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > cannonRange) return;

        Vector3 dirToPlayer = (player.position - cannonBarrel.position).normalized;
        float angle = Vector3.Angle(cannonBarrel.forward, dirToPlayer);

        if (angle < turretTrackingAccuracy)
        {
            FireCannon();
        }
    }

    void FireCannon()
    {
        lastFireTime = Time.time;

        if (projectilePrefab != null && firePoint != null)
        {
            GameObject proj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            YasinRPG_Projectile rpg = proj.GetComponent<YasinRPG_Projectile>();
            if (rpg != null)
            {
                rpg.Initialize(firePoint.forward, 0.7f);
                rpg.explosionDamage = cannonDamage;
                rpg.explosionRadius = explosionRadius;
            }

            BulletProjectile bullet = proj.GetComponent<BulletProjectile>();
            if (bullet != null)
            {
                bullet.Initialize(firePoint.forward, 0.5f);
                bullet.baseDamage = cannonDamage;
            }
        }

        if (cannonAudioSource != null && cannonFireSound != null)
        {
            cannonAudioSource.PlayOneShot(cannonFireSound);
        }

        GameObject flash = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        flash.transform.position = firePoint.position;
        flash.transform.localScale = Vector3.one * 0.5f;
        flash.GetComponent<Renderer>().material.color = Color.yellow;
        Destroy(flash, 0.1f);
    }

    void UpdateAudio()
    {
        if (engineAudioSource == null) return;

        if (agent != null && agent.velocity.magnitude > 0.5f)
        {
            engineAudioSource.pitch = 0.8f + (agent.velocity.magnitude / agent.speed) * 0.4f;
            engineAudioSource.volume = 0.6f + (agent.velocity.magnitude / agent.speed) * 0.4f;
        }
        else
        {
            engineAudioSource.pitch = Mathf.Lerp(engineAudioSource.pitch, 0.5f, Time.deltaTime * 2f);
            engineAudioSource.volume = Mathf.Lerp(engineAudioSource.volume, 0.3f, Time.deltaTime * 2f);
        }
    }

    public void TakeDamage(float damage)
    {
        if (!IsAlive) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    void Die()
    {
        IsAlive = false;

        if (agent != null) agent.isStopped = true;

        if (engineAudioSource != null)
            engineAudioSource.Stop();

        GameManager.Instance?.AddKill();

        GameObject explosion = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        explosion.transform.position = transform.position;
        explosion.transform.localScale = Vector3.one * 5f;
        explosion.GetComponent<Renderer>().material.color = Color.red;
        Object.Destroy(explosion, 2f);

        Destroy(gameObject, 3f);
    }
}
