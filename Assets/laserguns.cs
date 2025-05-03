using UnityEngine;
using System.Collections;

public class l : MonoBehaviour
{
    [Header("General Settings")]
    public Camera playerCamera;
    public Transform firePoint;
    public float range = 100f;

    [Header("Bullet Mode Settings")]
    public GameObject bulletPrefab;
    public float bulletForce = 1000f;
    public AudioClip bulletSound;
    public float bulletDamage = 25f;

    [Header("Laser Mode Settings")]
    public LineRenderer laserLine;
    public AudioClip laserSound;
    public float laserDamage = 5f;
    public float holdThreshold = 1f;
    public float laserOverheatTime = 5f;
    public float cooldownTime = 3f;
    public GameObject laserImpactVFX;

    [Header("Recoil Settings")]
    public Transform gunModel;
    public Vector3 recoilKickback = new Vector3(-0.05f, 0f, 0f);
    public float recoilRecoverySpeed = 10f;

    private AudioSource audioSource;
    private Vector3 originalGunPosition;
    private Vector3 currentRecoilOffset = Vector3.zero;

    private float fireHoldTime = 0f;
    private bool isLaserActive = false;
    private bool isOverheated = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (gunModel != null)
            originalGunPosition = gunModel.localPosition;

        if (laserLine != null)
            laserLine.enabled = false;
    }

    void Update()
    {
        if (isOverheated)
            return;

        if (Input.GetButton("Fire1"))
        {
            fireHoldTime += Time.deltaTime;

            if (fireHoldTime >= holdThreshold)
            {
                if (!isLaserActive)
                    ActivateLaser();

                UpdateLaserBeam();

                if (fireHoldTime >= laserOverheatTime)
                    StartCoroutine(Overheat());
            }
        }

        if (Input.GetButtonUp("Fire1"))
        {
            if (fireHoldTime < holdThreshold)
                FireBullet();

            DeactivateLaser();
            fireHoldTime = 0f;
        }

        if (gunModel != null)
        {
            currentRecoilOffset = Vector3.Lerp(currentRecoilOffset, Vector3.zero, Time.deltaTime * recoilRecoverySpeed);
            gunModel.localPosition = originalGunPosition + currentRecoilOffset;
        }
    }

    void FireBullet()
    {
        if (bulletSound != null && audioSource != null)
            audioSource.PlayOneShot(bulletSound);

        if (bulletPrefab != null && firePoint != null)
        {
            Vector3 shootDir = playerCamera.transform.forward;
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(shootDir));
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
                rb.AddForce(shootDir * bulletForce, ForceMode.Impulse);

            // Raycast for hit detection
            Ray ray = new Ray(playerCamera.transform.position, shootDir);
            if (Physics.Raycast(ray, out RaycastHit hit, range))
            {
                Debug.Log("🔫 Bullet hit: " + hit.collider.name);
                TryDealDamage(hit, bulletDamage);
            }

            Destroy(bullet, 2f);
        }

        currentRecoilOffset += recoilKickback;
    }

    void ActivateLaser()
    {
        isLaserActive = true;

        if (laserLine != null)
            laserLine.enabled = true;

        if (laserSound != null && audioSource != null)
        {
            audioSource.clip = laserSound;
            audioSource.loop = true;
            audioSource.Play();
        }

        currentRecoilOffset += recoilKickback;
    }

    void UpdateLaserBeam()
    {
        if (laserLine == null || firePoint == null || playerCamera == null)
            return;

        Vector3 start = firePoint.position;
        Vector3 direction = playerCamera.transform.forward;
        laserLine.SetPosition(0, start);

        if (Physics.Raycast(playerCamera.transform.position, direction, out RaycastHit hit, range))
        {
            Vector3 end = hit.point;
            laserLine.SetPosition(1, end);

            Debug.Log("🔦 Laser hit: " + hit.collider.name);
            TryDealDamage(hit, laserDamage * Time.deltaTime);

            if (laserImpactVFX != null)
                Instantiate(laserImpactVFX, hit.point, Quaternion.LookRotation(hit.normal));
        }
        else
        {
            Vector3 end = start + direction * range;
            laserLine.SetPosition(1, end);
        }
    }

    void DeactivateLaser()
    {
        if (!isLaserActive) return;

        isLaserActive = false;

        if (laserLine != null)
            laserLine.enabled = false;

        if (audioSource != null && audioSource.clip == laserSound)
            audioSource.Stop();
    }

    IEnumerator Overheat()
    {
        Debug.Log("🔥 Laser Overheated!");
        isOverheated = true;
        DeactivateLaser();
        yield return new WaitForSeconds(cooldownTime);
        isOverheated = false;
        fireHoldTime = 0f;
        Debug.Log("✅ Laser Cooled Down!");
    }

    void TryDealDamage(RaycastHit hit, float damage)
    {
        EnemyHealth health = hit.collider.GetComponentInParent<EnemyHealth>();
        if (health != null)
        {
            health.TakeDamage(damage);
        }
        else
        {
            Debug.Log("❌ No EnemyHealth found on " + hit.collider.name);
        }
    }
}
