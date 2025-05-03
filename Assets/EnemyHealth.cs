using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;

    private EnemyController enemyController;

    public AudioClip damageSound; // Sound to play when damaged
    private AudioSource audioSource; // AudioSource to play the sound

    [Header("Points")]
    public int pointsOnKill = 10; // Points awarded for killing this enemy

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

        // Play damage sound
        if (damageSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(damageSound);
        }

        if (currentHealth <= 0f)
        {
            // Award points to the player
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
}
