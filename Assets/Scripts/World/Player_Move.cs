using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Move : MonoBehaviour
{

    #region Player components
    [SerializeField]
    [Tooltip("Speed with which player moves.")]
    private float spd = 1f;
    private Vector3 pos;

    /* For animation stuff, later: */
    private Animator animate;
    private bool playerMoving;

    /* True if player can move, false otherwise
     (mainly a to freeze the player during dialogue events) */
    public bool canMove;

    private Rigidbody2D rb2d;
    #endregion


    #region Unity fxns
    // Start is called before the first frame update
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (canMove) {
            move();
        }
        

    }
    #endregion

    #region Player movement functions
    void move() {

        pos = transform.position;

        //update position
        pos.y += Input.GetAxisRaw("Vertical") * spd * Time.deltaTime;
        pos.x += Input.GetAxisRaw("Horizontal") * spd * Time.deltaTime;

        //update position in gameobject's transform object
        transform.position = pos;
    }
    #endregion

    #region Player-World Interaction functions
    /* such as: checking objects */

    #endregion
}
