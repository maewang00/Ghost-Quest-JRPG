using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonEntrance : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.CompareTag("Player"))
        {
            GameObject gm = GameObject.FindWithTag("GameController");
            gm.GetComponent<GameManager>().DungeonScene();
        }
    }
}
