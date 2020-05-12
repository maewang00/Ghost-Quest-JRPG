using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events;

[System.Serializable] public class _UnityEventString : UnityEvent<string> { }

public class CustomEvents : MonoBehaviour {

    #region setting the scene
    public static CustomEvents current; //make it a singleton, i.e. there's only one in the scene

    private void Awake() {
        current = this;

        GameObject gm = GameObject.FindWithTag("AudioManager");
        audioManager = gm.GetComponent<AudioManager>();
    }

    public bool test;

    #endregion

    #region vars
    private bool cutsceneInProgress = false;
    private bool readingInProgress = false;

    private char stringParser = '|';

    private AudioManager audioManager;
    #endregion


    #region events related to dialogue
    public _UnityEventString _printDialogue;
    public void PrintDialogue(string s) {
        readingInProgress = true;
        _printDialogue.Invoke(s);
    }

    public UnityEvent _closeDialogue;
    public void CloseDialogue() {
        readingInProgress = false;
        _closeDialogue.Invoke();
    }

    public _UnityEventString _startCutscene;
    public void StartCutscene(string fullCutsceneText) {
        cutsceneInProgress = true;
        string[] dialogues = fullCutsceneText.Split(stringParser);
        
        StartCoroutine(dialogueEvent(dialogues));
        _startCutscene.Invoke("");
    }

    IEnumerator dialogueEvent(string[] dialogues) {
        yield return new WaitUntil(() => Input.GetKeyUp(KeyCode.Space));
        string printme;
        string format = "\n";
        string replace = "\n- ";
        foreach (string str in dialogues) {
            printme = "- " + str;
            printme = printme.Replace(format, replace);

            PrintDialogue(printme);

            while (readingInProgress) {
                if (Input.GetKeyDown(KeyCode.Space)) {
                    readingInProgress = false;
                    audioManager.SFX_advanceText();
                }
                yield return null;
            }

        }
        CloseDialogue();
        EndCutscene();
    }

    public UnityEvent _endCutscene;
    public void EndCutscene() {
        cutsceneInProgress = false;
        _endCutscene.Invoke();
    }
    #endregion


    #region events related to enemy encounters

    //public UnityEvent


    #endregion

}
