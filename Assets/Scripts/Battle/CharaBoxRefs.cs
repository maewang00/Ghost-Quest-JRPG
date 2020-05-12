using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharaBoxRefs : MonoBehaviour
{

    #region References
    public Text name;

    [SerializeField]
    [Tooltip("Reference to the HP bar slider")]
    private Slider HP_Bar;
    [SerializeField]
    [Tooltip("Reference to the SP bar slider")]
    private Slider MP_Bar;

    [SerializeField]
    [Tooltip("Reference to this box's panel, so it can be color-changed")]
    private Image boxBG;

    [SerializeField]
    [Tooltip("Reference to the 'pointer' image for this box (when being selected)")]
    private Image pointer;

    [SerializeField]
    [Tooltip("Reference to the number for HP")]
    private Text HP_num_txt;

    [SerializeField]
    [Tooltip("Reference to the number for SP")]
    private Text MP_num_txt;

    [SerializeField]
    [Tooltip("For Debug purposes.")]
    private Animator charaboxAnimator;
    #endregion

    private void Awake() {
        charaboxAnimator = this.GetComponent<Animator>();
    }

    #region Highlight and selection functions

    public void highlightMe() {
        boxBG.color = Color.white;
    }

    public void shadowMe() {
        boxBG.color = Color.grey;
    }

    public void selectMe() {
        pointer.enabled = true;
    }

    public void unselectMe() {
        pointer.enabled = false;
    }

    public void changeHP_Bar(float newHP, float totalHP) {
        HP_Bar.value = newHP / totalHP;
        HP_num_txt.text = newHP.ToString() + "/" + totalHP.ToString();
    }

    public void changeMP_Bar(float newSP, float totalSP) {
        MP_Bar.value = newSP / totalSP;
        MP_num_txt.text = newSP.ToString() + "/" + totalSP.ToString();
    }

    public void UpdateBothBars(Character c) {
        changeHP_Bar(c._health, c._healthMax);
        changeMP_Bar(c._mana, c._manaMax);
    }

    public void nudgeUp() {
        this.transform.position = new Vector2(transform.position.x, transform.position.y + 20f);
        //Debug.Log("Setting bool");
        charaboxAnimator.SetBool("MyTurn", true);
    }

    public void nudgeDown() {
        this.transform.position = new Vector2(transform.position.x, transform.position.y - 20f);
        charaboxAnimator.SetBool("MyTurn", false);
    }

    #endregion

    #region Animations

    public void takeDamage() {
        charaboxAnimator.SetTrigger("TakeDamage");
    }

    #endregion
}
