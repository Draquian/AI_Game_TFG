using UnityEngine;

/// <summary>
/// BurnEffect applies damage over time to the GameObject on which it is attached.
/// It deals a total amount of damage (totalBurnDamage) evenly over a given duration.
/// </summary>
public class BurnEffect_Copilot : MonoBehaviour
{
    private float totalBurnDamage;
    private float duration;
    private float damagePerSecond;
    private float timeElapsed = 0f;

    /// <summary>
    /// Initializes and starts the burn effect.
    /// </summary>
    /// <param name="totalDamage">The total damage that will be dealt over the duration.</param>
    /// <param name="burnDuration">Duration over which the damage is applied.</param>
    public void StartBurn(float totalDamage, float burnDuration)
    {
        totalBurnDamage = totalDamage;
        duration = burnDuration;
        damagePerSecond = totalBurnDamage / duration;
        timeElapsed = 0f;
    }

    /// <summary>
    /// Resets the burn effect using new parameters.
    /// </summary>
    public void ResetBurn(float totalDamage, float burnDuration)
    {
        totalBurnDamage = totalDamage;
        duration = burnDuration;
        damagePerSecond = totalBurnDamage / duration;
        timeElapsed = 0f;
    }

    void Update()
    {
        // Apply damage over time until the duration expires.
        float delta = Time.deltaTime;
        timeElapsed += delta;
        if (timeElapsed < duration)
        {
            float damageThisFrame = damagePerSecond * delta;
            // Call TakeDamage on this GameObject.
            // Assumes that the target (enemy) has a public method TakeDamage(float).
            SendMessage("TakeDamage", damageThisFrame, SendMessageOptions.DontRequireReceiver);
        }
        else
        {
            // Burn effect complete; remove this component.
            Destroy(this);
        }
    }
}
