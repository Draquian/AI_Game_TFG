using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class PlayerStatsDisplay_Copilot : MonoBehaviour
{
    // Reference to the PlayerStats_Copilot component attached to the player
    [SerializeField] private PlayerStats_Copilot playerStats;

    // UI elements for filled bars (assumes these Image components use fill mode settings)
    [SerializeField] private Image hpBar;
    [SerializeField] private Image staminaBar;
    [SerializeField] private Image expBar;

    // The mana display is now a filled Image that changes color based on mana ratio
    [SerializeField] private Image manaImage;

    // The level text now uses TextMeshPro (TMP_Text) which will fade in and then fade out on a level change
    [SerializeField] private TMP_Text levelText;

    // Define the full mana color when mana is at maximum (set via Inspector or code)
    [SerializeField] private Color fullManaColor = Color.cyan;

    // Animation timing settings for the level text (in seconds)
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float displayDuration = 2.0f;
    [SerializeField] private float fadeOutDuration = 0.5f;

    // To track level change
    private int previousLevel;
    private Coroutine levelTextCoroutine;

    private void Start()
    {
        // Validate references
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats_Copilot reference not set in the inspector.");
        }
        if (hpBar == null || staminaBar == null || expBar == null || manaImage == null)
        {
            Debug.LogError("One or more UI Image references are not set in the inspector.");
        }
        if (levelText == null)
        {
            Debug.LogError("UI Text reference for level is not set in the inspector.");
        }

        // Initialize the previous level value and set initial alpha to 0 (hidden)
        previousLevel = playerStats.level;
        SetLevelTextAlpha(0f);

        switch (playerStats.magicAbility)
        {
            case ElectricityMagic_Copilot:
                manaImage.sprite = Resources.Load<Sprite>("Magic/Electricity/ElectricMana");
                break;
            case FireMagic_Copilot:
                manaImage.sprite = Resources.Load<Sprite>("Magic/Fire/FireMana");
                break;
            case WaterMagic_Copilot:
                manaImage.sprite = Resources.Load<Sprite>("Magic/Water/WaterMana");
                break;
            case ShadowMagic_Copilot:
                manaImage.sprite = Resources.Load<Sprite>("Magic/Shadow/ShadowMana");
                break;
            case LightMagic_Copilot:
                manaImage.sprite = Resources.Load<Sprite>("Magic/Light/LightMana");
                break;
        }

        // Optionally update all UI elements at the start
        UpdateStats();
    }

    private void Update()
    {
        // Continuously update the UI elements at the desired rate
        UpdateStats();
    }

    /// <summary>
    /// Updates all UI elements with the current player stats from PlayerStats_Copilot.
    /// </summary>
    private void UpdateStats()
    {
        if (playerStats == null)
            return;

        // Update the HP bar; avoid division by zero
        if (playerStats.maxHP > 0)
            hpBar.fillAmount = playerStats.HP / playerStats.maxHP;
        else
            hpBar.fillAmount = 0f;

        // Update the Stamina bar
        if (playerStats.maxStamina > 0)
            staminaBar.fillAmount = playerStats.stamina / playerStats.maxStamina;
        else
            staminaBar.fillAmount = 0f;

        // Update the Experience bar
        if (playerStats.experienceThreshold > 0)
            expBar.fillAmount = playerStats.experience / (float)playerStats.experienceThreshold;
        else
            expBar.fillAmount = 0f;

        // Update the Mana image (bar) with fill amount and color interpolation
        if (playerStats.maxMana > 0)
        {
            float manaFraction = playerStats.Mana / playerStats.maxMana;
            manaImage.fillAmount = manaFraction;
            manaImage.color = Color.Lerp(Color.gray, fullManaColor, manaFraction);
        }
        else
        {
            manaImage.fillAmount = 0f;
            manaImage.color = Color.gray;
        }

        // Check for a level change, and if there's one, animate the level UI text
        if (playerStats.level != previousLevel)
        {
            // If there's already an animation in progress, stop it first
            if (levelTextCoroutine != null)
                StopCoroutine(levelTextCoroutine);

            levelTextCoroutine = StartCoroutine(AnimateLevelText());
            previousLevel = playerStats.level;
        }
    }

    /// <summary>
    /// Animates the level text to fade in, stay visible, and then fade out.
    /// </summary>
    private IEnumerator AnimateLevelText()
    {
        // Set the text to the new level value
        levelText.text = "Level: " + playerStats.level.ToString();

        // Fade In
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeInDuration);
            SetLevelTextAlpha(alpha);
            yield return null;
        }
        SetLevelTextAlpha(1f);

        // Wait for the display duration (text at full opacity)
        yield return new WaitForSeconds(displayDuration);

        // Fade Out
        elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
            SetLevelTextAlpha(alpha);
            yield return null;
        }
        SetLevelTextAlpha(0f);
    }

    /// <summary>
    /// Helper method to set the alpha of the levelText.
    /// </summary>
    /// <param name="alpha">Alpha value to be set [0,1].</param>
    private void SetLevelTextAlpha(float alpha)
    {
        Color newColor = levelText.color;
        newColor.a = alpha;
        levelText.color = newColor;
    }
}
