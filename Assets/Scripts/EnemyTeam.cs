using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Enemy Team", menuName = "Enemy Team")]
public class EnemyTeam : ScriptableObject
{
    public List<CreatureType> Enemies;
}
