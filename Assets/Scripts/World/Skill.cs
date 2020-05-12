using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/* PLEASE NOTE: We will use this class to clobber ALL types of "Skills" of a PLAYER/FRIEND/Enemy
 * instead of making multiple classes (i.e. AttackSkill, DefenseSkill, HealSkill, etc.)
 *
 * The most important variables in this class are:
 * [int SkillType]: ID of the type, and
 * [List<Character> characters]:List of those who own this skill (can be more than one!)
 *
 * Thus some things like "Heal skill" or "Run skill" will not require float damagePoints, etc. and will be
 * defaulted to 0.
 */
public class Skill 
{
    #region variables

    //Owners of the skill (Friends or Enemies)
    List<Character> characters = new List<Character>();

    //Name of skill
    string skillName;

    //Skill description
    string skillDesc;

    /*Type of skill using an int system:
    * [0]: Attack skill
    * [1]: Defense skill
    * [2]: Heal skill
    * [3]: Item skill (this is special: if we find this type, we pull up the "Inventory" instead)
    * [4]: Run skill
    * [5]: Enter Skill List skill
    * [6]: DEF BUFFER
    * [7]: ATT BUFFER
    */
    int SkillType;

    //How much Magic Points are needed to cast this skill (default to 0)
    float MP;

    //How much damage to an enemy (default to 0)
    float damagePoints;

    //How much of the attack you can defend against (default to 0)
    float defensePoints;

    //How much HP can you gain (default to 0)
    float healPoints;

    //Does this affect all enemies/party?
    bool isWide;

    #endregion

    #region functions
    //Constructor init
    public Skill(Character c, string name, int type, float mp = 0.0f, float damagep = 0.0f, float defensep = 0.0f, float healp = 0.0f, bool iswide = false)
    {
        characters.Add(c);
        skillName = name;
        SkillType = type;
        MP = mp;
        damagePoints = damagep;
        defensePoints = defensep;
        healPoints = healp;
        isWide = iswide;

    }

    //Add a new character to the characters "owners" list
    public void addCharacterToSkill(Character c) {
        if (!characters.Contains(c))
        {
            characters.Add(c);
        }
    }
    #endregion

    #region getter_functions
    public string getName()
    {
        return skillName;
    }

    public string getDescription() {
        return skillDesc;
    }

    public int getType()
    {
        return SkillType;
    }

    public float getMp()
    {
        return MP;
    }

    public float getDamagepts()
    {
        return damagePoints;
    }

    public float getDefensepts()
    {
        return defensePoints;
    }

    public float getHealpts()
    {
        return healPoints;
    }

    public bool getisWide()
    {
        return isWide;
    }
    #endregion

    #region setter_functions
    public void setName(string s)
    {
        skillName = s;
    }

    public void setDesc(string s) {
        skillDesc = s;
    }

    public void setType(int i)
    {
        SkillType = i;
    }

    public void setMp(float f)
    {
        MP = f;
    }

    public void setDamagepts(float f)
    {
        damagePoints = f;
    }

    public void setDefensepts(float f)
    {
        defensePoints = f;
    }

    public void setDealpts(float f)
    {
        healPoints = f;
    }
    #endregion



}
