using UnityEngine;
using System.Collections;
using UnityEngine.AI; // Required for NavMesh

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;
    public GameObject damageTextPrefab;
    public Transform textSpawnPoint;

    private EnemyController enemyController;

    public AudioClip damageSound;
    private AudioSource audioSource;

    [Header("Points")]
    public int pointsOnKill = 10;

    void Start()
    {
        currentHealth = maxHealth;
        enemyController = GetComponent<EnemyController>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void TakeDamage(float amount)
    {
        if (currentHealth <= 0f) return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(0, currentHealth);

        Debug.Log($"Enemy took {amount} damage. Current health: {currentHealth}");

        if (damageSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(damageSound);
        }

        if (damageTextPrefab != null)
        {
            Vector3 spawnPosition = textSpawnPoint != null
                ? textSpawnPoint.position
                : transform.position + Vector3.up * 2f;

            GameObject textGO = Instantiate(damageTextPrefab, spawnPosition, Quaternion.identity);
            DamageText dt = textGO.GetComponent<DamageText>();
            if (dt != null)
            {
                dt.SetText(amount.ToString("F0"));
            }
        }

        if (currentHealth <= 0f)
        {
            PlayerStats playerStats = FindObjectOfType<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.AddPoints(pointsOnKill);
            }

            if (enemyController != null)
                enemyController.OnEnemyDeath();
        }
        else
        {
            if (enemyController != null && enemyController.currentState == EnemyState.Patrol)
                enemyController.currentState = EnemyState.Chase;
        }
    }

    // ✅ Called by the rifle if knockback is enabled
   public void ApplyKnockback(Vector3 direction, float force)
{
    StartCoroutine(KnockbackRoutine(direction, force));
}

private IEnumerator KnockbackRoutine(Vector3 direction, float force)
{
    float duration = 0.1f; // Adjust for smoother knockback
    float elapsed = 0f;
    Vector3 startPos = transform.position;
    Vector3 targetPos = startPos + direction.normalized * force;

    while (elapsed < duration)
    {
        transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
        elapsed += Time.deltaTime;
        yield return null;
    }

    transform.position = targetPos;
}

    public void ApplyNavMeshKnockback(Vector3 direction, float force)
    {
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogWarning("No NavMeshAgent found on enemy. Cannot apply NavMesh knockback.");
            return;
        }

        Vector3 knockbackTarget = transform.position + direction.normalized * force;

        if (NavMesh.SamplePosition(knockbackTarget, out NavMeshHit hit, force, NavMesh.AllAreas))
        {
            agent.Warp(hit.position); // Move the enemy to the valid NavMesh position
        }
        else
        {
            Debug.LogWarning("Knockback target is outside of NavMesh bounds.");
        }
    }
}
