using UnityEngine;

public class WaterBullet_Copilot : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float damage = 10f;       // Damage to apply on hit.
    public float lifetime = 5f;      // Time in seconds before the bullet auto-destroys.

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            // Automatically add a Rigidbody if one does not exist.
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.useGravity = false;
        // Optionally, you might want to set rb.isKinematic = false if you control its velocity.
    }

    void Start()
    {
        // Auto-destroy the bullet after its lifetime expires.
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if the bullet hit an enemy or boss.
        if (other.CompareTag("Enemy") || other.CompareTag("Boss"))
        {
            // Deal damage by triggering the object's TakeDamage method (if implemented).
            other.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
            Destroy(gameObject);
        }
        else if (!other.isTrigger)
        {
            // If it hits any other solid object (that isn't a trigger), destroy the bullet.
            if (!other.CompareTag("Player"))
                Destroy(gameObject);
        }
    }
}
