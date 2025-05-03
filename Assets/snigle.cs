using UnityEngine;
using System.Collections;

public class snigleShotgun : MonoBehaviour
{
    [Header("Gun Settings")]
    public Camera playerCamera;
    public Transform firePoint;
    public GameObject bulletPrefab;
    public GameObject impactEffect;
    public float fireRate = 1f;
    public float bulletSpeed = 80f;
    public float range = 50f;

    [Header("Shotgun Settings")]
    public int pelletsPerShot = 8;
    public float spreadAngle = 8f;

    [Header("Damage Settings")]
    public float maxDamage = 20f;
    public float minDamage = 5f;

    [Header("Sound & Effects")]
    public AudioClip fireSound;
    public GameObject muzzleFlash;

    [Header("Recoil - Rotation")]
    public Transform gunModel;
    public Vector3 recoilRotation = new Vector3(-10f, 3f, 2f);
    public float recoilReturnSpeed = 10f;
    public float recoilSnappiness = 10f;

    private AudioSource audioSource;
    private Vector3 currentRecoilRot;
    private Vector3 targetRecoilRot;
    private float nextTimeToFire = 0f;

    void Start()
    {
        // Auto-assign AudioSource if not set
        audioSource = GetComponent<AudioSource>();
        if (!audioSource)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;
        }

        if (gunModel != null)
            gunModel.localRotation = Quaternion.identity;

        if (muzzleFlash != null)
            muzzleFlash.SetActive(false);
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire1") && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + fireRate;
            Shoot();
        }

        // Recoil recovery
        targetRecoilRot = Vector3.Lerp(targetRecoilRot, Vector3.zero, recoilReturnSpeed * Time.deltaTime);
        currentRecoilRot = Vector3.Slerp(currentRecoilRot, targetRecoilRot, recoilSnappiness * Time.deltaTime);
        if (gunModel != null)
            gunModel.localRotation = Quaternion.Euler(currentRecoilRot);
    }

    void Shoot()
    {
        // 🔊 Play fire sound
        if (fireSound != null && audioSource != null)
        {
            Debug.Log("🔊 Playing shotgun sound");
            audioSource.PlayOneShot(fireSound);
        }

        // 🔥 Muzzle flash
        if (muzzleFlash != null)
        {
            muzzleFlash.SetActive(true);
            StartCoroutine(HideMuzzleFlash());
        }

        // 🔫 Fire pellets
        for (int i = 0; i < pelletsPerShot; i++)
        {
            Vector3 direction = playerCamera.transform.forward;
            direction += playerCamera.transform.right * Random.Range(-spreadAngle, spreadAngle) * 0.01f;
            direction += playerCamera.transform.up * Random.Range(-spreadAngle, spreadAngle) * 0.01f;
            direction.Normalize();

            Ray ray = new Ray(playerCamera.transform.position, direction);
            if (Physics.Raycast(ray, out RaycastHit hit, range))
            {
                // 💥 Impact effect
                if (impactEffect != null)
                    Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));

                // 📉 Damage falloff
                float distance = Vector3.Distance(playerCamera.transform.position, hit.point);
                float t = Mathf.InverseLerp(0f, range, distance);
                float damage = Mathf.Lerp(maxDamage, minDamage, t);

                // ☠️ Deal damage
                EnemyHealth enemy = hit.collider.GetComponentInParent<EnemyHealth>();
                if (enemy != null)
                    enemy.TakeDamage(damage);
            }

            // 💨 Visual pellet (optional)
            if (bulletPrefab != null)
            {
                GameObject pellet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(direction));
                Rigidbody rb = pellet.GetComponent<Rigidbody>();
                if (rb != null)
                    rb.linearVelocity = direction * bulletSpeed;

                Destroy(pellet, 2f);
            }
        }

        // 💥 Apply recoil
        targetRecoilRot += new Vector3(
            Random.Range(recoilRotation.x * 0.8f, recoilRotation.x),
            Random.Range(-recoilRotation.y, recoilRotation.y),
            Random.Range(-recoilRotation.z, recoilRotation.z)
        );
    }

    IEnumerator HideMuzzleFlash()
    {
        yield return new WaitForSeconds(0.05f);
        if (muzzleFlash != null)
            muzzleFlash.SetActive(false);
    }
}
