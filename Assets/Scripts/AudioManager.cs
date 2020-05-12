using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance = null;
    public IDictionary<string, AudioSource> sounds = new Dictionary<string, AudioSource>();

    #region unity_functions

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this.gameObject);
        }
        DontDestroyOnLoad(gameObject);

        AudioClip[] clips = Resources.LoadAll<AudioClip>("Sounds");
        foreach (AudioClip clip in clips)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.clip = clip;
            sounds.Add(clip.name, source);
        }
    }

    #endregion

    #region audio_functions

    private void Play(string soundName)
    {
        AudioSource sound = sounds[soundName];
        if (soundName.Contains("Background"))
        {
            sound.loop = true;
        }

        if (soundName == "Background_overworld")
        {
            sound.volume = 0.3f;
        }
        else if (soundName == "Background_dungeon" || soundName == "Background_battle")
        {
            sound.volume = 0.1f;
        }
        else if (soundName == "enemy_die" || soundName == "rollover" || soundName == "confirmation")
        {
            sound.volume = 0.5f;
        }
        sound.Play();
    }

    public void ResetBackground()
    {
        foreach (KeyValuePair<string, AudioSource> entry in sounds)
        {
            string soundName = entry.Key;
            AudioSource sound = entry.Value;
            if (soundName.Contains("Background"))
            {
                sound.Stop();
            }
        }
    }

    #endregion


    #region Specific sound clips
    //mostly for abstraction so other methods never have to call "play sound clip #22" or anything like that

    public void SFX_advanceText() {
        Play("click");
    }

    public void SFX_menuGoBack()
    {
        Play("back");
    }

    public void SFX_menuScroll()
    {
        Play("rollover");
    }

    public void SFX_menuSelect()
    {
        Play("confirmation");
    }

    public void SFX_playerHurt()
    {
        Play("player_hurt");
    }

    public void SFX_enemyHurt()
    {
        Play("enemy_hurt");
    }

    public void SFX_enemyDie()
    {
        Play("enemy_die");
    }

    public void SFX_checkObject(string item) {
        if (item == "Elder" || item == "Hornless" || item == "Franklin" || item == "SoulBagger")
        {
            Play("huh");
        }
        else if (item == "DungeonSign" || item.Contains("DevNotes"))
        {
            Play("book");
        }
        else if (item == "Chest")
        {
            Play("chest");
        }
        else if (item.Contains("House"))
        {
            Play("door");
        }
    }

    public void Background_dungeon()
    {
        Play("Background_dungeon");
    }

    public void Background_overworld()
    {
        Play("Background_overworld");
    }

    public void Background_battle()
    {
        Play("Background_battle");
    }

    #endregion
}
