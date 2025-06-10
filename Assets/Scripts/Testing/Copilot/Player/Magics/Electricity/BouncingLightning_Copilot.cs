using UnityEngine;

public class BouncingLightning_Copilot : MonoBehaviour
{
    [Header("Bouncing Lightning Settings")]
    public int bounceCount = 2;    // Number of bounces allowed.
    public float speed = 30f;      // Speed at which the projectile moves.
    public float damage = 20f;     // (Optional) Damage the projectile can deal on impact.
                                   // Minimal velocity threshold to avoid "sticking".
    public float minVelocityThreshold = 5f;

    // Track the last frame when a bounce was processed.
    private int lastBounceFrame = -1;

    private Rigidbody rb;

    void Awake()
    {
        // Ensure the projectile has a Rigidbody.
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.useGravity = false;
    }

    void Start()
    {
        // If no external launch is provided, you can set an initial velocity.
        // For example, push forward relative to your object's forward:
        if (rb.velocity == Vector3.zero)
        {
            rb.velocity = transform.forward * speed;
        }
    }

    /// <summary>
    /// Call this to launch the projectile with a specific direction.
    /// </summary>
    public void Launch(Vector3 direction)
    {
        rb.velocity = direction.normalized * speed;
        transform.rotation = Quaternion.LookRotation(direction);
    }

    /// <summary>
    /// When colliding with any non-trigger collider, reflect the lightning's velocity.
    /// Decrement bounceCount; if no bounces remain, destroy the projectile.
    /// </summary>
   void OnCollisionEnter(Collision collision)
    {
        // Check if this collision occurs on the same frame as the last processed bounce.
        if (Time.frameCount == lastBounceFrame)
        {
            // Already processed a bounce on this frame: ignore this collision.
            return;
        }

        // Mark this frame as processed.
        lastBounceFrame = Time.frameCount;

        // Process the bounce: decrement bounce count.
        bounceCount--;

        if (bounceCount > 0)
        {
            // Calculate reflection using the first contact's normal.
            /*Vector3 incomingVelocity = rb.velocity;
            Vector3 contactNormal = collision.contacts[0].normal;
            Vector3 reflectedVelocity = Vector3.Reflect(incomingVelocity, contactNormal);

            // If the reflected velocity is too low, nudge it using the contact normal.
            if (reflectedVelocity.magnitude < minVelocityThreshold)
            {
                reflectedVelocity = contactNormal * speed;
            }

            rb.velocity = reflectedVelocity.normalized * speed;
            transform.rotation = Quaternion.LookRotation(rb.velocity);*/

            Debug.Log("BouncingLightning: Bounced! Remaining bounces: " + bounceCount);
        }
        else
        {
            // No bounces remain—destroy the projectile.
            Destroy(gameObject);
        }
    }
}
