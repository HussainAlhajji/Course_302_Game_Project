using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public enum EnemyState
{
    Patrol,
    Chase,
    Attack,
    Stunned
}

public class EnemyController : MonoBehaviour
{
    [Header("Components")]
    public NavMeshAgent navMeshAgent;
    public Animator animator;
    public AudioSource audioSource;

    [Header("Enemy Stats")]
    public float health = 100f;
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public float attackRange = 2.5f;
    public float attackDamage = 10f;
    public float attackCooldown = 1.5f;
    public float minChaseDistance = 2.0f;
    public float retreatDistance = 1.5f;

    [Header("Detection Settings")]
    public float sightRange = 10f;
    public float fieldOfView = 120f;
    public Transform eyePosition;

    [Header("Sound Effects")]
    public AudioClip attackSound;
    public AudioClip hurtSound;
    public AudioClip deathSound;
    public AudioClip spottedSound;
    public AudioClip[] footstepSounds;
    [Range(0, 1)] public float footstepVolume = 0.5f;
    [Range(0.5f, 1.5f)] public float pitchVariation = 0.2f;

    [Header("Patrol Settings")]
    public float patrolRadius = 20f; // Radius for random patrol points
    public float waitTimeAtPatrolPoint = 2f; // Time to wait at each patrol point

    [Header("Death Settings")]
    public float disappearDelay = 1f; // Time before the enemy disappears after death

    [Header("Player Slowdown Settings")]
    [Range(0.1f, 1f)] public float slowdownMultiplier = 0.8f; // Multiplier for player's speed (e.g., 0.8 = 80% of original speed)
    [Range(0.1f, 5f)] public float slowdownDuration = 1f; // Duration of the slowdown effect in seconds
    public float minPlayerSpeed = 2f; // Minimum speed the player can be slowed to

    // Private variables
    private Transform playerTransform;
    public EnemyState currentState = EnemyState.Patrol;
    private float lastAttackTime;
    private bool shouldRetreatAfterAttack;
    private float nextGrowlTime;
    private float nextFootstepTime;
    private bool isWaiting = false;

    // Animation hashes
    private static readonly int SpeedParam = Animator.StringToHash("Speed");
    private static readonly int StateParam = Animator.StringToHash("State");
    private static readonly int AttackParam = Animator.StringToHash("Attack");
    private static readonly int DieParam = Animator.StringToHash("Die");

    private void Awake()
    {
        // Initialize components
        if (!navMeshAgent) navMeshAgent = GetComponent<NavMeshAgent>();
        if (!animator) animator = GetComponent<Animator>();
        if (!audioSource) audioSource = gameObject.AddComponent<AudioSource>();

        // Configure audio
        audioSource.spatialBlend = 1f; // 3D sound
        audioSource.minDistance = 3f;
        audioSource.maxDistance = 15f;

        // Find player
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Set up eye position if not assigned
        if (!eyePosition)
        {
            eyePosition = new GameObject("EyePosition").transform;
            eyePosition.SetParent(transform);
            eyePosition.localPosition = new Vector3(0, 1.6f, 0.2f);
        }
    }

    private void Start()
    {
        navMeshAgent.speed = patrolSpeed;
        navMeshAgent.stoppingDistance = minChaseDistance;
        lastAttackTime = -attackCooldown; // Allow immediate attack
        SetRandomPatrolPoint();
    }

    private void Update()
    {
        if (!playerTransform)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
            return;
        }

        // State machine
        switch (currentState)
        {
            case EnemyState.Patrol:
                Patrol();
                CheckForPlayer();
                break;

            case EnemyState.Chase:
                ChasePlayer();
                CheckAttackRange();
                break;

            case EnemyState.Attack:
                AttackPlayer();
                break;

            case EnemyState.Stunned:
                break;
        }

        UpdateAnimator();
    }

    private void UpdateAnimator()
    {
        if (animator)
        {
            animator.SetFloat(SpeedParam, navMeshAgent.velocity.magnitude);
            animator.SetInteger(StateParam, (int)currentState);
        }
    }

    // ===== SOUND METHODS =====
    public void PlayFootstepSound()
    {
        if (footstepSounds.Length == 0 || !audioSource) return;

        int index = Random.Range(0, footstepSounds.Length);
        audioSource.pitch = Random.Range(1f - pitchVariation, 1f + pitchVariation);
        audioSource.PlayOneShot(footstepSounds[index], footstepVolume);
    }

    private void PlayAttackSound()
    {
        if (attackSound && audioSource)
            audioSource.PlayOneShot(attackSound);
    }

    private void PlayHurtSound()
    {
        if (hurtSound && audioSource)
            audioSource.PlayOneShot(hurtSound);
    }

    private void PlayDeathSound()
    {
        if (deathSound && audioSource)
            audioSource.PlayOneShot(deathSound);
    }

    private void PlaySpottedSound()
    {
        if (spottedSound && audioSource)
            audioSource.PlayOneShot(spottedSound);
    }

    // ===== STATE BEHAVIORS =====
    private void Patrol()
    {
        navMeshAgent.speed = patrolSpeed;

        // Play footsteps while moving
        if (navMeshAgent.velocity.magnitude > 0.1f && Time.time > nextFootstepTime)
        {
            PlayFootstepSound();
            nextFootstepTime = Time.time + 0.5f; // Adjust timing for walk speed
        }

        if (!navMeshAgent.hasPath || navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        {
            if (!isWaiting)
            {
                isWaiting = true;
                Invoke(nameof(SetRandomPatrolPoint), waitTimeAtPatrolPoint);
            }
        }
    }

    private void SetRandomPatrolPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection += transform.position;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
        {
            navMeshAgent.SetDestination(hit.position);
        }

        isWaiting = false;
    }

    private void ChasePlayer()
    {
        if (!playerTransform) return;

        navMeshAgent.speed = chaseSpeed;
        navMeshAgent.SetDestination(playerTransform.position);

        // Play occasional growl
        if (Time.time > nextGrowlTime)
        {
            PlaySpottedSound();
            nextGrowlTime = Time.time + Random.Range(3f, 8f);
        }

        // Play faster footsteps
        if (navMeshAgent.velocity.magnitude > 0.1f && Time.time > nextFootstepTime)
        {
            PlayFootstepSound();
            nextFootstepTime = Time.time + 0.3f; // Adjust for run speed
        }
    }

    private void AttackPlayer()
    {
        if (!playerTransform)
        {
            currentState = EnemyState.Patrol;
            return;
        }

        // Face player
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        direction.y = 0;
        transform.forward = direction;

        // Stop movement while attacking
        navMeshAgent.isStopped = true;

        // Attack cooldown check
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            PerformAttack();
            shouldRetreatAfterAttack = true;
        }

        // Check if player moved out of range
        if (Vector3.Distance(transform.position, playerTransform.position) > attackRange * 1.2f)
        {
            currentState = EnemyState.Chase;
            navMeshAgent.isStopped = false;
        }
    }

    private void PerformAttack()
    {
        animator.SetTrigger(AttackParam);
        PlayAttackSound();
        lastAttackTime = Time.time;
        DealDamage();
    }

    private void DealDamage()
    {
        if (!playerTransform) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        if (distance <= attackRange * 1.2f)
        {
            PlayerHealth playerHealth = playerTransform.GetComponent<PlayerHealth>();
            if (playerHealth)
            {
                playerHealth.TakeDamage(attackDamage);

                // Apply a slight movement penalty to the player
                FirstPersonController playerController = playerTransform.GetComponent<FirstPersonController>();
                if (playerController)
                {
                    StartCoroutine(SlowPlayerTemporarily(playerController));
                }
            }

            // Disable physical interaction between the enemy and the player
            Collider enemyCollider = GetComponent<Collider>();
            Collider playerCollider = playerTransform.GetComponent<Collider>();
            if (enemyCollider && playerCollider)
            {
                Physics.IgnoreCollision(enemyCollider, playerCollider, true); // Disable collision
                StartCoroutine(ReenableCollision(enemyCollider, playerCollider, slowdownDuration)); // Re-enable collision after the slowdown duration
            }
        }
    }

    private IEnumerator SlowPlayerTemporarily(FirstPersonController playerController)
    {
        float originalWalkSpeed = playerController.walkSpeed;
        float originalSprintSpeed = playerController.sprintSpeed;

        // Calculate the new speeds with the slowdown multiplier
        float newWalkSpeed = Mathf.Max(originalWalkSpeed * slowdownMultiplier, minPlayerSpeed);
        float newSprintSpeed = Mathf.Max(originalSprintSpeed * slowdownMultiplier, minPlayerSpeed);

        // Apply the slowdown
        playerController.walkSpeed = newWalkSpeed;
        playerController.sprintSpeed = newSprintSpeed;

        yield return new WaitForSeconds(slowdownDuration);

        // Restore the player's original speed
        playerController.walkSpeed = originalWalkSpeed;
        playerController.sprintSpeed = originalSprintSpeed;
    }

    private IEnumerator ReenableCollision(Collider enemyCollider, Collider playerCollider, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (enemyCollider && playerCollider)
        {
            Physics.IgnoreCollision(enemyCollider, playerCollider, false); // Re-enable collision
        }
    }

    private void CheckForPlayer()
    {
        if (!playerTransform) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        if (distance <= sightRange)
        {
            Vector3 direction = (playerTransform.position - eyePosition.position).normalized;
            float angle = Vector3.Angle(eyePosition.forward, direction);

            if (angle <= fieldOfView / 2)
            {
                if (Physics.Raycast(eyePosition.position, direction, out RaycastHit hit, sightRange))
                {
                    if (hit.transform.CompareTag("Player") && currentState != EnemyState.Chase)
                    {
                        currentState = EnemyState.Chase;
                        PlaySpottedSound();
                    }
                }
            }
        }
    }

    private void CheckAttackRange()
    {
        if (!playerTransform)
        {
            currentState = EnemyState.Patrol;
            return;
        }

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        if (distance <= attackRange)
        {
            currentState = EnemyState.Attack;
            navMeshAgent.isStopped = true;
        }
        else
        {
            navMeshAgent.isStopped = false;
        }
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        PlayHurtSound();

        if (health <= 0)
        {
            OnEnemyDeath();
        }
        else
        {
            // Immediately switch to Chase or Attack state
            if (Vector3.Distance(transform.position, playerTransform.position) <= attackRange)
            {
                currentState = EnemyState.Attack;
            }
            else
            {
                currentState = EnemyState.Chase;
            }

            // Ensure the enemy starts moving toward the player
            if (currentState == EnemyState.Chase && navMeshAgent != null)
            {
                navMeshAgent.isStopped = false;
                navMeshAgent.SetDestination(playerTransform.position);
            }
        }
    }

    public void OnEnemyDeath()
    {
        PlayDeathSound();
        animator.SetTrigger(DieParam);
        
        // Disable movement and this controller
        navMeshAgent.enabled = false;
        enabled = false;

        // Start coroutine to destroy the enemy after the specified delay
        StartCoroutine(DestroyAfterDelay(disappearDelay));
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    // ==== ANIMATION EVENT METHODS ====
    // Call these from animation timelines
    public void AE_Footstep() => PlayFootstepSound();
    public void AE_AttackHit() => DealDamage();
}

