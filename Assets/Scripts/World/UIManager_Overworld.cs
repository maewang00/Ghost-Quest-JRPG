using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager_Overworld : MonoBehaviour
{

    #region Reference objects

    [SerializeField]
    [Tooltip("Prefab of the dialogue text box.")]
    private GameObject dialogueBox;

    [SerializeField]
    [Tooltip("Location of the canvas group to " +
        "instantiate the dialogue text box into.")]
    private GameObject dialogueBoxCanvas;

    private GameObject currentlyActiveDialogueBox;

    #endregion

    #region Dialogue box creation and destruction

    public void sayDialogue(string text) {
        if (currentlyActiveDialogueBox == null) {
            currentlyActiveDialogueBox = Instantiate(dialogueBox, dialogueBoxCanvas.transform);
        }
        currentlyActiveDialogueBox.GetComponent<DialogueBoxRefs>().txt.text = text;
    }

    public void closeDialogueBox() {
        Destroy(currentlyActiveDialogueBox);
    }
    #endregion

   

}
