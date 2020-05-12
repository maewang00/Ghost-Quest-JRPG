using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Player_Move : MonoBehaviour
{

    #region Player components
    [SerializeField]
    [Tooltip("Speed with which player walks.")]
    private float walkSpd = 1f;
    [SerializeField]
    [Tooltip("How much faster the player should move while running.")]
    private float runSpd = 1f;

    /* Player's actual speed: */
    private float spd;

    /* true if player is running (moving faster) false otherwise */
    private bool isRunning;

    private Vector3 pos;
    public Vector3 currDirection;
    [SerializeField]
    [Tooltip("Maximum range that player can check objects in the field.")]
    private float checkDistance;

    /* True if player can move, false otherwise
     (mainly a to freeze the player during dialogue events) */
    public bool canMove;


    #endregion

    #region References
    /* For animation stuff, later: */
    [SerializeField]
    [Tooltip("Reference to player's animator")]
    private Animator animate;
    private bool isMoving;

    [SerializeField]
    [Tooltip("Reference to the event system")]
    private Rigidbody2D rb2d;

    //reference to the event system:
    [SerializeField]
    [Tooltip("Reference to the event system")]
    private CustomEvents eventsys;

    private AudioManager audioManager;
    private DialogueManager dialogueManager;

    #endregion


    #region Unity fxns
    // Start is called before the first frame update
    void Start()
    {
        rb2d = this.GetComponent<Rigidbody2D>();
        spd = walkSpd;
        isRunning = false;
        animate = GetComponent<Animator>();
        GameObject gm = GameObject.FindWithTag("AudioManager");
        audioManager = gm.GetComponent<AudioManager>();
        gm = GameObject.FindWithTag("DialogueManager");
        dialogueManager = gm.GetComponent<DialogueManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (canMove) {
            checkRun(); //check to see if the player wants to run
            move();
        }
        if (Input.GetKeyDown(KeyCode.Space) && canMove) //Player wants to interact with thing in front of player
        {
            Interact();
        }

    }

    //called every fixed frame
    void FixedUpdate() {
        
    }
    #endregion

    #region Player movement functions
    private void move() {
        
        //thanks /r/unity2d: https://redd.it/8gb34x

        //Movement
        float x_dir = Input.GetAxisRaw("Horizontal") * spd;
        float y_dir = Input.GetAxisRaw("Vertical") * spd;
        
        Vector2 player_vel = rb2d.velocity;
        player_vel.x = x_dir;
        player_vel.y = y_dir;
        rb2d.velocity = player_vel;


        if (Input.GetAxisRaw("Horizontal") < 0) {
            currDirection.x = -1;
        } else if (Input.GetAxisRaw("Horizontal") > 0) {
            currDirection.x = 1;
        } else if (Input.GetAxisRaw("Vertical") != 0) {
            currDirection.x = 0;
        }

        if (Input.GetAxisRaw("Vertical") < 0) {
            currDirection.y = -1;
        } else if (Input.GetAxisRaw("Vertical") > 0) {
            currDirection.y = 1;
        } else if (Input.GetAxisRaw("Horizontal") != 0) {
            currDirection.y = 0;
        }


        //Animator:
        animate.SetFloat("DirX", currDirection.x);
        animate.SetFloat("DirY", currDirection.y);

        //Max Movement spd
        if (rb2d.velocity.magnitude > spd) {
            rb2d.velocity *= spd / rb2d.velocity.magnitude;
        }

        if (rb2d.velocity.magnitude != 0) {
            isMoving = true;
        }


    }

    private void checkRun() {
        
        if (Input.GetKeyDown(KeyCode.Z) && !isRunning) {
            isRunning = true;
            spd += runSpd;
        }

        if (Input.GetKeyUp(KeyCode.Z)) {
            isRunning = false;
            spd = walkSpd;
        }
    }

    public void cutsceneIsActive() {
        canMove = false;
        rb2d.velocity = Vector2.zero;
    }

    public void cutsceneIsOver() {
        canMove = true;

    }

    #endregion

    #region Player-World Interaction functions
    /* such as: checking objects */


    /*
     * TODO: May want to move this to a more general "objects manager" instead of
     * keeping it here in the player's script.
     */

    /*
     * IMPORTANT: How to format strings
     * I've hardcoded it so that the character '|' (the thing above the \ key on the keyboard)
     * is a "new text box" command.
     * If a string is long enough Unity is smart enough to wrap the text so it'll fit.
     *
     * (How to make newlines)
     * If you want to make a new line ON COMMAND (ex. poetry) you have to add '\n'
     * Specifically, the newline character. For example, if I wanted the text box to say:
     *
     * Haiku are fun to
     * make: for five syllables use
     * refridgerator
     *
     * then I would format the string as:
     * "Haiku are fun to" + '\n' + "make: for five syllables use" + '\n' + "refridgerator"
     *
     * (A note about style)
     * Also note that I've stylistically made it so that all dialogue AFTER a newline
     * will start with a "-".
     * so the above example would actually look like:
     *
     * - Haiku are fun to
     * - make: for five syllalbes use
     * - refridgerator
     *
     * This is mostly because i don't expect poetry to be in the game (we'll cross that bridge
     * if it happens) and so that text is more similar to how it is in Undertale or EarthBound.
     *
     * ex.
     *
     * - Wow, it's a lovely day! I wonder what kind of
     * adventures await us.
     * - …wait, I forgot to change out of my PJ's!
     * I better run back home!
     *
     * It'll also be useful for text scrolling, if/when we implement that.
     * --Lena
     */
    
    private void Interact() {
        Debug.Log("Check");
        Vector2 checkbox = new Vector2(1, 1);

        RaycastHit2D[] hits = Physics2D.BoxCastAll(this.transform.position, checkbox,
            0, currDirection * 1, 1);

        foreach (RaycastHit2D hit in hits) {
            if (hit.transform.CompareTag("Interactable")) {
                dialogueManager.Interact(hit.transform.name);
                break;
            }
        }



        /* Adrian's: (a little buggy) */
        //RaycastHit2D[] hits = Physics2D.BoxCastAll(transform.position
        //    + currDirection, new Vector2(0.75f, 0.75f), 0f, Vector2.zero, 0);

        /*RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, currDirection, checkDistance);

        List<RaycastHit2D> hitList;
        hitList = hits.Where(h => h.transform.CompareTag("Interactable")).ToList();
        if (hitList.Count > 0) {
            float closestDist = hitList.Min(h => h.distance);
            RaycastHit2D hit = hitList.First(h => h.distance == closestDist);
            string dialogue = interactions[hit.transform.name];
            audioManager.SFX_checkObject();
            eventsys.StartCutscene(dialogue);
        }*/
    }

    #endregion

    
}
