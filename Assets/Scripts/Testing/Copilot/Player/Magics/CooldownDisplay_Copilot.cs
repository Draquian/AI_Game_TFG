using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CooldownDisplay_Copilot : MonoBehaviour
{
    // Reference to the PlayerStats_Copilot script attached to the player.
    [SerializeField] private PlayerStats_Copilot playerStats;

    // --- Ability 1 UI ---
    // These represent the UI elements for the first ability (for example, magicAbility).
    // The image will transition from gray to a specified full color.
    [SerializeField] private Image ability1Image;
    [SerializeField] private TMP_Text ability1CooldownText;
    [SerializeField] private Color ability1FullColor = Color.cyan;

    // --- Ability 2 UI ---
    [SerializeField] private Image ability2Image;
    [SerializeField] private TMP_Text ability2CooldownText;
    [SerializeField] private Color ability2FullColor = Color.magenta;

    // --- Ability 3 UI ---
    [SerializeField] private Image ability3Image;
    [SerializeField] private TMP_Text ability3CooldownText;
    [SerializeField] private Color ability3FullColor = Color.yellow;

    private void Start()
    {
        // Validate the required references.
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats_Copilot reference is not set in the inspector.");
        }
        if (ability1Image == null || ability1CooldownText == null ||
            ability2Image == null || ability2CooldownText == null ||
            ability3Image == null || ability3CooldownText == null)
        {
            Debug.LogError("One or more ability UI references are not set in the inspector.");
        }

        // Dynamically attach the corresponding magic script to this game object.
        switch (playerStats.magicAbility)
        {
            case ElectricityMagic_Copilot:
                ability1Image.sprite = Resources.Load<Sprite>("Magic/Electricity/ElectricAttack");
                ability2Image.sprite = Resources.Load<Sprite>("Magic/Electricity/ElectricCharge");
                ability3Image.sprite = Resources.Load<Sprite>("Magic/Electricity/ElectricBoost");
                break;
            case FireMagic_Copilot:
                ability1Image.sprite = Resources.Load<Sprite>("Magic/Fire/FireAttack");
                ability2Image.sprite = Resources.Load<Sprite>("Magic/Fire/FireCharge");
                ability3Image.sprite = Resources.Load<Sprite>("Magic/Fire/FireBoost");
                break;
            case WaterMagic_Copilot:
                ability1Image.sprite = Resources.Load<Sprite>("Magic/Water/WaterAttack");
                ability2Image.sprite = Resources.Load<Sprite>("Magic/Water/WaterCharge");
                ability3Image.sprite = Resources.Load<Sprite>("Magic/Water/WaterBoost");
                break;
            case ShadowMagic_Copilot:
                ability1Image.sprite = Resources.Load<Sprite>("Magic/Shadow/ShadowAttack");
                ability2Image.sprite = Resources.Load<Sprite>("Magic/Shadow/ShadowCharge");
                ability3Image.sprite = Resources.Load<Sprite>("Magic/Shadow/ShadowBoost");
                break;
            case LightMagic_Copilot:
                ability1Image.sprite = Resources.Load<Sprite>("Magic/Light/LightAttack");
                ability2Image.sprite = Resources.Load<Sprite>("Magic/Light/LightCharge");
                ability3Image.sprite = Resources.Load<Sprite>("Magic/Light/LightBoost");
                break;
        }
    }

    private void Update()
    {
        if (playerStats == null)
            return;

        // Update each ability’s UI using a helper method.
        UpdateAbilityUI(playerStats.magicAbility, playerStats.magicAbility.magicAttackCooldown, playerStats.magicAbility.magicAttacCooldownRemaining, ability1Image, ability1CooldownText, ability1FullColor);
        UpdateAbilityUI(playerStats.magicAbility, playerStats.magicAbility.chargedAttackCooldown, playerStats.magicAbility.chargedAttackCooldownRemaining, ability2Image, ability2CooldownText, ability2FullColor);
        UpdateAbilityUI(playerStats.magicAbility, playerStats.magicAbility.boostCooldown, playerStats.magicAbility.boostCooldownRemaining, ability3Image, ability3CooldownText, ability3FullColor);
    }

    /// <summary>
    /// Updates the cooldown UI for a given ability.
    /// </summary>
    /// <param name="ability">The ability script (assumed type MagicAbility) that exposes cooldown info.</param>
    /// <param name="img">The UI Image that will display the fill and color transition.</param>
    /// <param name="cooldownText">The TextMeshPro text that will show the remaining cooldown time.</param>
    /// <param name="fullColor">The full (ready) color for the ability image.</param>
    private void UpdateAbilityUI(MagicBase_Copilot ability, float cooldownDuration, float cooldownRemaining, Image img, TMP_Text cooldownText, Color fullColor)
    {
        if (ability == null)
        {
            Debug.LogWarning("Ability reference is null.");
            return;
        }

        // Calculate progress: when an ability is on full cooldown, progress = 0; when ready, progress = 1.
        float progress = cooldownDuration > 0 ? 1f - (cooldownRemaining / cooldownDuration) : 1f;

        // Update image fill amount (assumes your Image component is set to Filled mode).
        img.fillAmount = progress;
        // Lerp the image’s color from gray (on cooldown) to fullColor (ready).
        img.color = Color.Lerp(Color.gray, fullColor, progress);

        // Update the cooldown text: if there is remaining cooldown, show it in seconds.
        if (cooldownRemaining > 0)
        {
            cooldownText.text = string.Format("{0:0.0}s", cooldownRemaining);
        }
        else
        {
            // When the ability is ready, clear the text or display "Ready" as preferred.
            cooldownText.text = "";
            // Alternatively: cooldownText.text = "Ready";
        }
    }
}
