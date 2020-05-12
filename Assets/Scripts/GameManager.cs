using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;
    private Vector3 prevPos;
    private bool battle = false;
    private bool fromDungeon = false;
    private AudioManager audioManager;

    [SerializeField]
    private List<EnemyTeam> teams;

    [HideInInspector]
    public List<Ai> EnemyAis;

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

        audioManager = GameObject.FindWithTag("AudioManager").GetComponent<AudioManager>();
    }

    void Update()
    {
        /*if (SceneManager.GetActiveScene().name == "TitleScene")
        {
            if (Input.GetButtonDown("joystick button 1") || Input.GetKeyDown(KeyCode.X)) {
                OverworldScene();
            }
        }
        else*/ if (SceneManager.GetActiveScene().name == "GameOverScene")
        {
            if (Input.GetButtonDown("joystick button 1") || Input.GetKeyDown(KeyCode.X)) {
                TitleScene();
            }
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    #endregion

    #region scene_transitions

    public void TitleScene()
    {
        SceneManager.LoadScene("TitleScene");
    }

    public void BattleScene(Vector3 pos)
    {
        prevPos = pos;
        battle = true;

        //ADD ENEMY TEAM
        //In the future would select a random team based on prevPos, but for now, just does so completely randomly. 
        Debug.Log("BATTLE MODE: ADDING CREATURES");
        foreach (Ai e in EnemyAis) {
            Destroy(e);
        }
        EnemyAis.Clear();
        int index = UnityEngine.Random.Range(0, teams.Count);
        Ai currentAi;
        foreach(CreatureType creature in teams[index].Enemies)
        {
            currentAi = gameObject.AddComponent<Ai>();
            currentAi.copyValues(creature);
            EnemyAis.Add(currentAi);
        }
        SceneManager.LoadScene("Battle");
    }

    private void BattleManager(List<Ai> enemyAis)
    {
        throw new NotImplementedException();
    }

    public void OverworldScene()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void DungeonScene()
    {
        SceneManager.LoadScene("DungeonScene");
    }

    public void GameOverScene()
    {
        SceneManager.LoadScene("GameOverScene");
    }

    public void WinScene()
    {
        SceneManager.LoadScene("WinScene");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        audioManager.ResetBackground();
        if (scene.name == "DungeonScene")
        {
            audioManager.Background_dungeon();
            if (!fromDungeon)
            {
                fromDungeon = true;
            }
            else if (battle)
            {
                GameObject gm = GameObject.Find("Player");
                Vector3 currPos = gm.transform.position;
                gm.transform.position = prevPos;
                battle = false;
            }
        }
        else if (scene.name == "SampleScene")
        {
            audioManager.Background_overworld();
            if (fromDungeon)
            {
                GameObject gm = GameObject.Find("Player");
                gm.transform.position = new Vector3(15, -12);
                fromDungeon = false;
            }
        }
        else if (scene.name == "Battle")
        {
            audioManager.Background_battle();
            
        }
        else if (scene.name == "TitleScene")
        {
            battle = false;
            fromDungeon = false;
        }
    }

    #endregion
}
 