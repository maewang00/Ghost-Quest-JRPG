using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager_Title : MonoBehaviour
{

    private int option = 0;

    [SerializeField]
    private Image[] pointers;

    [SerializeField]
    private GameObject GhostGameHeader;

    [SerializeField]
    private GameObject optionsScrollMenu;

    [SerializeField]
    private GameObject credits;

    private GameManager scenemanager;
    private bool incredits = false;

    // Start is called before the first frame update
    void Start()
    {
        scenemanager = GameObject.FindWithTag("GameController").GetComponent<GameManager>();
        CloseCredits();
    }

    // Update is called once per frame
    void Update()
    {
        StartCoroutine("MenuNavigation");

        
    }

    IEnumerator MenuNavigation() {
        if (!incredits) {
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetAxisRaw("Vertical") < 0) {
                //pressing down:
                yield return new WaitUntil(() => Input.GetAxisRaw("Vertical") == 0);

                option--;
                option = Mathf.Abs(option);
                option = option % pointers.Length;
            }
            if (Input.GetAxisRaw("Vertical") > 0) {
                yield return new WaitUntil(() => Input.GetAxisRaw("Vertical") == 0);
                option++;
                option = option % pointers.Length;
            }

            HighlightOption(option);

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Joystick1Button1)) { //|| 
                SelectOption(option);
            }
        }

        else {
            if (Input.anyKeyDown) {
                CloseCredits();
            }
        }
    }

    void HighlightOption(int option) {
        UnhighlightAll();
        pointers[option].enabled = true;
    }

    void UnhighlightAll() {
        foreach (Image pointer in pointers) {
            pointer.enabled = false;
        }
    }

    void SelectOption(int optionNumber) {

        switch(optionNumber) {
            case 0: //start game
                scenemanager.OverworldScene();
                break;
            default: //see the credits menu
                incredits = true;
                DisplayCredits();
                break;
        }
    }


    void DisplayCredits() {
        GhostGameHeader.SetActive(false);
        optionsScrollMenu.SetActive(false);
        credits.SetActive(true);
    }

    void CloseCredits() {
        credits.SetActive(false);
        GhostGameHeader.SetActive(true);
        optionsScrollMenu.SetActive(true);
        incredits = false;
    }
}
