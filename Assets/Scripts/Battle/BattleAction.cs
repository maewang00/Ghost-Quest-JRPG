using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* An action that goes into the BattleQueue in the BattleManager
 * NOTE: these will be permanently deleted everytime we popped these off the BattleQueue
 */
public class BattleAction
{

    #region variables

    //Who casted this in battle? (Enemy or Friend?)
    Character caster;

    //Who am I casting it to? (Enemy or Friend?)
    Character target;

    //What has been casted?
    Skill skillCasted;

    #endregion

    //Constructor init
    public BattleAction(Character c, Character _target, Skill skill) {
        caster = c;
        target = _target;
        skillCasted = skill;
    }

    #region getter_functions
    public Character Caster()
    {
        return caster;
    }

    public Character Target()
    {
        return target;
    }

    public Skill Skill()
    {
        return skillCasted;
    }

    #endregion

}
