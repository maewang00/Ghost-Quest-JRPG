using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    /*
    #region external_objects
    private BattleManager _manager;
    #endregion
    */
    
    #region base_stats
    //Changed most of these fields to protected so that the Ai class can use them.
    // --Angel
    
    [SerializeField]
    [Tooltip("This character's name.")]
    public string name = "Character";

    [SerializeField]
    [Tooltip("This character's sprite.")]
    protected Sprite sprite;
    
    [SerializeField]
    [Tooltip("This character's default health amount.")]
    protected float healthDefault = 100.0f;
    
    [SerializeField]
    [Tooltip("This character's default mana amount.")]
    protected float manaDefault = 100.0f;
    
    [SerializeField]
    [Tooltip("This character's default defense stat. It is a multiplier value between 0 and 1.")]
    protected float defenseDefault = 1.0f;
    
    [SerializeField]
    [Tooltip("This character's default attack value. It is the amount of damage this character does.")]
    protected float attackDefault = 1.0f;
    
    [SerializeField]
    [Tooltip("This character's default attack multiplier stat. The attack is multiplied by this for a damage increase or penalty.")]
    protected float attackMultDefault = 1.0f;
    #endregion

    #region current_stats
    /* [3-10-20] VARIABLE CHANGES:
     * Made _health/_mana and associated variables public so UI manager can see them.
     * Other code-related changes should be commented appropriately.
     * --Lena
     */
    [HideInInspector]
    public float _health;
    [HideInInspector]
    public float _mana;
    [HideInInspector]
    public float _defense;
    [HideInInspector]
    public float _attack;
    [HideInInspector]
    public float _healthMax;
    [HideInInspector]
    public float _manaMax;
    [HideInInspector]
    public bool isdead;
    [HideInInspector]
    public float damagetookrecently;
    #endregion

    #region modifiers
    private float _healthBoost;
    private float _manaBoost;
    [HideInInspector]
    public float _defenseBoostStack;
    public float _attackBoostStack;
    [HideInInspector]
    public float _attackMult;
    [HideInInspector]
    public float _defenseMult;
    #endregion

    #region class_specifics
    private List<Skill> _CommandList = new List<Skill>();
    private List<Skill> _skillList = new List<Skill>();
    #endregion

    #region unity_functions
    // Start is called before the first frame update
    void Awake()
    {
        //_manager = GetComponent<BattleManager>();
        Debug.Log(name + " was loaded.");
        CleanStats();
        _healthMax = healthDefault + _healthBoost;
        _manaMax = manaDefault + _manaBoost;
        _health = _healthMax;
        _mana = _manaMax;
        _defense = defenseDefault;
        _attack = attackDefault;
        isdead = false;


        Skill attack = new Skill(this, "SLASH", 0, 0, 20); //for bad guys: index 0 //damage of 20 // - A weak slashing attack.
        Skill skill_1 = new Skill(this, "PUNCH", 0, 15, 100); //for good guys: index 1 //damage of 100 // - You only need one.
        Skill skill_2 = new Skill(this, "ALL-ATTACK", 0, 30, 3.85f, 0, 0, true); 
        Skill skill_3 = new Skill(this, "CURE", 2, 20, 0, 0, 40); //1.4f
        Skill skill_4 = new Skill(this, "DEF-BUFF", 6, 7, 0, 0, 1.15f); //healp contains how much to buff (mod_per_stacks)
        Skill skill_5 = new Skill(this, "ATT-BUFF", 7, 7, 0, 0, 1.15f); //healp contains how much to buff (mod_per_stacks)
        Skill defend = new Skill(this, "DEFEND", 1, 0, 0, 1.30f);
        Skill run = new Skill(this, "RUN", 4);
        Skill enterSkillList = new Skill(this, "SKILLS", 5);

        skill_1.setDesc("A heavy blow to a target.");
        skill_2.setDesc("A light blow to multiple targets.");
        skill_3.setDesc("Heals a party member for some HP.");
        skill_4.setDesc("Increases a target's defense."); //TODO: "Stacks up to three times. partywide?"
        skill_5.setDesc("Increases a target's attack."); //TODO: "Stacks up to three times. partywide?"

        attack.setDesc("A normal blow to a target.");
        defend.setDesc("Reduces damage taken for the round.");
        run.setDesc("Run away from the encounter.");
        enterSkillList.setDesc("Use a skill.");


        _skillList.Add(skill_1);
        _skillList.Add(skill_2);
        _skillList.Add(skill_3);
        _skillList.Add(skill_4);
        _skillList.Add(skill_5);

        _CommandList.Add(attack);
        _CommandList.Add(enterSkillList);
        _CommandList.Add(defend);
        _CommandList.Add(run);

        // Makes sure that this object isn't destroyed on load.
        // NOTE: This script should persist even if our animations change since our stats shouldn't reset until we want them to.
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    #endregion

    // TODO: This is temporarily here
    #region skill_functions
    // All of this is temporary

    // Returns true if the attacked enemmy dies
    public bool Attack(Ai enemy, BattleAction ba)
    {
        Debug.Log(this.name + " attacks");
        return enemy.GetAttacked(ba.Skill().getDamagepts() * ba.Caster()._attackMult);
    }


    // Returns true if the attacked enemmy dies
    public bool Attack(Ai enemy, Skill s) 
    {
        Debug.Log(name + "used " + s.getName() + "on "+ enemy.name + ".");
        //TODO: Specify damage dealt
        //by accessing how much damage the character did.
        return enemy.GetAttacked(s.getDamagepts() * _attackMult);
    }
    
    // Returns true if the attacked enemmy dies
    public bool Attack(Character enemy, Skill s)
    {
        Debug.Log(name + " used " + s.getName() + " on " + enemy.name + ".");
        return enemy.GetAttacked(s.getDamagepts() * _attackMult);
    }

    /*
    // Returns true if the attacked enemmy dies
    public bool Attack(Ai enemy)
    {
        Debug.Log(name + " attacked " + enemy.name + ".");
        return enemy.GetAttacked(_attack);
    }
    */
    #endregion

    #region health_functions
    public bool GetAttacked(float damage)
    {
        int effectiveDamage = (int)(damage - _defense * _defenseMult);
        if (effectiveDamage <= 0)
        {
            effectiveDamage = 1;
        }
        damagetookrecently = effectiveDamage;

        Debug.Log(this.name + " Damage in GetAttacked: " + damage);
        Debug.Log(this.name + " Defense: " + _defense);
        Debug.Log(this.name + " DefenseMult: " + _defenseMult);
        Debug.Log(this.name + " DamagetookRecently: " + damagetookrecently);

        return TakeDamage(effectiveDamage);
    }

    // This character takes damage
    // Stats are NOT taken into account
    // Returns true upon death
    private bool TakeDamage(float damage)
    {
        //TODO: Note that "take damage" and "die" *might* need to be separate fxns
        //ideally the battle manager will call them (takeDamage() won't call die())
        //this is so that UI HP bars can be updated before the character is "deleted". --Lena
        Debug.Log(this.name + " Damage in TakeDamage: " + damage);
        if (_health - damage <= 0)
        {
            //TODO: Die
            Debug.Log(name + " took " + damage + " damage and died!");
            _health = 0;
            return true;
        }
        else
        {
            _health -= damage;
            Debug.Log(name + " took " + damage + " damage!");
            Debug.Log(name + " has " + _health + " life left.");
            return false;
        }
    }
    
    // Heals by the given AMOUNT
    // Doesn't go past the max health
    public void HealAmount(float amount)
    {
        if (_health + amount > _healthMax)
        {
            _health = _healthMax;
        }
        else
        {
            _health += amount;
        }
    }

    // Heals by adding a PERCENT amount of the max health to the current health
    // Doesn't go past the max health
    public void HealPercent(float percent)
    {
        if (_health + (_healthMax * percent) > _healthMax)
        {
            _health = _healthMax;
        }
        else
        {
            _health += (_healthMax * percent);
        }
    }
    #endregion

    #region stat_functions

    // Removes all boosts/multipliers to health, mana, defense, and attack
    private void CleanStats()
    {
        _healthBoost = 0;
        _manaBoost = 0;
        _defenseBoostStack = 0;
        _attackBoostStack = 0;
        _attackMult = attackMultDefault;
    }
    
    //Updates the stats to their appropriate values: CALL WHEN BUFFER SKILL IS CALLED
    public void UpdateStats(float buffATK = 1.0f, float buffDEF = 1.0f) //parameters come in percent of increase (1.xx)
    {
        if (buffATK != 1.0f)
        {
            _attackBoostStack++;
            _attackMult = _attackBoostStack * buffATK;
            Debug.Log(name + "'s ATTACK POWERED UP to stack: " + _attackBoostStack + " with _attackMult: " + _attackMult);
        }

        if (buffDEF != 1.0f)
        {
            _defenseBoostStack++;
            _defenseMult = _defenseBoostStack * buffDEF;
            Debug.Log(name + "'s DEFENSE POWERED UP to stack: " + _defenseBoostStack + " with _defenseMult: " + _defenseMult);
            Debug.Log(buffDEF);
        }
    }
    #endregion

    #region setter/getter_functions

    public void changeManaBy(float changemana)
    {
        float newVal = _mana - changemana;
        if (newVal >= 0 && newVal <= _manaMax)
        {
            _mana = newVal;
        }        
    }

    public List<Skill> GetSkillList()
    {
        return _skillList;
    }

    public List<Skill> GetCommandList() {
        return _CommandList;
    }

    public Skill GetCommandAt(int i)
    {
        return _CommandList[i];
    }


    public Sprite GetSprite() {
        return sprite;
    }
    #endregion

}
