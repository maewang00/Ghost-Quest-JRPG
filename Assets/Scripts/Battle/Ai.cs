using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ai : Character
{
    public void copyValues(CreatureType creature)
    {
        name = creature.name;
        sprite = creature.sprite;
        healthDefault = creature.healthDefault;
        manaDefault = creature.manaDefault;
        defenseDefault = creature.defenseDefault;
        _defense = defenseDefault;
        attackDefault = creature.attackDefault;
        attackMultDefault = creature.attackMultDefault;
        sprite = creature.sprite;
        _defenseMult = 1.0f;
    }
    
    // For now this assumes that the whole team is made up of Ais
    // I want to later extend this to have any Character list and not just Ais
    // Returns the enemy that died
    // If no enemy died then it returns null
    public Character TakeAction(List<Ai> friends, List<Character> enemies, Skill s)
    {
        if (this.Attack(enemies[0], s))
        {
            return enemies[0];
        }
        else
        {
            return null;
        }
    }    
}
