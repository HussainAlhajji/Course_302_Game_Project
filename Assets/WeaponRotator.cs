using System.Collections;
using UnityEngine;
using TMPro;

public class WeaponRotator : MonoBehaviour
{
    [Header("Weapon Sets")]
    public Transform[] weaponObjects;
    public Transform[] ultimateWeapons;

    [Header("UI")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI ultimateDurationText;
    public TextMeshProUGUI ultimateReadyText;

    [Header("Player Stats")]
    public PlayerStats playerStats;

    [Header("Ultimate Boost Settings")]
    public float healthBoost = 50f; // Temporary health boost during ultimate
    public float speedMultiplier = 1.5f; // Speed multiplier during ultimate
    public float ultimateDuration = 30f; // Duration of the ultimate

    private Transform currentWeapon;
    private bool usingUltimate = false;
    private Coroutine switchRoutine;
    private PlayerHealth playerHealth;
    private FirstPersonController playerController;

    void Start()
    {
        switchRoutine = StartCoroutine(SwitchWeaponTimer());
        UpdateUltimateReadyText();

        // Get references to PlayerHealth and FirstPersonController
        playerHealth = FindObjectOfType<PlayerHealth>();
        playerController = FindObjectOfType<FirstPersonController>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q) && playerStats != null && playerStats.CanUseUltimate() && !usingUltimate)
        {
            ActivateUltimate();
        }

        UpdateUltimateReadyText();
    }

    IEnumerator SwitchWeaponTimer()
    {
        while (true)
        {
            float switchCountdown = Random.Range(7f, 10f);

            while (switchCountdown > 0f)
            {
                if (timerText != null)
                {
                    timerText.text = $"{Mathf.Ceil(switchCountdown)}";
                }

                switchCountdown -= Time.deltaTime;
                yield return null;
            }

            if (usingUltimate)
                SwitchToRandomUltimateWeapon();
            else
                SwitchToRandomWeapon();

            yield return null;
        }
    }

    IEnumerator UltimateDurationCoroutine()
    {
        float timeLeft = ultimateDuration;

        while (timeLeft > 0f)
        {
            if (ultimateDurationText != null)
                ultimateDurationText.text = $"ULTIMATE ENDS IN: {Mathf.Ceil(timeLeft)}s";

            timeLeft -= Time.deltaTime;
            yield return null;
        }

        EndUltimate();
    }

    void ActivateUltimate()
    {
        usingUltimate = true;

        // Hide the "Press Q" text
        if (playerStats != null)
        {
            playerStats.HideUltimateReadyText();
        }

        // Apply health and speed boosts
        if (playerHealth != null)
        {
            playerHealth.currentHealth = Mathf.Min(playerHealth.currentHealth + healthBoost, playerHealth.maxHealth);
            playerHealth.UpdateHealthUI();
        }

        if (playerController != null)
        {
            playerController.walkSpeed *= speedMultiplier;
            playerController.sprintSpeed *= speedMultiplier;
        }

        // Switch to ultimate weapons
        SwitchToRandomUltimateWeapon();

        // Start the ultimate duration timer
        StartCoroutine(UltimateDurationCoroutine());
    }

    void EndUltimate()
    {
        usingUltimate = false;

        if (ultimateDurationText != null)
            ultimateDurationText.text = "";

        if (playerStats != null)
            playerStats.ResetPoints();

        // Reset health and speed boosts
        if (playerController != null)
        {
            playerController.walkSpeed /= speedMultiplier;
            playerController.sprintSpeed /= speedMultiplier;
        }

        Debug.Log("[WeaponRotator] Ultimate ended. Returning to regular weapons.");
        SwitchToRandomWeapon();
    }

    void SwitchToRandomWeapon()
    {
        if (weaponObjects.Length <= 1) return;

        DisableAllWeapons();

        int currentIndex = System.Array.IndexOf(weaponObjects, currentWeapon);
        int newIndex;

        do
        {
            newIndex = Random.Range(0, weaponObjects.Length);
        } while (newIndex == currentIndex && weaponObjects.Length > 1);

        currentWeapon = weaponObjects[newIndex];
        currentWeapon.gameObject.SetActive(true);

        Debug.Log("[WeaponRotator] Switched to: " + currentWeapon.name);
    }

    void SwitchToRandomUltimateWeapon()
    {
        if (ultimateWeapons.Length == 0) return;

        DisableAllWeapons();

        int currentIndex = System.Array.IndexOf(ultimateWeapons, currentWeapon);
        int newIndex;

        do
        {
            newIndex = Random.Range(0, ultimateWeapons.Length);
        } while (newIndex == currentIndex && ultimateWeapons.Length > 1);

        currentWeapon = ultimateWeapons[newIndex];
        currentWeapon.gameObject.SetActive(true);

        Debug.Log("[WeaponRotator] Switched Ultimate to: " + currentWeapon.name);
    }

    void DisableAllWeapons()
    {
        foreach (var w in weaponObjects)
            w.gameObject.SetActive(false);

        foreach (var u in ultimateWeapons)
            u.gameObject.SetActive(false);
    }

    void UpdateUltimateReadyText()
    {
        if (ultimateReadyText != null)
        {
            ultimateReadyText.gameObject.SetActive(playerStats != null && playerStats.CanUseUltimate());
        }
    }
}
