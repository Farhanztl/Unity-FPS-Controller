using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// ZombieController - Smart zombie enemy that:
///   1. Patrols at normal speed.
///   2. Detects the player via line-of-sight (Raycast) within a detection radius.
///   3. Accelerates (sprints) toward the player when spotted.
///   4. Shoots projectiles at the player when in range.
///   5. Loses the player after losing sight for a while, then returns to patrol.
/// </summary>
public class ZombieController : MonoBehaviour
{
    // ── References ──────────────────────────────────────────────
    public NavMeshAgent agent;
    public Transform firePoint;       // Empty child transform on the zombie's "hand" or "mouth"
    public GameObject bulletPrefab;   // Drag the "Enemy Bullet" prefab here

    // ── Detection ────────────────────────────────────────────────
    [Header("Detection")]
    public float detectionRadius   = 12f;  // Radius to start looking for the player
    public float loseRadius        = 18f;  // Distance at which zombie gives up chasing
    public float lineOfSightAngle  = 120f; // Field-of-view angle (degrees) for initial detection

    // ── Movement ─────────────────────────────────────────────────
    [Header("Movement")]
    public float patrolSpeed       = 2.5f; // Normal patrol/wander speed
    public float chaseSpeed        = 6f;   // Sprint speed when chasing
    public float stopDistance      = 3f;   // Stop moving when this close to player (shoot instead)

    // ── Shooting ─────────────────────────────────────────────────
    [Header("Shooting")]
    public float fireRate          = 2f;   // Seconds between shots
    public float aimLeadFactor     = 0.2f; // How much to lead the player's movement

    // ── Patrol ───────────────────────────────────────────────────
    [Header("Patrol")]
    public float patrolWaitTime    = 3f;   // Seconds to wait at each patrol point
    public float patrolRadius      = 10f;  // Radius around startPoint to pick patrol destinations

    // ── Private state ────────────────────────────────────────────
    private enum ZombieState { Patrol, Chase, Shoot }
    private ZombieState state = ZombieState.Patrol;

    private Vector3 startPoint;
    private float   fireTimer;
    private float   patrolTimer;
    private float   lostSightTimer;
    private float   lostSightGrace = 3f;   // Seconds to keep chasing after losing sight

    private Transform playerTransform;
    private CharacterController playerCC; // Used to estimate player velocity for aim-lead

    // ── Layer mask (optional): set to ignore the zombie itself ───
    [Header("Layer Masks")]
    public LayerMask obstacleMask;  // Assign layers that block line-of-sight (walls, terrain, etc.)

    // ────────────────────────────────────────────────────────────
    void Start()
    {
        startPoint        = transform.position;
        fireTimer         = fireRate;
        patrolTimer       = patrolWaitTime;

        if (PlayerController.instance != null)
        {
            playerTransform = PlayerController.instance.transform;
            playerCC        = PlayerController.instance.GetComponent<CharacterController>();
        }

        agent.speed = patrolSpeed;
        PickNewPatrolPoint();
    }

    // ────────────────────────────────────────────────────────────
    void Update()
    {
        if (playerTransform == null) return;

        switch (state)
        {
            case ZombieState.Patrol: UpdatePatrol(); break;
            case ZombieState.Chase:  UpdateChase();  break;
            case ZombieState.Shoot:  UpdateShoot();  break;
        }
    }

    // ── PATROL ──────────────────────────────────────────────────
    void UpdatePatrol()
    {
        agent.speed = patrolSpeed;

        // Check if we can see the player -> switch to Chase
        if (CanSeePlayer())
        {
            EnterChase();
            return;
        }

        // Wait at patrol point, then pick a new one
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            patrolTimer -= Time.deltaTime;
            if (patrolTimer <= 0f)
            {
                PickNewPatrolPoint();
                patrolTimer = patrolWaitTime;
            }
        }
    }

    // ── CHASE ───────────────────────────────────────────────────
    void UpdateChase()
    {
        agent.speed = chaseSpeed;

        float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // Lost player too far away
        if (distToPlayer > loseRadius)
        {
            lostSightTimer += Time.deltaTime;
            if (lostSightTimer >= lostSightGrace)
            {
                EnterPatrol();
                return;
            }
        }
        else
        {
            lostSightTimer = 0f;
        }

        // Close enough to shoot
        if (distToPlayer <= stopDistance)
        {
            agent.ResetPath(); // Stop moving
            state = ZombieState.Shoot;
            return;
        }

        // Navigate toward player
        agent.SetDestination(playerTransform.position);

        // Face the player
        FaceTarget(playerTransform.position);
    }

    // ── SHOOT ───────────────────────────────────────────────────
    void UpdateShoot()
    {
        float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // Player moved away -> chase again
        if (distToPlayer > stopDistance + 1f)
        {
            state = ZombieState.Chase;
            return;
        }

        // Lost player -> patrol
        if (distToPlayer > loseRadius)
        {
            EnterPatrol();
            return;
        }

        // Always face the player while shooting
        FaceTarget(playerTransform.position);

        // Fire
        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f)
        {
            fireTimer = fireRate;
            FireAtPlayer();
        }
    }

    // ────────────────────────────────────────────────────────────
    // Line-of-sight check: distance + FOV angle + Raycast (no walls)
    bool CanSeePlayer()
    {
        float dist = Vector3.Distance(transform.position, playerTransform.position);
        if (dist > detectionRadius) return false;

        // Check if the player is within the forward FOV cone
        Vector3 dirToPlayer = (playerTransform.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dirToPlayer);
        if (angle > lineOfSightAngle * 0.5f) return false;

        // Raycast to make sure no wall is in the way
        Vector3 eyePos    = transform.position + Vector3.up * 1.5f;
        Vector3 playerEye = playerTransform.position + Vector3.up * 1f;

        if (obstacleMask != 0 && Physics.Raycast(eyePos, (playerEye - eyePos).normalized,
                            out RaycastHit hit,
                            dist,
                            obstacleMask))
        {
            // Hit something before reaching the player -> blocked
            return false;
        }

        return true;
    }

    // ────────────────────────────────────────────────────────────
    void FireAtPlayer()
    {
        if (bulletPrefab == null || firePoint == null) return;

        // Aim with lead: predict where player will be in aimLeadFactor seconds
        Vector3 predictedPos = playerTransform.position;
        if (playerCC != null)
        {
            predictedPos += playerCC.velocity * aimLeadFactor;
        }

        // Aim firePoint at predicted position (aim at chest height)
        Vector3 aimDir = (predictedPos + Vector3.up * 1f) - firePoint.position;
        if (aimDir != Vector3.zero)
            firePoint.rotation = Quaternion.LookRotation(aimDir);

        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
    }

    // ────────────────────────────────────────────────────────────
    void EnterChase()
    {
        state          = ZombieState.Chase;
        lostSightTimer = 0f;
        agent.speed    = chaseSpeed;
    }

    void EnterPatrol()
    {
        state          = ZombieState.Patrol;
        patrolTimer    = patrolWaitTime;
        agent.speed    = patrolSpeed;
        agent.SetDestination(startPoint);
        PickNewPatrolPoint();
    }

    void PickNewPatrolPoint()
    {
        // Random point within patrolRadius around the start point
        Vector2 rand    = Random.insideUnitCircle * patrolRadius;
        Vector3 target  = startPoint + new Vector3(rand.x, 0f, rand.y);

        // Make sure the point is on the NavMesh
        if (NavMesh.SamplePosition(target, out NavMeshHit navHit, patrolRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(navHit.position);
        }
        else
        {
            agent.SetDestination(startPoint); // Fallback
        }
    }

    void FaceTarget(Vector3 target)
    {
        Vector3 dir        = (target - transform.position);
        dir.y              = 0f;
        if (dir == Vector3.zero) return;
        Quaternion lookRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 8f);
    }

    // ────────────────────────────────────────────────────────────
    // Draw detection ranges in the Scene view for easy tuning
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, loseRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
}
