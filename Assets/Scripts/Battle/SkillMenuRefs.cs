using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillMenuRefs : MonoBehaviour
{
    public SkillMenuRefs(string description) {
        skillDesc = description;
    }
    
    [SerializeField]
    [Tooltip("Reference to skill icon")]
    private Image skillIcon;

    [SerializeField]
    [Tooltip("Reference to pointer image")]
    private Image pointer;
    [SerializeField]
    [Tooltip("Reference to Text for skill name")]
    private TextMeshProUGUI skillName;
    [SerializeField]
    [Tooltip("Reference to SP cost field")]
    private TextMeshProUGUI manaCost;

    private string skillDesc;

    #region Setters

    public void setSkillIconTo(Sprite icon) {
        skillIcon.sprite = icon;
    }

    public void turnPointerOn() {
        pointer.enabled = true;
    }

    public void turnPointerOff() {
        pointer.enabled = false;
    }

    public void setSPCostTo(float skillCost) {
        manaCost.text = skillCost + " SP";
    }

    public void setSkillNameTo(string name) {
        skillName.text = name;
    }

    public void setSkillDescTo(string desc) {
        skillDesc = desc;
    }

    #endregion

    #region Getters

    public string getSkillName() {
        return skillName.text;
    }

    public string getSkillDesc() {
        return skillDesc;
    }

    #endregion

}
