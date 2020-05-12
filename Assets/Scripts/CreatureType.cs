using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Creature Type", menuName = "Creature Type")]
public class CreatureType : ScriptableObject
{
    public new string name;
    public Sprite sprite;
    public float healthDefault;
    public float manaDefault;
    public float defenseDefault;
    public float attackDefault;
    public float attackMultDefault;
}
