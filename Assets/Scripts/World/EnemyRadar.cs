using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRadar : MonoBehaviour
{

    #region References to fields

    [SerializeField]
    [Tooltip("Reference to the radar UI's manager")]
    private EnemyRadarUI radarUIman;

    private Rigidbody2D rb2d;

    #endregion

    #region vars

    private int thresholdValue = 100;

    /* counter that increments slowly when player is in a specific zone */
    [SerializeField]
    [Tooltip("Visible for debug purposes.")]
    private int thresholdCounter;

    /* true if the player's RB2D has a velocity with magnitude != 0, false otherwise */
    private bool isMoving;

    private float timeBetweenEncounterUpdates = 2f;
    private float timer;


    /* true if in "hostile zone", false otherwise */
    [SerializeField]
    [Tooltip("Visible for debug purposes.")]
    private bool inHostileZone = false;

    #endregion


    private void Update() {
        isMoving = rb2d.velocity.magnitude != 0;
    }


    #region Psuedo-random Enemy radar functions and variables


    #region funcs
    /* Methods */

    private void Start() {
        rb2d = GetComponent<Rigidbody2D>();
        radarUIman.updateRadar(thresholdCounter);
        isMoving = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Danger Area"))
        {
            Debug.Log("Enter");
            inHostileZone = true;
            StartCoroutine(encounterTrigger());
        }


    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Danger Area"))
        {
            Debug.Log("Exit");
            inHostileZone = false;
            thresholdCounter = 0;
            radarUIman.updateRadar(thresholdCounter);
        }
    }


    IEnumerator encounterTrigger()
    {

        while (inHostileZone)
        {
            int increment = Random.Range(5, 20);

            if (isMoving) {
                if (incrementTimer()) {
                    thresholdCounter += increment;
                    radarUIman.updateRadar(thresholdCounter);
                }
            }

            if (thresholdCounter >= thresholdValue)
            {
                //TODO: launch enemy encounter.
                // insert call here.
                GameObject gm = GameObject.Find("Player");
                Vector3 pos = gm.transform.position;
                gm = GameObject.FindWithTag("GameController");
                gm.GetComponent<GameManager>().BattleScene(pos);
                //thresholdCounter = 0;
            }

            yield return null;
        }

        
    }

    private bool incrementTimer() {
        timer -= Time.deltaTime;

        if (timer <= 0) {
            timer = timeBetweenEncounterUpdates;
            return true;
        }
        return false;
    }

    #endregion



    #endregion
}
