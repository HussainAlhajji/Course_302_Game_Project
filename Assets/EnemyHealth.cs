using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;
    public GameObject damageTextPrefab; // Prefab with DamageText script
    public Transform textSpawnPoint; 


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

    // Spawn floating damage text
     if (damageTextPrefab != null)
    {
        Vector3 spawnPosition = textSpawnPoint != null
            ? textSpawnPoint.position
            : transform.position + Vector3.up * 2f;

        GameObject textGO = Instantiate(damageTextPrefab, spawnPosition, Quaternion.identity);
        DamageText dt = textGO.GetComponent<DamageText>();
        if (dt != null)
            dt.SetText(amount.ToString("F0")); // Display the damage amount
    }
    if (currentHealth <= 0f)
    {
        // Award points
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
        // Change enemy state if needed
        if (enemyController != null && enemyController.currentState == EnemyState.Patrol)
            enemyController.currentState = EnemyState.Chase;
    }
    Debug.Log("Enemy took damage: " + amount);
Debug.Log("Spawn point: " + (textSpawnPoint != null ? textSpawnPoint.name : "none"));
Debug.Log("Instantiating damageTextPrefab...");
Debug.Log("Damage text prefab: " + damageTextPrefab.name);
}

}
