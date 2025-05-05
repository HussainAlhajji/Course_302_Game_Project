using UnityEngine;
using System.Collections;

public class AssaultRifle : MonoBehaviour
{
    [Header("Gun Settings")]
    public Camera playerCamera;
    public Transform firePoint;
    public GameObject bulletPrefab;
    public GameObject impactEffect;
    public float fireRate = 0.1f;
    public float bulletSpeed = 100f;
    public float range = 100f;

    [Header("Damage Settings")]
    public float baseDamage = 25f;
    public float minDamage = 10f;

    [Header("Special Ability")]
    public bool enableKnockback = true; // ✅ Toggle knockback per weapon
    public float knockbackForce = 5f;

    [Header("Sound & Effects")]
    public AudioClip fireSound;
    private AudioSource audioSource;
    public GameObject muzzleFlashSprite;

    [Header("Recoil")]
    public Transform gunModel;
    public Vector3 recoilKickback = new Vector3(-0.05f, 0f, 0f);
    public float recoilReturnSpeed = 10f;

    [Header("Aiming")]
    public Transform aimPosition;
    public float aimSpeed = 10f;
    public float aimFOV = 40f;
    private Vector3 hipPosition;
    private bool isAiming = false;
    private float defaultFOV;
    private Vector3 currentRecoilOffset = Vector3.zero;
    private float nextTimeToFire = 0f;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (gunModel != null)
            hipPosition = gunModel.localPosition;

        if (playerCamera != null)
            defaultFOV = playerCamera.fieldOfView;
    }

    void Update()
    {
        // Toggle aim
        if (Input.GetKeyDown(KeyCode.M))
        {
            isAiming = !isAiming;
        }

        // Aim movement
        if (gunModel != null)
        {
            gunModel.localPosition = Vector3.Lerp(gunModel.localPosition, hipPosition + currentRecoilOffset, Time.deltaTime * aimSpeed);
        }

        // FOV zoom
        if (playerCamera != null)
        {
            float targetFOV = isAiming ? aimFOV : defaultFOV;
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * 8f);
        }

        // Fire input
        if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + fireRate;
            Shoot();
        }

        // Smooth recoil recovery
        currentRecoilOffset = Vector3.Lerp(currentRecoilOffset, Vector3.zero, Time.deltaTime * recoilReturnSpeed);
    }

    void Shoot()
    {
        // 🔥 Muzzle flash
        if (muzzleFlashSprite != null)
        {
            muzzleFlashSprite.SetActive(true);
            StartCoroutine(HideMuzzleFlash());
        }

        // 🔊 Fire sound
        if (fireSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(fireSound);
        }

        // 🎯 Raycast to check hit
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            // 💥 Spawn impact effect
            if (impactEffect != null)
            {
                Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            }

            // 📉 Calculate distance-based damage
            float distance = Vector3.Distance(playerCamera.transform.position, hit.point);
            float t = Mathf.InverseLerp(0f, range, distance);
            float damage = Mathf.Lerp(baseDamage, minDamage, t);

            // 🧠 Deal damage to enemy and apply knockback if enabled
            EnemyHealth enemy = hit.collider.GetComponentInParent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);

                if (enableKnockback)
                {
                    Vector3 knockbackDir = hit.point - firePoint.position;
                    enemy.ApplyKnockback(knockbackDir, knockbackForce);
                }
            }
            else
            {
                Debug.LogWarning("No EnemyHealth script found on " + hit.collider.name);
            }
        }

        // 💨 Visual bullet
        if (bulletPrefab != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
                rb.linearVelocity = firePoint.forward * bulletSpeed;
            Destroy(bullet, 2f);
        }

        // 💥 Recoil
        currentRecoilOffset += recoilKickback;
    }

    IEnumerator HideMuzzleFlash()
    {
        yield return new WaitForSeconds(0.05f);
        if (muzzleFlashSprite != null)
            muzzleFlashSprite.SetActive(false);
    }
}
