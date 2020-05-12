using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    #region variables

    Rigidbody2D enemyRB;

    #endregion

    #region unity_functions

    private void Awake()
    {
        enemyRB = GetComponent<Rigidbody2D>();
    }

    #endregion
}
