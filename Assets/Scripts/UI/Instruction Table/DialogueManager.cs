using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public GameObject dialoguePanel;       // Panel chứa Text
    public TextMeshProUGUI dialogueText; // Text để hiện câu thoại
    private Queue<string> sentences;

    void Start()
    {
        sentences = new Queue<string>();
        dialoguePanel.SetActive(false);
    }

    public void StartDialogue(Dialogue dialogue)
    {
        dialoguePanel.SetActive(true);
        sentences.Clear();

        foreach (string sentence in dialogue.sentences)
        {
            sentences.Enqueue(sentence);
        }

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        string sentence = sentences.Dequeue();
        dialogueText.text = sentence;
    }

    public void EndDialogue()
    {
        dialoguePanel.SetActive(false);
    }

    void Update()
    {
        if (dialoguePanel.activeSelf && Input.GetKeyDown(KeyCode.F))
        {
            DisplayNextSentence();
        }
    }
}
