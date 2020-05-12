using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
/// <summary>
/// Handles the rising, fading, and self-deletion of the text (damage number) this script is attached to.
/// </summary>
public class DamageNumber : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Reference to damage number's text field")]
    private TextMeshProUGUI numberText;

    [SerializeField]
    [Tooltip("How fast the numbers should fade.")]
    private float fadeSpeed;

    [SerializeField]
    [Tooltip("How long before the numbers start to disappear, independent of animation time.")]
    private float fadeTime;
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update() {
        
        fadeTime -= Time.deltaTime;

        if (fadeTime < 0) { //if it's time to fade, start fading away.
            Color textcolor = numberText.color;
            textcolor.a -= fadeSpeed * Time.deltaTime;
            numberText.color = textcolor;

            if (textcolor.a <= 0) { //if text has become fully transparent (alpha channel is <= 0)
                Destroy(gameObject);
            }
        }
    }
    

    public void setNumberTo(int num) {
        numberText.text = num.ToString();
    }

    public void setNumberColor(Color c) {
        numberText.color = c;
    }
    

}
