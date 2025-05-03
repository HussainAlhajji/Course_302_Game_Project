using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class Grenade : MonoBehaviour
{
    [Header("Explosion Settings")]
    public float blastRadius = 5f;
    public float explosionForce = 700f;
    public float damage = 100f;
    public GameObject explosionEffect;
    public AudioClip explosionSound;

    private bool hasExploded = false;

    void OnCollisionEnter(Collision collision)
    {

    Debug.Log("Grenade hit: " + collision.collider.name);

    

        if (hasExploded) return;

        hasExploded = true;

        if (explosionEffect != null)
            Instantiate(explosionEffect, transform.position, Quaternion.identity);

        if (explosionSound != null)
            AudioSource.PlayClipAtPoint(explosionSound, transform.position);

        Collider[] colliders = Physics.OverlapSphere(transform.position, blastRadius);
        foreach (Collider nearbyObject in colliders)
        {
            Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
            if (rb != null)
                rb.AddExplosionForce(explosionForce, transform.position, blastRadius);

            EnemyHealth enemy = nearbyObject.GetComponent<EnemyHealth>();
            if (enemy != null)
                enemy.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}
