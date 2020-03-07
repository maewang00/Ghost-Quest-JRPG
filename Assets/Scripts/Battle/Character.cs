using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    #region base_stats
    [SerializeField]
    [Tooltip("This character's default health amount.")]
    private float healthDefault;
    
    [SerializeField]
    [Tooltip("This character's default mana amount.")]
    private float manaDefault;
    
    [SerializeField]
    [Tooltip("This character's default defense stat. It is a multiplier value between 0 and 1.")]
    private float defenseDefault;
    
    [SerializeField]
    [Tooltip("This character's default attack stat. It is a multiplier value between 0 and 1.")]
    private float attackDefault;
    #endregion

    #region current_stats
    private float _health;
    private float _mana;
    private float _defense;
    private float _attack;
    private float _healthMax;
    private float _manaMax;
    #endregion
    
    #region modifiers
    private float _healthBoost;
    private float _manaBoost;
    private float _defenseBoost;
    private float _attackBoost;
    #endregion
    
    #region class_specifics
    //private readonly List<> _skillsList;
    #endregion
    
    // Start is called before the first frame update
    void Start()
    {
        RemoveBoosts();
        _healthMax = healthDefault + _healthBoost;
        _manaMax = manaDefault + _manaBoost;
        _health = _healthMax;
        _mana = _manaMax;
        RestoreStats();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Heals by the given AMOUNT
    // Doesn't go past the max health
    private void HealAmount(float amount)
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
    private void HealPercent(float percent)
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

    // Removes all boosts to health, mana, defense, and attack
    private void RemoveBoosts()
    {
        _healthBoost = 0;
        _manaBoost = 0;
        _defenseBoost = 0;
        _attackBoost = 0;   
    }
    
    // Restores the attack and defense stats to their full values
    private void RestoreStats()
    {
        //TODO: Make sure the total doesn't equal or pass 1.
        _defense = defenseDefault + _defenseBoost;
        _attack = attackDefault + _attackBoost;
    }
    
}
