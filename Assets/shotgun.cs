using UnityEngine;
using System.Collections;
 
public class Shotgun : MonoBehaviour
{
    private bool canShoot = true;
 
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
    private AudioSource audioSource;
    public GameObject[] muzzleFlashes;
 
    [Header("Recoil")]
    public Transform gunModel;
    public Vector3 recoilRotation = new Vector3(-10f, 3f, 2f);
    public float recoilReturnSpeed = 10f;
    public float recoilSnappiness = 10f;
 
    private Vector3 currentRecoilRot;
    private Vector3 targetRecoilRot;
 
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (!audioSource)
            audioSource = gameObject.AddComponent<AudioSource>();
 
        if (gunModel != null)
            gunModel.localRotation = Quaternion.identity;
 
        foreach (GameObject flash in muzzleFlashes)
        {
            if (flash != null)
                flash.SetActive(false);
        }
    }
 
    void Update()
    {
        targetRecoilRot = Vector3.Lerp(targetRecoilRot, Vector3.zero, recoilReturnSpeed * Time.deltaTime);
        currentRecoilRot = Vector3.Slerp(currentRecoilRot, targetRecoilRot, recoilSnappiness * Time.deltaTime);
        if (gunModel != null)
            gunModel.localRotation = Quaternion.Euler(currentRecoilRot);
    }
 
    public void ForceFire()
    {
        if (canShoot)
        {
            Shoot();
            StartCoroutine(IndividualCooldown());
        }
    }
 
    private IEnumerator IndividualCooldown()
    {
        canShoot = false;
        yield return new WaitForSeconds(fireRate);
        canShoot = true;
    }
 
    public void Shoot()
    {
        // 🔊 Fire sound
        if (fireSound != null && audioSource != null)
            audioSource.PlayOneShot(fireSound);
 
        // 🔥 Muzzle flashes
        foreach (GameObject flash in muzzleFlashes)
        {
            if (flash != null)
                flash.SetActive(true);
        }
        StartCoroutine(HideAllMuzzleFlashes());
 
        // 🔫 Pellets
        for (int i = 0; i < pelletsPerShot; i++)
        {
            // Direction from camera center
            Vector3 dir = playerCamera.transform.forward;
            dir += playerCamera.transform.right * Random.Range(-spreadAngle, spreadAngle) * 0.01f;
            dir += playerCamera.transform.up * Random.Range(-spreadAngle, spreadAngle) * 0.01f;
            dir.Normalize();
 
            // Raycast from camera center
            Ray ray = new Ray(playerCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0)), dir);
            if (Physics.Raycast(ray, out RaycastHit hit, range))
            {
                if (impactEffect != null)
                    Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
 
                float distance = Vector3.Distance(playerCamera.transform.position, hit.point);
                float t = Mathf.InverseLerp(0f, range, distance);
                float damage = Mathf.Lerp(maxDamage, minDamage, t);
 
                EnemyHealth enemy = hit.collider.GetComponentInParent<EnemyHealth>();
                if (enemy != null)
                    enemy.TakeDamage(damage);
            }
 
            // 🔄 Visual bullet
            if (bulletPrefab != null)
            {
                GameObject pellet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(dir));
                Rigidbody rb = pellet.GetComponent<Rigidbody>();
                if (rb != null)
                    rb.linearVelocity = dir * bulletSpeed;
                Destroy(pellet, 2f);
            }
        }
 
        // 🔁 Recoil
        targetRecoilRot += new Vector3(
            Random.Range(recoilRotation.x * 0.8f, recoilRotation.x),
            Random.Range(-recoilRotation.y, recoilRotation.y),
            Random.Range(-recoilRotation.z, recoilRotation.z)
        );
    }
 
    IEnumerator HideAllMuzzleFlashes()
    {
        yield return new WaitForSeconds(0.05f);
        foreach (GameObject flash in muzzleFlashes)
        {
            if (flash != null)
                flash.SetActive(false);
        }
    }
}
 