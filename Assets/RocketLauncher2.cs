using UnityEngine;
using System.Collections.Generic;

public class ROCKETLauncher : MonoBehaviour
{
    [Header("Launcher Setup")]
    public Transform firePoint;

    [Header("Ammo Settings")]
    public int maxAmmo = 10;
    private int currentAmmo;

    public Transform rocketHolder;
    public GameObject rocketPrefab;
    public List<GameObject> rocketVisualPrefabs;
    public GameObject loadedRocket;

    [Header("Rocket Settings")]
    public float launchForce = 25f;

    [Header("Reload Settings")]
    public float reloadTime = 2f;
    private bool isReloading = false;

    [Header("Audio Settings")]
    public AudioClip launchSound;
    public AudioClip reloadSound;
    private AudioSource audioSource;

    [Header("Recoil Settings")]
    public Transform recoilObject;
    public Vector3 recoilKickback = new Vector3(0f, 0f, -0.2f);
    public float recoilReturnSpeed = 4f;

    private Vector3 recoilOriginalPos;
    private Vector3 recoilCurrentOffset = Vector3.zero;

    private int currentRocketIndex = 0;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (recoilObject != null)
            recoilOriginalPos = recoilObject.localPosition;

        currentAmmo = maxAmmo;
        ReloadRocket();
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire1") && !isReloading && loadedRocket != null)
        {
            FireLoadedRocket();
        }

        if (recoilObject != null)
        {
            recoilCurrentOffset = Vector3.Lerp(recoilCurrentOffset, Vector3.zero, Time.deltaTime * recoilReturnSpeed);
            recoilObject.localPosition = recoilOriginalPos + recoilCurrentOffset;
        }
    }

    void FireLoadedRocket()
    {
        if (currentAmmo <= 0)
        {
            Debug.Log("Out of ammo!");
            return;
        }

        isReloading = true;

        loadedRocket.transform.parent = null;

        Rigidbody rb = loadedRocket.GetComponent<Rigidbody>();
        if (rb == null) rb = loadedRocket.AddComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        Collider col = loadedRocket.GetComponent<Collider>();
        if (col == null) col = loadedRocket.AddComponent<SphereCollider>();
        col.enabled = true;

        // Arc force
        Vector3 shootForce = firePoint.forward * launchForce;
rb.AddForce(shootForce, ForceMode.Impulse);


        if (launchSound != null && audioSource != null)
            audioSource.PlayOneShot(launchSound);

        recoilCurrentOffset += recoilKickback;

        loadedRocket = null;
        currentAmmo--;

        Invoke(nameof(FinishReload), reloadTime);
    }

    void FinishReload()
    {
        isReloading = false;

        if (reloadSound != null && audioSource != null)
            audioSource.PlayOneShot(reloadSound);

        ReloadRocket();
    }

    void ReloadRocket()
    {
        if (rocketVisualPrefabs.Count == 0)
        {
            Debug.LogWarning("No rockets left to load!");
            return;
        }

        if (currentRocketIndex >= rocketVisualPrefabs.Count)
            currentRocketIndex = 0;

        GameObject rocketVisualPrefab = rocketVisualPrefabs[currentRocketIndex];

        if (rocketVisualPrefab != null && rocketHolder != null)
        {
            GameObject newRocket = Instantiate(rocketVisualPrefab, rocketHolder.position, rocketHolder.rotation, rocketHolder);
            loadedRocket = newRocket;

            // Disable physics before firing
            Rigidbody rb = newRocket.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            Collider col = newRocket.GetComponent<Collider>();
            if (col != null) col.enabled = false;
        }

        currentRocketIndex++;
    }
}
