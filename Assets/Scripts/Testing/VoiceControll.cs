using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;
using System.Linq;
using UnityEngine.UI;


public class VoiceControll : MonoBehaviour
{
    KeywordRecognizer keywordRecognizer;
    Dictionary<string, Action> actions = new Dictionary<string, Action>();

    public string slot1;

    void Start()
    {
        actions.Add("exit", ExitGame);
        addToRecognition();
    }

    void RecognizedSpeech(PhraseRecognizedEventArgs speech)
    {
        Debug.Log("text");
        actions[speech.text].Invoke();
    }

    void Slot1()
    {
        Debug.Log("test Completed");
    }

    void ExitGame()
    {
        Debug.Log("Quit application");

        //Application.Quit();
    }

    public void setText(string text)
    {
        actions.Remove(slot1);
        slot1 = text;
        actions.Add(slot1, Slot1);
    }

    void addToRecognition()
    {
        if (keywordRecognizer != null)
        {
            keywordRecognizer.Stop();
            keywordRecognizer.OnPhraseRecognized -= RecognizedSpeech;
        }

        keywordRecognizer = new KeywordRecognizer(actions.Keys.ToArray());
        keywordRecognizer.OnPhraseRecognized += RecognizedSpeech;
        keywordRecognizer.Start();
        Debug.Log("start voice");
    }
}
