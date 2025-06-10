using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LLMUnity;
using TMPro;

public class LLMComunication : MonoBehaviour
{
    LLM llm;

    public LLMCharacter llmCharacter;

    public string cLenguage;
    string targetLanguage = "English";
    private bool newLenguage = false;

    private string newText = "NULL";
    // Start is called before the first frame update
    void Start()
    {
        llm = GetComponent<LLM>();
    }

    public void TranslateButtons()
    {
        TextMeshProUGUI buttonText = FindObjectOfType<TextMeshProUGUI>();

        _ = llmCharacter.Chat("Can you tell me in what lenguage is this text: " + buttonText.text + "The response has to be only the lenguage and nothing more", GetLenguage, null, true);
        Debug.Log("PEPE");
        StartCoroutine(ChangeText(targetLanguage));
    }

    IEnumerator ChangeText(string targetLanguage)
    {
        if (targetLanguage != cLenguage)
        {
            newLenguage = false;

            TextMeshProUGUI[] allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);

            for (int i = 0; i < allTexts.Length; i++)
            {
                System.Threading.Tasks.Task<string> task = llmCharacter.Chat("Translate to " + targetLanguage + " the text:" + allTexts[i].text, SetNewText, null, true);
                
                yield return new WaitUntil(() => task.IsCompleted);

                allTexts[i].text = newText;
            }
        }
    }

    void GetLenguage(string msg)
    {
        cLenguage = msg;
        newLenguage = true;

    }

    void SetNewText(string newText)
    {
        if(this.newText != newText)
        {
            this.newText = newText;
        }
    }

    public void SetLenguage(string newLenguage)
    {
        targetLanguage = newLenguage;

        RefreshLenguage();
    }

    void RefreshLenguage()
    {
        TranslateButtons();
    }
}