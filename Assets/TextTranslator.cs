using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using LLMUnity;

/// <summary>
/// Translates TextMeshPro text components on scene load and whenever their text changes or they become active.
/// Uses a single LLMCharacter instance from the LLM for Unity package.
/// </summary>
public class TextTranslator : MonoBehaviour
{
    [Tooltip("Reference to an LLMCharacter (from LLM for Unity) used for translation.")]
    public LLMCharacter llmCharacter;

    [Tooltip("Target language for translation (e.g. \"Spanish\").")]
    public string targetLanguage = "French";

    // Cache to avoid re-translating the same text
    private readonly Dictionary<TMP_Text, string> translatedTexts = new Dictionary<TMP_Text, string>();

    async void Start()
    {
        // Ensure we have a valid LLMCharacter
        if (llmCharacter == null)
        {
            llmCharacter = FindObjectOfType<LLMCharacter>();
            if (llmCharacter == null)
            {
                Debug.LogError("LLMCharacter reference is missing and none was found in the scene. Please add an LLMCharacter component.");
                return;
            }
        }

        // Subscribe to TextMeshPro text-change events
        TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);

        // Initial batch translation for existing active texts
        await TranslateAllTexts();
    }

    void OnDestroy()
    {
        // Unsubscribe when this object is destroyed
        TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);
    }

    /// <summary>
    /// Public method to set the target language at runtime and re-translate all texts.
    /// </summary>
    public async void SetTargetLanguage(string language)
    {
        targetLanguage = language;
        translatedTexts.Clear(); // Clear cache to force re-translation
        await TranslateAllTexts();
    }

    /// <summary>
    /// Translates all active TMP_Text components that haven't been translated yet.
    /// </summary>
    public async Task TranslateAllTexts()
    {
        var textComps = FindObjectsOfType<TMP_Text>();
        foreach (var textComp in textComps)
        {
            if (textComp == null || !textComp.gameObject.activeInHierarchy)
                continue;

            await TryTranslate(textComp);
        }
    }

    /// <summary>
    /// Callback for any TextMeshPro text-change event.
    /// </summary>
    private void OnTextChanged(UnityEngine.Object obj)
    {
        if (obj is TMP_Text textComp && textComp.gameObject.activeInHierarchy)
        {
            _ = TryTranslate(textComp);
        }
    }

    /// <summary>
    /// Translates a single TMP_Text if it hasn't been translated or if its text changed.
    /// Skips objects tagged with "title".
    /// </summary>
    private async Task TryTranslate(TMP_Text textComp)
    {
        // Skip objects tagged as "title"
        if (textComp.gameObject.CompareTag("Title"))
            return;

        string currentText = textComp.text;
        if (string.IsNullOrEmpty(currentText))
            return;

        // Skip if already translated and matches
        if (translatedTexts.TryGetValue(textComp, out var last) && last == currentText)
            return;

        // Build prompt
        string prompt = $"Translate the following text to {targetLanguage}: {currentText}";
        try
        {
            string translated = await llmCharacter.Chat(prompt);
            if (!string.IsNullOrEmpty(translated))
            {
                textComp.text = translated;
                translatedTexts[textComp] = translated;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Translation failed for '{currentText}': {e.Message}");
        }
    }
}
