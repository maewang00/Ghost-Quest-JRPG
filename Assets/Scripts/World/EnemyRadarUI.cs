using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyRadarUI : MonoBehaviour
{
    #region [Dungeon] Enemy radar stuff

    [SerializeField]
    [Tooltip("Reference to the battle narrator text/shoutcaster")]
    private Image radarImage;

    [SerializeField]
    private Sprite[] radarTiers = new Sprite[4];

    private int maxRadarValue = 100;

    public void updateRadar(int value) {

        if (value <= 25) {
            radarImage.sprite = radarTiers[0];
        } else if (value <= 55) {
            radarImage.sprite = radarTiers[1];
        }
        else if (value <= 75) {
            radarImage.sprite = radarTiers[2];
        } else {
            radarImage.sprite = radarTiers[3];
        }

    }

    #endregion
}
