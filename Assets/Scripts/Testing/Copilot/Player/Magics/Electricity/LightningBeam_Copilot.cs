using UnityEngine;

public class LightningBeam_Copilot : MonoBehaviour
{
    [Header("Lightning Beam Settings")]
    public float damage = 20f;          // Damage to deal on impact.
    public float slowDuration = 2f;     // Duration to slow the target (in seconds).
    public float speed = 50f;           // Speed at which the beam travels.
    public float lifetime = 5f;         // Lifetime of the beam if it doesn't hit anything.

    private Rigidbody rb;

    // Optionally, this method can be used to initialize beam parameters
    // from the ElectricityMagic script if needed.
    public void Initialize(float _damage, float _slowDuration)
    {
        damage = _damage;
        slowDuration = _slowDuration;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.useGravity = false;        // Ensure gravity doesn't affect the beam.
        rb.isKinematic = false;       // Allows velocity control.
    }

    void Start()
    {
        // Set an initial velocity so the beam travels straight forward.
        rb.velocity = transform.forward * speed;
        // Auto-destroy the beam after 'lifetime' seconds.
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if we hit an enemy or boss.
        if (other.CompareTag("Enemy") || other.CompareTag("Boss"))
        {
            // Deal damage. The target must handle a TakeDamage(float) method.
            other.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
            // Apply a slow effect for 2 seconds.
            other.SendMessage("ApplySlow", slowDuration, SendMessageOptions.DontRequireReceiver);
            Debug.Log("LightningBeam: Hit " + other.name + " dealing " + damage + " damage and applying slow for " + slowDuration + " seconds.");
        }
        // Destroy the beam after it contacts something.

        if (!other.CompareTag("Player"))  Destroy(gameObject);
    }
}
