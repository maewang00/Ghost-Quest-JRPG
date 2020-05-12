using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
//using UnityEngine.SceneManagement;

public class BattleManager : MonoBehaviour
{

    #region basic_variables
    [Tooltip("Friends: Placeholder for now.")]
    public List<Character> Friends;

    [Tooltip("EnemyAis: Placeholder for now.")]
    public List<Ai> EnemyAis;

    [Tooltip("Reference to the UI manager to yell at for UI updates.")]
    public GameObject UI_manager_reference_object;

    /* NOTE that in Start I yank out the UIManager script
     * and pop it to the UIManager object "UI_man".
     * Call *that*! You can just do UI_man.whatever() instead of
     * UI_manager_reference_object.GetComponent etc... */
    private UIManager UI_man;

    //private AudioManager audioManager;
    #endregion

    #region battle_variables
    private Queue<BattleAction> BattleQueue;
    private int _targetChosen;
    private bool _waitingForInput;
    private bool _choosingEnemyOrFriend;
    private bool _waitingForBattleInput;
    private int _actionChoice;
    private bool notPickTarget;
    private bool _inSkillList;
    private int prevlist; //[0] for commands list, [1] for skills list
    private int prevselect; //previous index in prevlist
    private bool pullBack;
    private Character selectedFriend;
    List<Skill> currList;
    int selectindex;
    bool battling;
    private List<Character> friendsSelfDefended;
    private List<float> prevDEFMult;

    private int numAlive; //Number of allies still alive. Can only decrease, since we have no revive skill.
    private int memIndex;
    #endregion



    /// <summary>
    /// Class Constructor.
    /// </summary>
    /// <param name="friends">List of all allies in the encounter</param>
    /// <param name="enemyAis">List of enemies the player will be fighting</param>
    public BattleManager(List<Character> friends, List<Ai> enemyAis)
    {
        Friends = friends;
        EnemyAis = enemyAis;
        battling = true;
    }


    #region Front end functions [FE_]
    //methods that do simple things for front-end related tasks.

    #region Scene setup (Deploying enemy sprites, setting up player characters)

    private void setScene() {
        foreach (Character c in Friends) {
            UI_man.AddCharacterBox(c);
        }
        UI_man.untargetAllCharacters();
        int index = 0;
        foreach (Ai enemy in EnemyAis) {
            UI_man.AddEnemySprite(enemy.GetSprite(), index);
            index++;
        }
        FE_DisableAllMenus();
    }

    #endregion

    /// <summary>
    /// Does the highlight thing for selecting a target. Make sure to run
    /// UI_man.ResetCommandMenu() after calling this function.
    /// </summary>
    /// <param name="currIndex"></param>
    private void FE_selectEnemy(int currIndex) {
        UI_man.TurnSkillMenuOff();
        UI_man.TurnCommandMenuOn();
        //UI_man.TurnCommandMenuOff();
        //play SFX for chosing the enemy
        //audioManager.SFX_menuScroll();

        //update UI accordingly: show the enemy's name
        UI_man.HideCommandSelectionBox();
        UI_man.ChangeCommandMenuBoldfont("Target:" + '\n' + EnemyAis[currIndex].name);

        //Unhighlights all enemies
        UI_man.UnhighlightAll();

        //Highlights current enemy
        UI_man.HighlightEnemy(currIndex);
    }

    private void FE_selectAllEnemies() {
        //audioManager.SFX_menuScroll();
        UI_man.TurnSkillMenuOff();
        UI_man.TurnCommandMenuOff();
        UI_man.TurnShoutcastOn();
        UI_man.UpdateShoutcast("Target: All enemies");
    }

    private void FE_defeatedEnemy(int enemyIndex) {
        UI_man.RemoveEnemySprite(enemyIndex);
    }

    public void UpdateShoutcastAndVariables(string message, bool waitingForBattleInput, bool choosingEnemy) {
        UI_man.UpdateShoutcast(message);
        _waitingForInput = true;
        _waitingForBattleInput = waitingForBattleInput;
        _choosingEnemyOrFriend = choosingEnemy;
        selectindex = 0;
        UI_man.UpdateAllBars();
    }

    private void FE_NavigateCommandMenu(int index, bool waitingForBattleInput, bool choosingEnemy) {
        //audioManager.SFX_menuScroll();
        UI_man.HighlightCommand(index);
        _waitingForInput = true;
        _waitingForBattleInput = waitingForBattleInput;
        _choosingEnemyOrFriend = choosingEnemy;
        selectindex = 0;
    }

    private void FE_NavigateSkillMenu(int index, bool waitingForBattleInput, bool choosingEnemy) {
        //audioManager.SFX_menuScroll();
        UI_man.HighlightSkill(index);
        _waitingForInput = true;
        _waitingForBattleInput = waitingForBattleInput;
        _choosingEnemyOrFriend = choosingEnemy;
        selectindex = 0;
    }

    private void FE_CommandMenuToSkillMenu(Character c, bool waitingForBattleInput, bool choosingEnemy) {
        UI_man.TurnCommandMenuOff();
        UI_man.TurnSkillMenuOn();
        UI_man.InstantiateSkillList(c);
        UI_man.HighlightSkill(0);
    }

    private void FE_SkillMenuToCommandMenu() {
        UI_man.TurnSkillMenuOff();
        UI_man.TurnCommandMenuOn();
        UI_man.HighlightCommand(0);
    }

    private void FE_SelectFriend(int index) {
        FE_DisableAllMenus();
        UI_man.targetOneCharacter(index);
        //audioManager.SFX_menuScroll();
    }

    private void FE_SelectAllFriends() {
        FE_DisableAllMenus();
        UI_man.targetAllCharacters();
        //audioManager.SFX_menuScroll();
    }

    private void FE_DisableAllMenus() {
        UI_man.TurnShoutcastOff();
        UI_man.TurnSkillMenuOff();
        UI_man.TurnCommandMenuOff();
    }

    private void FE_SelectToCommandMenu() {
        FE_DisableAllMenus();
        UI_man.UnhighlightAll();
        UI_man.untargetAllCharacters();
        UI_man.ResetCommandMenu();
        UI_man.TurnCommandMenuOn();
        UI_man.HighlightCommand(_actionChoice);
    }

    private void FE_SelectToSkillMenu(Character c) {
        FE_DisableAllMenus();
        UI_man.UnhighlightAll();
        UI_man.untargetAllCharacters();
        UI_man.TurnSkillMenuOn();
        UI_man.InstantiateSkillList(c);
        UI_man.HighlightSkill(_actionChoice);
    }

    private void FE_EnemyDoesAnAction(int enemyIndex) {
        UI_man.EnemyMakesAnAction(enemyIndex);
    }

    private void FE_EnemyTakesDamage(int enemyIndex, int damage) {
        UI_man.EnemyTookDamage(enemyIndex, damage);
    }

    private void FE_CharacterTakesDamage(int charaIndex, int damage) {
        UI_man.characterTakesDamage(charaIndex, damage);
    }

    private void FE_CharacterIsHealed(int charaIndex, int healAmount) {
        UI_man.characterIsHealed(charaIndex, healAmount);
    }

    #endregion


    #region unity_functions

    private void Awake()
    {
        GameObject gameManagerObj = GameObject.Find("GameManager");
        EnemyAis = gameManagerObj.GetComponent<GameManager>().EnemyAis;
        
        BattleQueue = new Queue<BattleAction>();
        friendsSelfDefended = new List<Character>();
        prevDEFMult = new List<float>();
        UI_man = UI_manager_reference_object.GetComponent<UIManager>();
        //audioManager = GameObject.FindWithTag("AudioManager").GetComponent<AudioManager>();
    }

    /* Start is called before the first frame update */
    void Start()
    {
        //set up UI:
        setScene();
        numAlive = Friends.Count;

        _waitingForInput = false;
        StartCoroutine(Battle());
    }

    #region reference to skill types
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
    #endregion

    #region organizer_functions

    private void ToPrevList ()
    {
        if (prevlist == 0) //prev is commands list
        {
            currList = selectedFriend.GetCommandList();
            _inSkillList = false;
            selectindex = prevselect;
            Debug.Log("Running0");
            FE_SelectToCommandMenu();
        }
        else if (prevlist == 1) //prev is skills list
        {
            currList = selectedFriend.GetSkillList();
            _inSkillList = true;
            selectindex = prevselect;
            Debug.Log("Running1");
            //TODO: Mae: This case never seems to run? No clue why.
            FE_SelectToSkillMenu(selectedFriend);
        }
    }
    #endregion
    
    /* Update is called once per frame */
    void Update()
    {
///////////////////////////////////////////////////////////Picking Command or Skill///////////////////////////////////////////////////////////
        if (_waitingForInput)
        {
            if (_waitingForBattleInput && !_choosingEnemyOrFriend) //picking skill (up and down)
            {
                //Debug helper, press space.
                if (Input.GetKeyDown(KeyCode.Space)) {
                    Debug.Log("Currently at: " + currList[selectindex].getName() + " with Friend " + selectedFriend.name);
                }

                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    selectindex = (selectindex - 1) % currList.Count;
                    if (selectindex == -1)
                    {
                        selectindex = currList.Count - 1;
                    }
                    Debug.Log("Pressed up: " + currList[selectindex].getName());

                    //audioManager.SFX_menuScroll(); //play SFX
                    UI_man.UpdateShoutcast(currList[selectindex].getName());
                    if (_inSkillList) {
                        UI_man.HighlightSkill(selectindex);
                    }
                    else {
                        UI_man.HighlightCommand(selectindex);
                    }
                }
                else if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    selectindex = (selectindex + 1) % currList.Count;
                    Debug.Log("Pressed down: " + currList[selectindex].getName());

                    //audioManager.SFX_menuScroll(); //play SFX
                    UI_man.UpdateShoutcast(currList[selectindex].getName());
                    if (_inSkillList) {
                        UI_man.HighlightSkill(selectindex);
                    } else {
                        UI_man.HighlightCommand(selectindex);
                    }
                }
                else if (Input.GetKeyDown(KeyCode.X))
                {
                    Debug.Log("Pressed select. Selected skill: " + currList[selectindex].getName());
                    //audioManager.SFX_menuSelect(); //play SFX
                    _actionChoice = selectindex;
                    prevselect = selectindex;
                    prevlist = 0;

                    //////////////////////////////////////////////////////Set vars to NOT PICK TARGET (run, defend, all-ranged)//////////////////////////////////////
                    //no need for picking enemies/friends if you are running, defending self, or doing an all-ranged enemy/friend skill
                    if (currList[selectindex].getName() == "RUN" ||
                        currList[selectindex].getName() == "DEFEND" ||
                        currList[selectindex].getisWide())
                    {
                        notPickTarget = true;
                    }

                    //if picked command is going into skills list, switch currList into skills list
                    if (currList[_actionChoice].getName() == "SKILLS")
                    {
                        currList = selectedFriend.GetSkillList();
                        _inSkillList = true;
                        prevlist = 1;
                        selectindex = 0;
                        FE_CommandMenuToSkillMenu(selectedFriend, true, false);
                    }
                    else { //else execute as normally..

                        Debug.Log(currList[_actionChoice].getMp());
                        Debug.Log(selectedFriend._mana);
                        //do you have enough mana? Then enter.
                        //if (currList[_actionChoice].getMp() <= selectedFriend._mana)
                        //{
                            _waitingForInput = false;
                            _choosingEnemyOrFriend = false;
                            selectindex = 0;
                        //}
                    }
                }

                //in skills list and want to go back to commands
                else if (_inSkillList && Input.GetKeyDown(KeyCode.Z)) {
                    Debug.Log("PULL BACK Z - CODE1");
                    currList = selectedFriend.GetCommandList();
                    _inSkillList = false;
                    FE_SkillMenuToCommandMenu();
                }
                else if (Input.GetKeyDown(KeyCode.Z))
                {
                    Debug.Log("PULL BACK Z - CODE2");
                    pullBack = true;
                    _waitingForInput = false;
                    _choosingEnemyOrFriend = false;
                }
            }
            ///////////////////////////////////////////////////////////Picking enemy or friend///////////////////////////////////////////////////////////
            else if (!_waitingForBattleInput && _choosingEnemyOrFriend) //picking enemy (left and right)
            {
                ///////////////////////////////////////////////////////////Picking friend (Heal or Buff)///////////////////////////////////////////////////////////
                
                if (currList[_actionChoice].getType() == 2 ||
                    currList[_actionChoice].getType() == 6 ||
                    currList[_actionChoice].getType() == 7)
                {
                    if (Input.GetKeyDown(KeyCode.LeftArrow))
                    {
                        selectindex = (selectindex - 1) % Friends.Count;
                        if (selectindex == -1)
                        {
                            selectindex = Friends.Count - 1;
                        }

                        Debug.Log("Pressed left: " + Friends[selectindex].name);
                        FE_SelectFriend(selectindex);
                    }
                    else if (Input.GetKeyDown(KeyCode.RightArrow))
                    {
                        selectindex = (selectindex + 1) % Friends.Count;

                        Debug.Log("Pressed right: " + Friends[selectindex].name);
                        FE_SelectFriend(selectindex);
                    }
                    else if (Input.GetKeyDown(KeyCode.X))
                    {
                        Debug.Log("Pressed select. Selected friend: " + Friends[selectindex].name);
                        //audioManager.SFX_menuSelect(); //play SFX
                        //UI_man.UpdateShoutcast("Selected friend: " + Friends[selectindex].name);

                        UI_man.untargetAllCharacters();
                        _targetChosen = selectindex;
                        _waitingForInput = false;
                        _choosingEnemyOrFriend = false;
                    }
                    else if (Input.GetKeyDown(KeyCode.Z))
                    {
                        Debug.Log("PULL BACK Z - CODE3");
                        _waitingForInput = false;
                        _choosingEnemyOrFriend = false;
                        pullBack = true;
                    }

                }

///////////////////////////////////////////////////////////Picking enemy (Any other command)///////////////////////////////////////////////////////////
                else //pick enemy target
                {
                    if (Input.GetKeyDown(KeyCode.LeftArrow))
                    {
                        selectindex = (selectindex - 1) % EnemyAis.Count;
                        if (selectindex == -1)
                        {
                            selectindex = EnemyAis.Count - 1;
                        }
                        Debug.Log("Pressed left: " + EnemyAis[selectindex].name);

                        //Do frontend stuff (FE)
                        FE_selectEnemy(selectindex);
                    }
                    else if (Input.GetKeyDown(KeyCode.RightArrow))
                    {
                        selectindex = (selectindex + 1) % EnemyAis.Count;

                        Debug.Log("Pressed right: " + EnemyAis[selectindex].name);

                        //Do frontend stuff
                        FE_selectEnemy(selectindex);
                    }
                    else if (Input.GetKeyDown(KeyCode.X))
                    {
                        Debug.Log("Pressed select. Selected enemy: " + EnemyAis[selectindex].name);
                        //audioManager.SFX_menuSelect();
                        UI_man.UnhighlightAll();
                        UI_man.ResetCommandMenu();
                        //UI_man.UpdateShoutcast("Selected target: " + EnemyAis[selectindex].name);

                        _targetChosen = selectindex;
                        Debug.Log("PICKED ENEMY: " + _targetChosen);
                        _waitingForInput = false;
                        _choosingEnemyOrFriend = false;
                    }
                    else if (Input.GetKeyDown(KeyCode.Z))
                    {
                        Debug.Log("PULL BACK Z - CODE4");
                        _waitingForInput = false;
                        _choosingEnemyOrFriend = false;
                        pullBack = true;
                    }

                }

            }

///////////////////////////////////////////////////////////Doing nothing. Just advancing Dialog///////////////////////////////////////////////////////////
            else if (!_waitingForBattleInput)
            { //no selecting, just advancing dialog in UI
                if (Input.GetKeyDown(KeyCode.X))
                {
                    //audioManager.SFX_advanceText();
                    _waitingForInput = false;
                }
            }
        }
        
    }
    #endregion



    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    

    #region MAIN_BATTLE_LOGIC
    /*Player picks actions, then Enemies, then execute all actions*/
    private IEnumerator Battle()
    {
        battling = true;

        UI_man.TurnShoutcastOn();
        _waitingForInput = true;

        if (EnemyAis.Count == 0) {
            UI_man.UpdateShoutcast(EnemyAis[0].name + " appeared!");
        } else {
            UI_man.UpdateShoutcast(EnemyAis[0].name + " and co. appeared!");
        }

        yield return new WaitUntil(() => !_waitingForInput);

        UI_man.TurnCommandMenuOn();
        UI_man.HighlightCommand(0);

        while (battling)
        {
            /////////////////////////////////////////////Player starts choosing actions!/////////////////////////////////////////////
            Debug.Log("PlayerPhase starts");
            UI_man.TurnShoutcastOff();
            int currCharaIndex = 0; //for UI
            int numInParty = Friends.Count;

            while (currCharaIndex < numInParty)
            {
                if (!Friends[currCharaIndex].isdead)
                {
                    Debug.Log(Friends[currCharaIndex].name + "'s turn to choose!");
                    UI_man.nudgeCharacterBoxUp(currCharaIndex); //pushes the character box up to show
                                                                //that you're issuing commands for this character
                    UI_man.TurnCommandMenuOn(); //turn the command menu on (UI)

                    // WARNING _actionChoice is hardcoded in Character.cs and in Shoutcast (line below)
                    notPickTarget = false;
                    //for Update()
                    selectedFriend = Friends[currCharaIndex];
                    currList = selectedFriend.GetCommandList();
                    _inSkillList = false;

                    //Choosing a command:
                    FE_NavigateCommandMenu(0, true, false);
                    yield return new WaitUntil(() => !_waitingForInput); //wait for player to choose an option

                    //Go back to previous friend
                    if (pullBack)
                    {
                        Debug.Log("PULL BACK");
                        if (currCharaIndex > 0) {
                            UI_man.nudgeCharacterBoxDown(currCharaIndex);
                            currCharaIndex--;


                            //Build another Queue and replace!
                            Queue<BattleAction> tempQueue = new Queue<BattleAction>(); //reset battle queue
                            while (BattleQueue.Count != 1) {
                                tempQueue.Enqueue(BattleQueue.Dequeue());
                            }
                            BattleQueue = tempQueue;
                        }
                        pullBack = false;
                    }
                    else
                    {
                        //Player has chosen:
                        Debug.Log("SKILL TYPE IS: " + currList[_actionChoice].getType());

                        //After Picking Skill, Do we pick a target? Friend or Enemy?
                        if (!notPickTarget)
                        {
                            //UpdateShoutcastAndVariables("Press left and right to choose an enemy. Press X to select.", false, true);

                            _waitingForInput = true;
                            _waitingForBattleInput = false;
                            _choosingEnemyOrFriend = true;
                            selectindex = 0;

                            if (currList[_actionChoice].getType() == 2 ||
                                currList[_actionChoice].getType() == 6 ||
                                currList[_actionChoice].getType() == 7)
                            {
                                FE_SelectFriend(0);
                            }
                            else if (currList[_actionChoice].getType() == 0)
                            {
                                FE_selectEnemy(0);
                            }

                            yield return new WaitUntil(() => !_waitingForInput && !_choosingEnemyOrFriend);
                            //Enemy/Friend selection to Menu for selecting skills
                            if (!pullBack)
                            {
                                UI_man.TurnShoutcastOff();
                            }
                        }



                        //Enemy/Friend selection TO Menu for selecting skills
                        if (pullBack)
                        {
                            ToPrevList();
                            pullBack = false;
                        }
                        else
                        {

                            Debug.Log("SKILL TYPE IS NOW: " + currList[_actionChoice].getType());

                            //self defense
                            if (currList[_actionChoice].getName() == "DEFEND")
                            {
                                Debug.Log("want some self-defense");
                                BattleQueue.Enqueue(new BattleAction(Friends[currCharaIndex], Friends[currCharaIndex], currList[_actionChoice]));
                            }

                            //attack or run or all ranged attack
                            else if (currList[_actionChoice].getType() == 0 || currList[_actionChoice].getType() == 4 || currList[_actionChoice].getisWide())
                            {
                                Debug.Log("want some attack/run/ranged attack");
                                BattleQueue.Enqueue(new BattleAction(Friends[currCharaIndex], EnemyAis[_targetChosen], currList[_actionChoice]));
                            }

                            //heal individual or buff individual
                            else
                            {
                                Debug.Log("want some heal/buff");
                                BattleQueue.Enqueue(new BattleAction(Friends[currCharaIndex], Friends[_targetChosen], currList[_actionChoice]));
                            }


                            Debug.Log("Skill added to the BattleQueue!");
                            UI_man.nudgeCharacterBoxDown(currCharaIndex);
                            if (_inSkillList)
                            {
                                FE_SkillMenuToCommandMenu();
                            }

                            currCharaIndex++;
                        }
                    }
                }
            }


            ///////////////////////////////////////////////////Execute all friend actions!///////////////////////////////////////////////////
            //IMPORTANT: In this method, if the all enemies or you RUN,
            //EXIT BATTLE and change scene to "You Lose"/World Scene after UI text immediately)
            Debug.Log("Executing starts");
            UI_man.TurnCommandMenuOff();
            UI_man.TurnSkillMenuOff();
            UI_man.TurnShoutcastOn();
            //While the queue is not empty, continue to update one-by-one
            while (BattleQueue.Count != 0)
            {
                Debug.Log("Battle Queue is at count: " + BattleQueue.Count);
                bool enemyisDead = false;
                BattleAction action = BattleQueue.Dequeue();

                currCharaIndex = Friends.IndexOf(action.Caster());
                UI_man.nudgeCharacterBoxUp(currCharaIndex);

                //TODO: if attack
                if (action.Skill().getType() == 0)
                {
                    //////////////////////////////attack ALL enemies//////////////////////////////
                    if (action.Skill().getisWide()) 
                    {
                        UpdateShoutcastAndVariables(action.Caster().name + " used " + action.Skill().getName() + " on all enemies!", false, false);

                        yield return new WaitUntil(() => !_waitingForInput);

                        int currEnemyIndex = 0;
                        foreach (Ai enemy in EnemyAis.ToList())
                        {
                            enemyisDead = action.Caster().Attack(enemy, action.Skill());

                            //UpdateShoutcastAndVariables(enemy.name + " received " + enemy.damagetookrecently + " damage.", false, false);
                            FE_EnemyTakesDamage(currEnemyIndex, Mathf.RoundToInt(enemy.damagetookrecently));
                            //yield return new WaitUntil(() => !_waitingForInput);
                            yield return new WaitForSeconds(0.2f);

                            //if attacked enemy died
                            if (enemyisDead)
                            {
                                Debug.Log("YEETED OUTTA HERE");
                                //Ai enemyAi = (Ai) action.Target();
                                FE_defeatedEnemy(currEnemyIndex);
                                //UpdateShoutcastAndVariables("Enemy " + enemy.name + " died.", false, false);
                                //yield return new WaitUntil(() => !_waitingForInput);

                                yield return new WaitForSeconds(1f);

                                EnemyAis.Remove(enemy);
                                Destroy(enemy);
                                enemyisDead = false;
                            }

                            //If all enemies are killed/list of enemies is empty, exit the while loop
                            if (EnemyAis.Count == 0)
                            {
                                battling = false;
                                Debug.Log("ENEMIES ALL YEETED OUT OF EXISTENCE");
                                break;
                            }

                            currEnemyIndex++;
                        }
                        yield return new WaitForSeconds(0.25f);


                    }
                    //////////////////////////////attack ONE enemy//////////////////////////////
                    else
                    {
                        Ai enemyAi;
                        //If the enemy you were supposed to hit isn't dead during execution..
                        if (EnemyAis.Contains(action.Target()))
                        {
                            //Friend attacks an enemy! (Is the enemy dead?) --> var enemyisDead
                            enemyisDead = action.Caster().Attack(action.Target(), action.Skill());
                            enemyAi = (Ai)action.Target();
                        }
                        //If the enemy you were supposed to hit died already, defaultly hit the first enemy
                        else
                        {
                            Debug.Log("Target was dead: shifting target");
                            enemyAi = EnemyAis[0];
                            enemyisDead = action.Caster().Attack(enemyAi, action.Skill());
                        }

                        UpdateShoutcastAndVariables(action.Caster().name + " used " + action.Skill().getName() + " on " + enemyAi.name + ".", false, false);
                        yield return new WaitUntil(() => !_waitingForInput);
                        //UpdateShoutcastAndVariables(action.Target().name + " received " + action.Target().damagetookrecently + " damage.", false, false);
                        FE_EnemyTakesDamage(EnemyAis.IndexOf(enemyAi), Mathf.RoundToInt(enemyAi.damagetookrecently)); //show damage numbers
                        //yield return new WaitUntil(() => !_waitingForInput);
                        yield return new WaitForSeconds(1f);

                        //Check if attacked enemy died
                        if (enemyisDead)
                        {
                            Debug.Log("YEETED OUTTA HERE");
                            //Ai enemyAi = EnemyAis[_targetChosen];
                            FE_defeatedEnemy(EnemyAis.IndexOf(enemyAi));
                            //UpdateShoutcastAndVariables("Enemy " + enemyAi.name + " died.", false, false);
                            //yield return new WaitUntil(() => !_waitingForInput);

                            yield return new WaitForSeconds(1f);

                            //TODO: in the future, put these lines in their own method call.
                            EnemyAis.Remove(enemyAi);
                            Destroy(enemyAi);
                            enemyisDead = false;

                            //UI_man.RemoveEnemySprite(_targetChosen);
                        }

                        //If all enemies are killed/list of enemies is empty, exit the while loop
                        if (EnemyAis.Count == 0)
                        {
                            battling = false;
                            Debug.Log("ENEMIES ALL YEETED OUT OF EXISTENCE");
                            break;
                        }
                    }
                }


                //TODO: if defend
                if (action.Skill().getType() == 1)
                {
                    UpdateShoutcastAndVariables(action.Caster().name + " defends.", false, false);
                    yield return new WaitUntil(() => !_waitingForInput);
                    friendsSelfDefended.Add(action.Caster());
                    prevDEFMult.Add(action.Caster()._defenseMult);
                    action.Caster().UpdateStats(1.0f, action.Skill().getDefensepts());
                }

                //TODO: if heal
                if (action.Skill().getType() == 2)
                {

                    Debug.Log("You are healed!");
                    action.Target().HealPercent(action.Skill().getHealpts());

                    UpdateShoutcastAndVariables(action.Caster().name + " heals " + action.Target().name + ".", false, false);
                    FE_CharacterIsHealed(Friends.IndexOf(action.Target()), Mathf.RoundToInt(action.Skill().getHealpts()));
                    yield return new WaitUntil(() => !_waitingForInput);
                }

                if (action.Skill().getType() == 4)
                {
                    BattleQueue = new Queue<BattleAction>(); //reset battle queue
                    UpdateShoutcastAndVariables("Ran away successfully!", false, false);
                    yield return new WaitUntil(() => !_waitingForInput);
                    battling = false;
                    //return to the real world
                    GameObject gm = GameObject.FindWithTag("GameController");
                    gm.GetComponent<GameManager>().DungeonScene();
                    break;
                }

                //TODO: if defend buffer
                if (action.Skill().getType() == 6)
                {
                    Debug.Log(action.Target() + " got stiffer thanks to " + action.Caster() + " (lmao)");
                    action.Target().UpdateStats(1.0f, action.Skill().getHealpts());
                    UpdateShoutcastAndVariables(action.Caster().name + " buffed " + action.Target().name + "'s DEF.", false, false);
                    yield return new WaitUntil(() => !_waitingForInput);
                    UpdateShoutcastAndVariables(action.Target().name + "'s DEF increased by one stage!", false, false);
                    yield return new WaitUntil(() => !_waitingForInput);
                }

                //TODO: if attack buffer
                if (action.Skill().getType() == 7)
                {
                    Debug.Log(action.Target() + " got buff thanks to " + action.Caster());
                    action.Target().UpdateStats(action.Skill().getHealpts(), 1.0f);
                    UpdateShoutcastAndVariables(action.Caster().name + " buffed " + action.Target().name + "'s ATK.", false, false);
                    yield return new WaitUntil(() => !_waitingForInput);
                    UpdateShoutcastAndVariables(action.Target().name + "'s ATK increased by one stage!", false, false);
                    yield return new WaitUntil(() => !_waitingForInput);
                }

                //Update mana usage: assumes that mana can only decrease tho
                action.Caster().changeManaBy(action.Skill().getMp());
                //UI_man.UpdateAllBars();
                UI_man.nudgeCharacterBoxDown(currCharaIndex);
            }

            Debug.Log("I demand i go back to the world");
            //All enemies defeated! Hurrah! Go back to the world!
            if (EnemyAis.Count == 0)
            {
                Debug.Log("going back");
                // TODO: PUT A UI "YOU WIN" TEXT SEQUENCE and gain EXPERIENCE [right here] in the future
                UpdateShoutcastAndVariables("YOU WIN! You sure are strong!", false, false);
                yield return new WaitUntil(() => !_waitingForInput);

                // Shift to World Scene
                GameObject gm = GameObject.FindWithTag("GameController");
                gm.GetComponent<GameManager>().DungeonScene();
            }

            /////////////////////////////////////////////Enemy starts choosing actions!/////////////////////////////////////////////
            Debug.Log("EnemyPhase starts");
            if (battling) {

                foreach (Ai enemy in EnemyAis)
                {
                    //defaulted to "Slash"
                    Character pickedTarget = Friends[Random.Range(0, Friends.Count)];

                    while (pickedTarget.isdead) { //TODO: This is a temporary measure
                        pickedTarget = Friends[Random.Range(0, Friends.Count)];
                    }

                    BattleQueue.Enqueue(new BattleAction(enemy, pickedTarget, enemy.GetCommandAt(0)));
                }

                bool friendisDead = false;
                BattleAction enemyaction;
                while (BattleQueue.Count != 0)
                {
                    enemyaction = BattleQueue.Dequeue();

                    UpdateShoutcastAndVariables("Enemy " + enemyaction.Caster().name + " uses " + enemyaction.Skill().getName() + ".", false, false);
                    FE_EnemyDoesAnAction(EnemyAis.IndexOf(enemyaction.Caster() as Ai));
                    yield return new WaitUntil(() => !_waitingForInput);

                    //If the target has already died, roll RNG until you find a not-dead target.
                    if (enemyaction.Target().isdead && numAlive > 0) {
                        Character pickedTarget = Friends[Random.Range(0, Friends.Count)];
                        while (pickedTarget.isdead) { //TODO: This is a temporary measure
                            pickedTarget = Friends[Random.Range(0, Friends.Count)];
                        }
                        enemyaction = new BattleAction(enemyaction.Caster(), pickedTarget, enemyaction.Caster().GetCommandAt(0));

                    }

                    //Character attackedfriend = enemy.TakeAction(EnemyAis, Friends, enemy.GetSkillAt(0));
                    friendisDead = enemyaction.Caster().Attack(enemyaction.Target(), enemyaction.Skill());

                    UI_man.UpdateBarsFor(enemyaction.Target());
                    //UI_man.characterTakesDamage(enemyaction.Target());

                    FE_CharacterTakesDamage(Friends.IndexOf(enemyaction.Target()), Mathf.RoundToInt(enemyaction.Target().damagetookrecently));
                    //UpdateShoutcastAndVariables(enemyaction.Target().name + " been hit and lost " + enemyaction.Target().damagetookrecently + " HP!", false, false);
                    //yield return new WaitUntil(() => !_waitingForInput);

                    yield return new WaitForSeconds(0.75f);

                    if (friendisDead) {
                        Debug.Log("WTF YO");
                        UpdateShoutcastAndVariables("Friend " + enemyaction.Target().name + " died.", false, false);
                        yield return new WaitUntil(() => !_waitingForInput);
                        enemyaction.Target().isdead = true;
                        //Friends.Remove(enemyaction.Target());
                        //TODO: Make sure friends dying is implemented properly:
                        //Since revives do not exist, this is a hard-coded solution.
                        numAlive--;
                    }

                    //if the entire friend party died
                    if (numAlive == 0)
                    {
                        Debug.Log("Player has lost!");
                        
                        UpdateShoutcastAndVariables("You can no longer fight…", false, false);
                        yield return new WaitUntil(() => !_waitingForInput);
                        //TODO: End the battle
                        battling = false;
                        GameObject gm = GameObject.FindWithTag("GameController");
                        gm.GetComponent<GameManager>().GameOverScene();
                        break;
                    }
                }

                for (int i = 0; i < friendsSelfDefended.Count; i++)
                {
                    friendsSelfDefended[i]._defenseMult = prevDEFMult[i];
                    friendsSelfDefended[i]._defenseBoostStack -= 1;
                }
                friendsSelfDefended.Clear();
                prevDEFMult.Clear();
            }
        }
        yield return null;
    }
    
    #endregion

}


































































#region Angel's code

//////////////////////////BELOW IS ANGEL'S CODE////////////////////////
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
////using UnityEngine.SceneManagement;

//public class BattleManager : MonoBehaviour
//{
//    [SerializeField]
//    [Tooltip("Friends: Placeholder for now.")]
//    public List<Character> Friends;
//    //public List<Character> Friends;
//    [SerializeField]
//    [Tooltip("EnemyAis: Placeholder for now.")]
//    public List<Ai> EnemyAis;
//    //public readonly List<Ai> EnemyAis;

//    [SerializeField]
//    [Tooltip("Reference to the UI manager to yell at for UI updates.")]
//    private GameObject UI_manager_reference_object;
//    /* NOTE that in Start I yank out the UIManager script
//     * and pop it to the UIManager object "UI_man".
//     * Call *that*! You can just do UI_man.whatever() instead of
//     * UI_manager_reference_object.GetComponent blah blah blah
//     * This is just here so that I can drag the manager into the inspector
//     * and we don't have to go hunting for the thing every time the battle scene loads */

//    private UIManager UI_man;

//    public BattleManager(List<Character> friends, List<Ai> enemyAis)
//    {
//        Friends = friends;
//        EnemyAis = enemyAis;
//    }

//    #region variables
//    private int _targetChosen;
//    private bool _waitingForInput;
//    #endregion

//    #region unity_functions
//    // Start is called before the first frame update
//    void Start()
//    {
//        UI_man = UI_manager_reference_object.GetComponent<UIManager>();
//        _waitingForInput = false;
//        Invoke("StartBattle", 1);
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        if (_waitingForInput)
//        {
//            if (Input.GetKeyDown(KeyCode.Space))
//            {
//                _targetChosen = 0;
//                _waitingForInput = false;
//            }
//        }
//    }
//    #endregion

//    private void StartBattle()
//    {
//        StartCoroutine(Battle());
//    }

//    private IEnumerator Battle()
//    {
//        //yield return new WaitForSeconds(1);
//        bool isDead = false;
//        bool loop = true;
//        while (loop)
//        {
//            //TODO: In the future give the enemy the option to choose order
//            //TODO: also, may want to make "Player Phase" and "Enemy Phase" methods.
//            //Since they'll never intertwine with each other
//            //(players should always go before enemies,
//            //and all players will act before the enemy will),
//            //it'll make legibility a little easier. --Lena

//            //CHANGES: Changed turn order to Player -> Enemy

//            //TODO: In the future give the player the option to choose order
//            foreach (Character friend in Friends)
//            {
//                Debug.Log("Press Space to Attack...");
//                UI_man.UpdateShoutcast("Press space to attack…");

//                _waitingForInput = true;
//                yield return new WaitUntil(() => !_waitingForInput);

//                //Debug.Log("Finished waiting.");
//                // TODO: Currently only set to attack Character scripts, NOT Ai scripts.
//                // If the chosen enemy is killed
//                UI_man.UpdateShoutcast(friend.name + " attacked "
//                    + EnemyAis[_targetChosen].name + "!");

//                _waitingForInput = true;
//                yield return new WaitUntil(() => !_waitingForInput);
//                //TODO: in the UpdateShoutcast method:
//                //make it so that the player has to press a button to continue the shoutcast --Lena

//                isDead = friend.Attack(EnemyAis[_targetChosen]);
//                UI_man.UpdateShoutcast("Dished out some damage!");
//                //TODO: Specify damage dealt
//                //by accessing how much damage the character did.
//                _waitingForInput = true;
//                yield return new WaitUntil(() => !_waitingForInput);

//                if (isDead)
//                {

//                    Ai enemyAi = EnemyAis[_targetChosen];
//                    UI_man.UpdateShoutcast(enemyAi.name + " has been defeated!");
//                    EnemyAis.Remove(enemyAi);
//                    Destroy(enemyAi);
//                }
//            }

//            UI_man.UpdateShoutcast("It's the enemy's turn…");
//            _waitingForInput = true;
//            yield return new WaitUntil(() => !_waitingForInput);
//            //in the future, I'll probably put this into the UpdateShoutcast fxn.
//            //for now this is good enough.

//            foreach (Ai enemy in EnemyAis) {
//                if (Friends.Count == 0) {
//                    Debug.Log("Player has lost!");
//                    UI_man.UpdateShoutcast("You can no longer fight…");
//                    //TODO: End the battle
//                    loop = false;
//                    break;
//                }

//                UI_man.UpdateShoutcast("Enemy " + enemy.name + " attacked!");
//                _waitingForInput = true;
//                yield return new WaitUntil(() => !_waitingForInput);

//                Character being = enemy.TakeAction(EnemyAis, Friends);
//                UI_man.UpdateShoutcast("You've been hit!");
//                UI_man.UpdateBarsFor(Friends[0]);

//                _waitingForInput = true;
//                yield return new WaitUntil(() => !_waitingForInput);

//                if (being != null) {
//                    Friends.Remove(being);
//                }
//            }

//        }
//        yield return null;
//    }

//    /*
//    private IEnumerator waitASec()
//    {
//        yield return new WaitForSecondsRealtime(1);
//    }
//    */

//    private IEnumerator WaitForChoice()
//    {
//        bool chose = false;
//        while (!chose)
//        {
//            if (Input.GetKeyDown(KeyCode.Space))
//            {
//                chose = true;
//                _targetChosen = 0;
//            }
//            yield return new WaitForEndOfFrame();
//        }
//    }

//}


//////////OLD Battle with 3 coroutines//////////
//private void Battle()
//{
//    //TODO: (BUG) Can't seem to access bool true in class's battling..
//    // so I forced it to be true in function scope: if you can fix, lemme know
//    battling = true;

//    int i = 0;
//    while (i != 3)
//    {
//        //Player starts choosing actions!
//        Debug.Log("PlayerPhase starts");
//        StartCoroutine(PlayerPhase());

//        //In the future, I'll probably put this into the UpdateShoutcast fxn. For now this is good enough.
//        // UPDATE: commented out for now since I turned Battle() into a regular method and not a coroutine
//        // Can fix back in later -Mae :'O
//        //UI_man.UpdateShoutcast("It's the enemy's turn…");
//        //_waitingForInput = true;
//        //yield return new WaitUntil(() => !_waitingForInput);

//        //Enemy starts choosing actions!
//        Debug.Log("EnemyPhase starts");
//        StartCoroutine(EnemyPhase());

//        //Debug.Log("BYE THERE");

//        //Execute all actions!
//        //IMPORTANT: In this method, if the entire party dies or RUNS,
//        //EXIT BATTLE and change scene to "You Lose"/World Scene after UI text immediately)
//        Debug.Log("Executing starts");
//        StartCoroutine(ExecuteActions());

//        //If all enemies are killed/list of enemies is empty, exit the while loop
//        if (EnemyAis.Capacity == 0)
//        {
//            battling = false;
//        }

//        i++;
//    }
//    // TODO: PUT A UI "YOU WIN" TEXT SEQUENCE and gain EXPERIENCE [right here] in the future
//    // TODO: Shift to World Scene [right here]!
//}

#endregion