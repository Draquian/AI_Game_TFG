using UnityEngine;
using System.Collections;

public class TheArcaneWeaver_Copilot : EnemyBase_Copilot
{
    public enum BossPhase { Phase1, Phase2, Phase3 }
    private BossPhase currentPhase;

    // New: Reference to the boss room (from MazeGenerator).

    private MazeGenerator_Copilot.Room bossRoom;
    public float minTeleportDistanceFromPlayer = 10f;

    // Movement settings.
    public float hoverSpeed = 5f;  // Can be used for non-teleport movement.

    // Attack settings.
    // Arcane projectile settings.
    public GameObject arcaneProjectilePrefab; // If not assigned, one will be created at runtime.
    public float projectileSpeedPhase1 = 15f;
    public float projectileSpeedPhase3 = 25f;

    void Start()
    {
        // Set boss statistics.
        enemyName = "The Arcane Weaver";
        description = "A powerful mage who has become one with arcane energies, capable of manipulating raw magic and summoning elemental guardians. This encounter tests the player's ability to dodge a barrage of magical attacks, manage multiple enemies, and recognize and avoid a devastating channeled beam. A robed figure levitating in the air, surrounded by swirling arcane symbols that shift in color and intensity with each phase. During Phase 2, it summons translucent Stone Golems made of crackling blue energy.";
        maxHP = 4000f;
        currentHP = maxHP;
        physicalDefense = 10f;
        magicalDefense = 40f;
        attackRange = 10f;
        movementSpeed = 5f; // This is the base movement speed.
        physicalAttackDamage = 15f;
        magicalDamage = 40f;
        lootDropChance = 1f;  // 100%

        // Retrieve the boss room from MazeGenerator.
        MazeGenerator_Copilot mg = FindObjectOfType<MazeGenerator_Copilot>();
        if (mg != null)
        {
            bossRoom = mg.bossRoom; // Assumes MazeGenerator exposes a public bossRoom of type Room.
        }
        else
        {
            Debug.LogWarning("MazeGenerator not found; boss will not teleport properly.");
        }

        // Begin movement and attack cycles.
        StartCoroutine(BossMovement());
        StartCoroutine(BossAttackLoop());
    }

    void Update()
    {
        UpdatePhase();
    }

    // Update the boss phase based on current HP.
    void UpdatePhase()
    {
        float hpPercentage = currentHP / maxHP;
        if (hpPercentage > 0.76f)
        {
            currentPhase = BossPhase.Phase1;
        }
        else if (hpPercentage > 0.25f)
        {
            currentPhase = BossPhase.Phase2;
        }
        else
        {
            currentPhase = BossPhase.Phase3;
        }
    }

    // Boss movement: hovers unpredictably and teleports occasionally within the boss room.
    IEnumerator BossMovement()
    {
        while (currentHP > 0)
        {
            // With 30% probability, attempt to teleport.
            if (bossRoom != null && Random.value < 0.3f)
            {
                // Get a candidate teleport position from inside the boss room.
                Vector3 newPos = GetRandomTeleportPosition();
                // Make sure the teleport destination is not too close to the player.
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                int maxAttempts = 10, attempts = 0;
                while (player != null && Vector3.Distance(newPos, player.transform.position) < minTeleportDistanceFromPlayer && attempts < maxAttempts)
                {
                    newPos = GetRandomTeleportPosition();
                    attempts++;
                }
                transform.position = newPos;
            }
            else
            {
                // Hover: move slowly in a small random direction.
                Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), Random.Range(-0.3f, 0.3f), Random.Range(-1f, 1f)).normalized;
                Vector3 startPos = transform.position;
                Vector3 targetPos = startPos + randomDir * 2f; // Move a small distance.
                float travelTime = 2f;
                float elapsed = 0f;
                while (elapsed < travelTime)
                {
                    transform.position = Vector3.Lerp(startPos, targetPos, elapsed / travelTime);
                    elapsed += Time.deltaTime;
                    yield return null;
                }
            }
            yield return new WaitForSeconds(Random.Range(2f, 4f));
        }
    }

    // Helper: Returns a random position within the boss room bounds.
    Vector3 GetRandomTeleportPosition()
    {
        if (bossRoom == null)
            return transform.position;

        // Assuming the Room class has public floats 'width' and 'length'.
        float halfWidth = bossRoom.width / 2f;
        float halfLength = bossRoom.depth / 2f;

        float randomX = Random.Range(bossRoom.center.x - halfWidth, bossRoom.center.x + halfWidth);
        float randomZ = Random.Range(bossRoom.center.z - halfLength, bossRoom.center.z + halfLength);
        // Optionally, preserve the boss's current Y (height) or use a fixed elevated value.
        float y = transform.position.y;
        return new Vector3(randomX, y, randomZ);
    }

    // Main attack loop that selects attacks based on the current phase.
    IEnumerator BossAttackLoop()
    {
        while (currentHP > 0)
        {
            switch (currentPhase)
            {
                case BossPhase.Phase1:
                    yield return StartCoroutine(FireProjectileVolley(projectileSpeedPhase1, false));
                    if (Random.value < 0.2f)
                    {
                        yield return StartCoroutine(CreateMagicalBarrier());
                    }
                    yield return new WaitForSeconds(2f);
                    break;

                case BossPhase.Phase2:
                    yield return StartCoroutine(SummonStoneGolems());
                    yield return StartCoroutine(FireProjectileVolley(projectileSpeedPhase1, false));
                    yield return StartCoroutine(ChanneledArcaneBeamAttack(3f, false));
                    yield return new WaitForSeconds(1f);
                    break;

                case BossPhase.Phase3:
                    yield return StartCoroutine(FireProjectileVolley(projectileSpeedPhase3, true));
                    yield return StartCoroutine(ChanneledArcaneBeamAttack(1.5f, true));
                    yield return new WaitForSeconds(0.5f);
                    break;
            }
            yield return null;
        }
    }

    // Fires a volley of three projectiles.
    IEnumerator FireProjectileVolley(float projSpeed, bool applySlow)
    {
        Vector3 targetDir = GetPlayerDirection();
        for (int i = 0; i < 3; i++)
        {
            LaunchArcaneProjectile(targetDir, projSpeed, applySlow);
            yield return new WaitForSeconds(0.6f);
        }
        yield return null;
    }

    // Returns a normalized direction vector from the boss to the player.
    Vector3 GetPlayerDirection()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            return (player.transform.position - transform.position).normalized;
        return transform.forward;
    }

    // Instantiates an arcane projectile.
    void LaunchArcaneProjectile(Vector3 direction, float speed, bool applySlow)
    {
        if (arcaneProjectilePrefab == null)
        {
            // Create a basic projectile if none assigned.
            arcaneProjectilePrefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            arcaneProjectilePrefab.transform.localScale = Vector3.one * 0.5f;
            Rigidbody rb = arcaneProjectilePrefab.AddComponent<Rigidbody>();
            rb.useGravity = false;
            Collider col = arcaneProjectilePrefab.GetComponent<Collider>();
            col.isTrigger = true;
            ArcaneProjectile_Copilot ap = arcaneProjectilePrefab.AddComponent<ArcaneProjectile_Copilot>();
            ap.damage = magicalDamage;
            ap.applySlow = applySlow;
            ap.slowDuration = 3f;
            arcaneProjectilePrefab.SetActive(false);
        }
        Vector3 spawnPos = transform.position; // Optionally add an offset.
        GameObject projectile = Instantiate(arcaneProjectilePrefab, spawnPos, Quaternion.LookRotation(direction));
        projectile.SetActive(true);
        Rigidbody projRb = projectile.GetComponent<Rigidbody>();
        projRb.velocity = direction * speed;
        Destroy(projectile, 5f);
    }

    // Creates a magical barrier attack.
    IEnumerator CreateMagicalBarrier()
    {
        Debug.Log(enemyName + " is telegraphing a magical barrier!");
        yield return new WaitForSeconds(1f);
        Debug.Log("A magical barrier is now formed for 5 seconds.");
        // Instantiate barrier effects here.
        yield return new WaitForSeconds(5f);
    }

    // Summons two Stone Golems (loaded from Resources/Enemies).
    IEnumerator SummonStoneGolems()
    {
        GameObject stoneGolemPrefab = Resources.Load<GameObject>("Enemies/StoneGolem");
        if (stoneGolemPrefab != null)
        {
            Vector3 pos1 = transform.position + new Vector3(2f, -1f, 2f);
            Vector3 pos2 = transform.position + new Vector3(-2f, -1f, 2f);
            Instantiate(stoneGolemPrefab, pos1, Quaternion.identity);
            Instantiate(stoneGolemPrefab, pos2, Quaternion.identity);
            Debug.Log(enemyName + " summons two Stone Golems!");
        }
        else
        {
            Debug.Log("Stone Golem prefab not found in Resources/Enemies!");
        }
        yield return null;
    }

    // Executes a channeled arcane beam attack.
    IEnumerator ChanneledArcaneBeamAttack(float chargeTime, bool widerAoE)
    {
        Debug.Log(enemyName + " begins channeling an arcane beam for " + chargeTime + " seconds.");
        yield return new WaitForSeconds(chargeTime);
        Debug.Log(enemyName + (widerAoE ? " unleashes a wide beam attack!" : " unleashes a focused beam attack!"));
        // Create beam effects and apply damage here.
        yield return null;
    }
}
