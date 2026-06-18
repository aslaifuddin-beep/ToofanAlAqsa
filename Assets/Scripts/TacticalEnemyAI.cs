using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class TacticalEnemyAI : MonoBehaviour
{
    public enum AIState
    {
        Patrol,
        Alert,
        Combat,
        Flanking,
        Cover,
        Suppressed,
        Dead
    }

    [Header("AI Settings")]
    public AIState currentState = AIState.Patrol;
    public float health = 100f;
    public float detectionRange = 30f;
    public float fieldOfView = 70f;
    public float fireRange = 25f;
    public float accuracy = 0.85f;

    [Header("Movement")]
    public float walkSpeed = 2f;
    public float runSpeed = 4f;
    public float patrolRadius = 20f;
    public float minTimeAtPatrolPoint = 2f;

    [Header("Combat")]
    public float fireRate = 0.3f;
    public float damagePerShot = 15f;
    public float suppressionThreshold = 30f;
    public float flankDelay = 3f;

    [Header("Cover")]
    public LayerMask coverLayerMask = ~0;
    public float coverCheckRadius = 5f;
    public float minCoverTime = 2f;
    public float maxCoverTime = 4f;

    [Header("References")]
    public NavMeshAgent agent;
    public Transform weaponMuzzle;
    public AudioSource audioSource;
    public AudioClip fireSound;
    public Animator animator;

    private Transform player;
    private HealthSystem playerHealth;
    private Vector3 patrolOrigin;
    private Vector3 patrolTarget;
    private float stateTimer;
    private float lastFireTime;
    private float suppressionLevel;
    private Vector3 lastKnownPlayerPos;
    private Vector3 currentCoverPos;
    private bool hasCover;
    private int patrolIndex;

    void Start()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        patrolOrigin = transform.position;
        player = Camera.main?.transform;

        if (player != null)
            playerHealth = player.GetComponentInParent<HealthSystem>();

        SetState(AIState.Patrol);
        PickNewPatrolTarget();
    }

    void Update()
    {
        if (currentState == AIState.Dead) return;
        if (agent == null) return;

        stateTimer -= Time.deltaTime;

        if (player == null)
        {
            player = Camera.main?.transform;
            return;
        }

        if (!agent.isOnNavMesh)
        {
            NavMeshHit navHit;
            if (NavMesh.SamplePosition(transform.position, out navHit, 5f, NavMesh.AllAreas))
                agent.Warp(navHit.position);
            else
                return;
        }

        float distToPlayer = Vector3.Distance(transform.position, player.position);
        bool canSeePlayer = CanSeePlayer();

        suppressionLevel = Mathf.Max(0, suppressionLevel - Time.deltaTime * 5f);

        switch (currentState)
        {
            case AIState.Patrol:        UpdatePatrol(distToPlayer, canSeePlayer); break;
            case AIState.Alert:         UpdateAlert(distToPlayer, canSeePlayer); break;
            case AIState.Combat:        UpdateCombat(distToPlayer, canSeePlayer); break;
            case AIState.Flanking:      UpdateFlanking(distToPlayer, canSeePlayer); break;
            case AIState.Cover:         UpdateCover(distToPlayer, canSeePlayer); break;
            case AIState.Suppressed:    UpdateSuppressed(distToPlayer, canSeePlayer); break;
        }

        if (animator != null)
        {
            animator.SetFloat("Speed", agent.velocity.magnitude / agent.speed);
            animator.SetBool("InCombat", currentState == AIState.Combat || currentState == AIState.Flanking);
        }
    }

    bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dirToPlayer);

        if (angle > fieldOfView * 0.5f) return false;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > detectionRange) return false;

        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 1f, dirToPlayer, out hit, dist))
        {
            if (hit.transform == player || hit.transform.IsChildOf(player))
            {
                lastKnownPlayerPos = player.position;
                return true;
            }
        }

        return false;
    }

    void UpdatePatrol(float dist, bool canSee)
    {
        if (agent == null) return;
        agent.speed = walkSpeed;

        if (canSee)
        {
            SetState(AIState.Alert);
            return;
        }

        if (agent.isOnNavMesh && dist < detectionRange * 0.5f && !canSee)
        {
            agent.isStopped = false;
        }

        if (agent.hasPath && !agent.pathPending && agent.remainingDistance < 0.5f)
        {
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0f)
            {
                PickNewPatrolTarget();
            }
        }
    }

    void UpdateAlert(float dist, bool canSee)
    {
        if (agent == null || !agent.isOnNavMesh) return;
        agent.speed = runSpeed;

        if (canSee)
        {
            lastKnownPlayerPos = player.position;
            SetState(AIState.Combat);
            return;
        }

        if (agent.isOnNavMesh)
            agent.SetDestination(lastKnownPlayerPos);

        if (agent.hasPath && agent.remainingDistance < 1f)
        {
            SetState(AIState.Patrol);
        }
    }

    void UpdateCombat(float dist, bool canSee)
    {
        if (agent == null) return;
        agent.speed = runSpeed;

        if (!canSee)
        {
            SetState(AIState.Alert);
            return;
        }

        lastKnownPlayerPos = player.position;

        if (dist > fireRange)
        {
            if (agent.isOnNavMesh)
            {
                agent.SetDestination(player.position);
                agent.isStopped = false;
            }
        }
        else
        {
            agent.isStopped = true;
            FaceTarget();

            ShootAtPlayer();
        }

        if (suppressionLevel > suppressionThreshold)
        {
            SetState(AIState.Suppressed);
            return;
        }

        if (stateTimer <= 0f && dist < fireRange * 0.8f)
        {
            float roll = Random.value;
            if (roll < 0.4f)
                SetState(AIState.Cover);
            else if (roll < 0.7f)
                SetState(AIState.Flanking);
            stateTimer = flankDelay;
        }
    }

    void UpdateFlanking(float dist, bool canSee)
    {
        if (agent == null) return;
        agent.speed = runSpeed;

        if (!canSee)
        {
            SetState(AIState.Alert);
            return;
        }

        Vector3 flankDir = transform.right * (Random.value > 0.5f ? 1 : -1);
        Vector3 flankTarget = lastKnownPlayerPos + flankDir * 10f + Vector3.forward * 5f;

        if (agent.isOnNavMesh)
        {
            agent.SetDestination(flankTarget);
            agent.isStopped = false;
        }

        if (canSee && dist < fireRange)
        {
            ShootAtPlayer();
        }

        if (agent.isOnNavMesh && Vector3.Distance(transform.position, flankTarget) < 2f)
        {
            SetState(AIState.Combat);
        }

        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            SetState(AIState.Combat);
        }
    }

    void UpdateCover(float dist, bool canSee)
    {
        if (agent == null) return;
        agent.speed = walkSpeed;

        if (!hasCover)
        {
            FindCoverPosition();
        }

        if (hasCover && agent.isOnNavMesh)
        {
            agent.SetDestination(currentCoverPos);
            agent.isStopped = false;

            if (agent.hasPath && agent.remainingDistance < 1f)
            {
                agent.isStopped = true;
                FaceTarget();

                if (canSee)
                {
                    ShootAtPlayer();
                }

                if (stateTimer <= 0f)
                {
                    SetState(AIState.Combat);
                }
            }
        }
        else
        {
            if (dist > fireRange * 0.5f)
            {
                SetState(AIState.Combat);
            }
        }
    }

    void UpdateSuppressed(float dist, bool canSee)
    {
        if (agent == null) return;
        agent.speed = runSpeed;

        FindCoverPosition();
        if (agent.isOnNavMesh)
        {
            if (hasCover)
                agent.SetDestination(currentCoverPos);
            else
            {
                Vector3 retreatDir = (transform.position - lastKnownPlayerPos).normalized;
                agent.SetDestination(transform.position + retreatDir * 15f);
            }
            agent.isStopped = false;
        }

        suppressionLevel -= Time.deltaTime * 10f;
        if (suppressionLevel <= 0f)
        {
            SetState(AIState.Combat);
        }
    }

    void ShootAtPlayer()
    {
        if (Time.time - lastFireTime < fireRate) return;
        if (player == null) return;

        lastFireTime = Time.time;

        float hitChance = accuracy;
        float dist = Vector3.Distance(transform.position, player.position);
        hitChance -= (dist / fireRange) * 0.2f;

        if (Random.value <= hitChance)
        {
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damagePerShot, DamageSystem.BodyPart.Torso);
            }
        }

        if (audioSource != null && fireSound != null)
        {
            audioSource.PlayOneShot(fireSound);
        }

        if (weaponMuzzle != null)
        {
            GameObject flash = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            flash.transform.position = weaponMuzzle.position;
            flash.transform.localScale = Vector3.one * 0.1f;
            flash.GetComponent<Renderer>().material.color = Color.yellow;
            Destroy(flash, 0.05f);
        }

        suppressionLevel += 10f;
    }

    void FindCoverPosition()
    {
        hasCover = false;

        if (player == null) return;

        Vector3 playerDir = (transform.position - player.position).normalized;

        Collider[] obstacles = Physics.OverlapSphere(transform.position, coverCheckRadius, coverLayerMask);
        foreach (Collider col in obstacles)
        {
            if (col == null || col.isTrigger) continue;

            Vector3 dirToObstacle = (col.transform.position - player.position).normalized;
            float dot = Vector3.Dot(dirToObstacle, playerDir);

            if (dot > 0.3f)
            {
                Vector3 coverPoint = col.ClosestPoint(transform.position);
                Vector3 offset = (coverPoint - col.transform.position).normalized * 1.5f;
                currentCoverPos = coverPoint + offset;

                NavMeshHit navHit;
                if (NavMesh.SamplePosition(currentCoverPos, out navHit, 2f, NavMesh.AllAreas))
                {
                    currentCoverPos = navHit.position;
                    hasCover = true;
                    return;
                }
            }
        }

        if (!hasCover)
        {
            currentCoverPos = transform.position + playerDir * 5f;
            NavMeshHit navHit;
            if (NavMesh.SamplePosition(currentCoverPos, out navHit, 2f, NavMesh.AllAreas))
            {
                currentCoverPos = navHit.position;
                hasCover = true;
            }
        }
    }

    void PickNewPatrolTarget()
    {
        Vector3 randomDir = Random.insideUnitSphere * patrolRadius;
        randomDir.y = 0;
        patrolTarget = patrolOrigin + randomDir;

        NavMeshHit navHit;
        if (NavMesh.SamplePosition(patrolTarget, out navHit, patrolRadius, NavMesh.AllAreas))
        {
            if (agent != null && agent.isOnNavMesh)
                agent.SetDestination(navHit.position);
        }

        stateTimer = minTimeAtPatrolPoint + Random.Range(0f, 3f);
    }

    void FaceTarget()
    {
        if (player == null) return;

        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 5f);
    }

    public void TakeDamage(float damage, Vector3 hitOrigin)
    {
        health -= damage;
        suppressionLevel += damage * 2f;

        lastKnownPlayerPos = hitOrigin;

        if (health <= 0)
        {
            SetState(AIState.Dead);
            return;
        }

        if (currentState == AIState.Patrol || currentState == AIState.Alert)
        {
            SetState(AIState.Combat);
        }
    }

    public void SetState(AIState newState)
    {
        if (currentState == AIState.Dead) return;

        currentState = newState;
        stateTimer = 0f;

        switch (newState)
        {
            case AIState.Patrol:
                agent.isStopped = false;
                agent.speed = walkSpeed;
                break;
            case AIState.Alert:
                agent.isStopped = false;
                agent.speed = runSpeed;
                break;
            case AIState.Combat:
                agent.isStopped = false;
                agent.speed = runSpeed;
                stateTimer = Random.Range(3f, 6f);
                break;
            case AIState.Flanking:
                agent.isStopped = false;
                agent.speed = runSpeed;
                stateTimer = Random.Range(3f, 5f);
                break;
            case AIState.Cover:
                agent.isStopped = false;
                agent.speed = walkSpeed;
                stateTimer = Random.Range(minCoverTime, maxCoverTime);
                break;
            case AIState.Suppressed:
                agent.isStopped = false;
                agent.speed = runSpeed;
                break;
            case AIState.Dead:
                agent.isStopped = true;
                agent.enabled = false;
                if (animator != null) animator.SetTrigger("Die");
                Destroy(gameObject, 3f);
                break;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Vector3 fovLeft = Quaternion.Euler(0, -fieldOfView * 0.5f, 0) * transform.forward * detectionRange;
        Vector3 fovRight = Quaternion.Euler(0, fieldOfView * 0.5f, 0) * transform.forward * detectionRange;
        Gizmos.DrawLine(transform.position, transform.position + fovLeft);
        Gizmos.DrawLine(transform.position, transform.position + fovRight);

        if (hasCover)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(currentCoverPos, 0.3f);
        }
    }
}
