using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Dialogue : MonoBehaviour
{
    
    public TextMeshProUGUI text;
    public Image arrow;
    public Queue<string> dialogueQueue = new Queue<string>();
    public int maxCharacters;

    void Start () {
        QueueDialogue("Wassup son!");
        QueueDialogue("How's it going my sexy demonesque");
        QueueDialogue("Please sir I am a walnut and do not appreciate it");
    }

    /// <summary>Queues a new line of dialogue to be displayed</summary>
    /// <returns>True if the line had to be split into multiple boxes</returns>
    public bool QueueDialogue (string dialogue) {
        if (dialogue.Length <= maxCharacters) {
            dialogueQueue.Enqueue(dialogue);
            return false;
        }
        string d = dialogue;
        while (d.Length > 0) {
            if (d.Length > maxCharacters) {
                Regex trimTrailingWord = new Regex("(.*) (.*)$");
                string segment = trimTrailingWord.Match(d).Groups[1].Value;
                dialogueQueue.Enqueue(segment);
                d = d.Substring(segment.Length);
            } else {
                dialogueQueue.Enqueue(d);
                break;
            }
        }
        return true;
    }

    public void DisplayNext () {
        if (dialogueQueue.Count == 0) {
            gameObject.SetActive(false);
            return;
        }
        gameObject.SetActive(true);
        string next = dialogueQueue.Dequeue();
        arrow.enabled = dialogueQueue.Count > 0;
        text.text = next;
    }
    
}
