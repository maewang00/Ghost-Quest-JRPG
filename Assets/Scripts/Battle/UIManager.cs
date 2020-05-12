using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {
    /*
     * ABOUT: 
     * Might be a bit redundant to say so,
     * but the UI manager deals with updating the UI.
     * Specifically, stuff like HP/SP bars, the shoutcast,
     * enemy sprites, and character info (Name + HP/SP bars).
     * Ideally, methods can just call the methods of this script
     * to make UI updates a little easier.
     * (Hooray for abstraction!)
     */

    #region References to UI fields
    [SerializeField]
    [Tooltip("Reference to the battle narrator text/shoutcaster")]
    private Text shoutcastString;

    [SerializeField]
    [Tooltip("Reference to the shoutcast text box panel (the whole thing)")]
    private GameObject shoutcastBox;

    [SerializeField]
    [Tooltip("Reference to the area where enemy sprites are instantiated")]
    private HorizontalLayoutGroup enemySpriteArea;

    [SerializeField]
    [Tooltip("Reference to area where player character boxes are instantiated")]
    private HorizontalLayoutGroup playerBoxArea;

    [SerializeField]
    [Tooltip("Reference to the stat box UI prefab that player characters are represented by")]
    private GameObject characterBoxPrefab;

    [SerializeField]
    [Tooltip("Reference to the prefab that enemy characters are represented by")]
    private GameObject enemySpritePrefab;

    #endregion



    #region Other vars
    private bool _waitingForInput;

    private List<GameObject> enemySpriteList;
    private List<int> enemySpriteIDList;
    private List<Character> characterList;
    private List<CharaBoxRefs> characterStatBoxList;
    #endregion


    #region Unity fxns
    // Called before the first frame update
    void Start() {
        enemySpriteList = new List<GameObject>();
        enemySpriteIDList = new List<int>();
        characterList = new List<Character>();
        characterStatBoxList = new List<CharaBoxRefs>();
        locations = new List<GameObject>();
    }

    // Called once per frame
    void Update() {

    }
    #endregion


    #region Changing fields/variables/elements, updating text

    /// <summary>
    /// Updates the HP bar of the specificed character.
    /// </summary>
    /// <param name="c">Character c: Character to update bar for</param>
    public void UpdateBarsFor(Character c) {
        Debug.Log("UI: Updating HP bar for " + c.name + " to " + c._health / c._healthMax + "%");
        int index = characterList.IndexOf(c);
        UpdateBarsFor(index);
    }

    /// <summary>
    /// Updates bar for the passed-in character index. (Zero indexed)
    /// </summary>
    /// <param name="characterIndex">index of character to update (zero indexed)</param>
    public void UpdateBarsFor(int characterIndex) {
        Debug.Log("UI: Updating bars for character " + characterIndex);
        CharaBoxRefs cbox = characterStatBoxList[characterIndex].GetComponent<CharaBoxRefs>();
        cbox.UpdateBothBars(characterList[characterIndex]);
    }

    /// <summary>
    /// Updates all bars for all characters.
    /// </summary>
    public void UpdateAllBars() {
        int index = 0;
        foreach (Character c in characterList) {
            UpdateBarsFor(index);
            index++;
        }
    }


    #region Changing colors of UI objects
    /// <summary>
    /// Highlights the selected enemy.
    /// </summary>
    /// <param name="index"></param>
    public void HighlightEnemy(int index) {
        enemySpriteList[index].GetComponent<Image>().color = new Color(1f, 0.5f, 0.5f);
    }

    /// <summary>
    /// Unhighlights the selected enemy.
    /// </summary>
    /// <param name="index"></param>
    public void UnhighlightEnemy(int index) {
        enemySpriteList[index].GetComponent<Image>().color = new Color(1f, 1f, 1f);
    }

    /// <summary>
    /// Unhighlights all enemy sprites.
    /// </summary>
    public void UnhighlightAll() {
        foreach (GameObject enemysprite in enemySpriteList) {
            enemysprite.GetComponent<Image>().color = new Color(1f, 1f, 1f);
        }
    }
    #endregion

    #region character box stuff (selecting, animating)

    public void targetOneCharacter(int index) {
        untargetAllCharacters();
        characterStatBoxList[index].selectMe();
    }

    public void targetAllCharacters() {
        foreach (CharaBoxRefs cbox in characterStatBoxList) {
            cbox.selectMe();
        }
    }

    public void untargetAllCharacters() {
        foreach (CharaBoxRefs cbox in characterStatBoxList) {
            cbox.unselectMe();
        }
    }

    public void nudgeCharacterBoxUp(int index) {
        Debug.Log("UI: Nudging " + characterStatBoxList[index].name.text);
        characterStatBoxList[index].nudgeUp();
    }

    public void nudgeCharacterBoxDown(int index) {
        characterStatBoxList[index].nudgeDown();
    }

    public void characterTakesDamage(Character c, int damage) {
        int index = characterList.IndexOf(c);
        characterTakesDamage(index, damage);
    }

    public void characterTakesDamage(int index, int damage) {
        characterStatBoxList[index].takeDamage();
        createDamageNumber(locations[index], damage);
    }

    public void characterIsHealed(int index, int healAmount) {
        createHealNumber(locations[index], healAmount);
    }
    

    #endregion

    #endregion

    #region Instantiating and deleting UI elements
    /*
     * TODO: In the future, we'll probably wanna link enemy sprites to gameobjects.
     */

    /// <summary>
    /// Adds the passed in enemy sprite to the battle scene. The ID field is so
    /// the UI manager knows which enemy sprite to remove in the future.
    /// It's fine to just number it in the order of instantiation (ex. 0, 1, 2):
    /// just make sure it's unique.
    /// Also stores the sprite in enemySpriteList for future reference.
    /// </summary>
    /// <param name="enemySprite"></param>
    /// <param name="ID"></param>
    public void AddEnemySprite(Sprite enemySprite, int ID) {
        Debug.Log("UI: Loading Enemy Sprite #" + ID);
        enemySpriteIDList.Add(ID);
        GameObject esprite = Instantiate(enemySpritePrefab, enemySpriteArea.transform);
        esprite.GetComponent<Image>().sprite = enemySprite;
        enemySpriteList.Add(esprite);
    }

    /// <summary>
    /// Removes the enemy sprite that has the passed in ID. 
    /// The ID is the same int that was passed in on enemy sprite instantiation
    ///  (typically just the index of which one it is (sprite 2, etc.).)
    /// </summary>
    /// <param name="ID"></param>
    public void RemoveEnemySprite(int ID) {
        Debug.Log("UI: Removing Enemy Sprite #" + ID);
        StartCoroutine(enemyspriteFadeOut(ID, enemySpriteList[ID].GetComponent<Image>()));
    }

    [SerializeField]
    [Tooltip("Speed at which to fade enemy sprites")]
    private float fadeSpeed;
    private IEnumerator enemyspriteFadeOut(int ID, Image fadeMe) {
        Color startColor = fadeMe.color;
        float aniTime = 0;
        while (aniTime < 1) {
            aniTime += Time.deltaTime * fadeSpeed;
            fadeMe.color = Color.Lerp(startColor, Color.clear, aniTime);
            yield return null;
        }

        Destroy(enemySpriteList[ID]);
        enemySpriteIDList.RemoveAt(ID);
        enemySpriteList.RemoveAt(ID);
    }


    private List<GameObject> locations;
    /// <summary>
    /// Adds a character box for the passed in character
    /// and establishes a link between character boxes and character script objects.
    /// </summary>
    /// <param name="c"></param>
    public void AddCharacterBox(Character c) {
        Debug.Log("UI: Loading character box for " + c.name);
        GameObject charabox = Instantiate(characterBoxPrefab, playerBoxArea.transform);
        CharaBoxRefs box = charabox.GetComponent<CharaBoxRefs>();
        characterStatBoxList.Add(box);
        characterList.Add(c);
        locations.Add(charabox);

        box.name.text = c.name;
        box.UpdateBothBars(c);

        RepositionCharacterBoxes();
    }

    private void RepositionCharacterBoxes() {
        int instantiatePoint = characterList.Count;
        instantiatePoint *= 2;

        //calculate where to position:

        Vector2 dimension = playerBoxArea.GetComponent<RectTransform>().anchorMax;

        dimension.x -= playerBoxArea.GetComponent<RectTransform>().anchorMin.x;
        dimension.y -= playerBoxArea.GetComponent<RectTransform>().anchorMin.y;

        float instantiateDistance = dimension.x / instantiatePoint;
        Vector2 newpos = playerBoxArea.GetComponent<RectTransform>().anchorMin;
        newpos.x += instantiateDistance;

        foreach (GameObject box in locations) {            
            box.GetComponent<RectTransform>().anchorMin = newpos;
            box.GetComponent<RectTransform>().anchorMax = newpos;
            newpos.x += instantiateDistance * 2;
        }
    }

    [SerializeField]
    [Tooltip("Reference to the Damage Number prefab")]
    private GameObject damagenumberPrefab;
    /// <summary>
    /// Creates a damage number at the passed-in position that displays the passed-in damage.
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="damage"></param>
    private void createDamageNumber(GameObject parent, int damage) {
        GameObject dmgNum = Instantiate(damagenumberPrefab, parent.transform);
        dmgNum.GetComponent<DamageNumber>().setNumberTo(damage);
    }

    private void createHealNumber(GameObject parent, int healAmount) {
        GameObject dmgNum = Instantiate(damagenumberPrefab, parent.transform);
        dmgNum.GetComponent<DamageNumber>().setNumberTo(healAmount);
        dmgNum.GetComponent<DamageNumber>().setNumberColor(Color.green);
    }

    #region Shoutcast UI

    /// <summary>
    /// Updates the text in the shoutcast to the passed in parameter.
    /// Does not guarantee that the shoutcast is active: be sure to
    /// turn it on before calling this method.
    /// </summary>
    /// <param name="message">what you want the shoutcast to say</param>
    public void UpdateShoutcast(string message) {
        shoutcastString.text = message;
    }

    /// <summary>
    /// Turns the shoutcast on.
    /// </summary>
    public void TurnShoutcastOn() {
        shoutcastBox.SetActive(true);
    }

    /// <summary>
    /// Turns the shoutcast off.
    /// </summary>
    public void TurnShoutcastOff() {
        shoutcastBox.SetActive(false);
    }

    #endregion


    #region Enemy sprite "animations"

    [SerializeField]
    [Tooltip("Reference to the opaque white mask to color a sprite white")]
    private Material whiteMaterial;

    private Material defaultMaterial;

    /// <summary>
    /// Makes the passed-in corresponding enemy sprite flash white twice,
    /// to show that it's performing an action.
    /// </summary>
    /// <param name="ID"></param>
    public void EnemyMakesAnAction(int ID) {
        Image spritepic = enemySpriteList[ID].GetComponent<Image>();
        
        StartCoroutine(flashSpriteWhiteXTimes(spritepic, 1));
    }

    private IEnumerator flashSpriteWhiteXTimes(Image sprite, int timesToFlash) {
        defaultMaterial = sprite.material;

        while (timesToFlash >= 0) {
            sprite.material = whiteMaterial;
            yield return new WaitForSeconds(0.15f);

            sprite.material = defaultMaterial;
            yield return new WaitForSeconds(0.15f);

            timesToFlash--;
        }
    }


    public void EnemyTookDamage(int ID, int damage) {
        createDamageNumber(enemySpriteList[ID], damage);
        StartCoroutine(flashSpriteWhiteXTimes(enemySpriteList[ID].GetComponent<Image>(), 0));
    }


    #endregion

    #region Command menu UI

    [SerializeField]
    [Tooltip("Reference to the command menu set (the Canvas Group GameObject")]
    private GameObject commandMenus;

    [SerializeField]
    [Tooltip("Reference to the 'Pick a command' text")]
    private Text commandHeader;

    [SerializeField]
    [Tooltip("Reference to the list of options panel")]
    private GameObject commandList;

    [SerializeField]
    [Tooltip("Reference to command menu options")]
    private SkillMenuRefs[] commandOptions;

    [SerializeField]
    [Tooltip("The default command header text")]
    private string defaultHeader = "Pick a command.";

    /// <summary>
    /// Highlights the passed-in command option (0 indexed)
    /// </summary>
    /// <param name="index">(0 - Attack, 1 - Defend, 2 - Heal, 3 - Run)</param>
    public void HighlightCommand(int index) {
        unhighlightAllCommands();
        if (index >= commandOptions.Length) {
            index = 0;
        }
        commandOptions[index].turnPointerOn();
    }

    private void unhighlightAllCommands() {
        foreach (SkillMenuRefs smr in commandOptions) {
            smr.turnPointerOff();
        }
    }

    /// <summary>
    /// Turns the command menu UI on.
    /// </summary>
    public void TurnCommandMenuOn() {
        commandMenus.SetActive(true);
    }

    /// <summary>
    /// Turns the command menu UI off.
    /// </summary>
    public void TurnCommandMenuOff() {
        commandMenus.SetActive(false);
    }

    public void ChangeCommandMenuBoldfont(string header) {
        commandHeader.text = header;
    }

    public void ResetCommandMenu() {
        commandHeader.text = defaultHeader;
        ShowCommandSelectBox();
    }

    public void HideCommandSelectionBox() {
        commandList.SetActive(false);
    }

    public void ShowCommandSelectBox() {
        commandList.SetActive(true);
    }

    #endregion

    #region Skill UI Handling

    [SerializeField]
    [Tooltip("Reference to the skill command menu set (The Canvas Group GameObject)")]
    private GameObject skillSelectionMenus;

    [SerializeField]
    [Tooltip("Reference to the skill panel")]
    private GameObject skillPanel;

    [SerializeField]
    [Tooltip("Reference to the vertical layer group for skill listing")]
    private GameObject skillListArea;

    [SerializeField]
    [Tooltip("Reference to the skill description area")]
    private Text skillDescription;

    [SerializeField]
    [Tooltip("Reference to the skill name area")]
    private Text skillName;

    [SerializeField]
    [Tooltip("List of skill icons")]
    private Sprite[] skillIcons;

    [SerializeField]
    [Tooltip("Skill list entry prefab")]
    private GameObject skillListEntryPrefab;

    private List<SkillMenuRefs> skillListEntries;
    private List<GameObject> skillListPrefabs;

    private bool _isOn = false;

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

    /// <summary>
    /// Turns the skill menu UI on.
    /// </summary>
    public void TurnSkillMenuOn() {
        _isOn = true;
        skillSelectionMenus.SetActive(true);

    }

    /// <summary>
    /// Turns the skill menu UI off.
    /// </summary>
    public void TurnSkillMenuOff() {
        if (_isOn) {
            foreach (GameObject g in skillListPrefabs) {
                Destroy(g);
            }
        }
        _isOn = false;
        skillSelectionMenus.SetActive(false);
    }

    /// <summary>
    /// Shows the skill list for the passed in character (UI).
    /// Handles instantiation and setup but not navigation.
    /// </summary>
    /// <param name="c"> Character to display skill info for </param>
    public void InstantiateSkillList(Character c) {
        //turn the skill panel on
        skillPanel.SetActive(true);
        //set up the skill list entry list object
        skillListEntries = new List<SkillMenuRefs>();
        skillListPrefabs = new List<GameObject>();

        //instantiate entries
        for (int i = 0; i < c.GetSkillList().Count; i++) {
            Skill skl = c.GetSkillList()[i];
            GameObject listObject = Instantiate(skillListEntryPrefab, skillListArea.transform);
            SkillMenuRefs ticker = listObject.GetComponent<SkillMenuRefs>();

            //set all fields
            ticker.setSkillIconTo(skillIcons[skl.getType()]);
            ticker.setSkillNameTo(skl.getName());
            ticker.setSPCostTo(skl.getMp());
            ticker.setSkillDescTo(skl.getDescription());

            //add to entry reference object
            skillListEntries.Add(ticker);
            skillListPrefabs.Add(listObject);
        }
    }
    /// <summary>
    /// Displays the little pointer finger next to the skill ticker
    /// to show that the skill is being pointed at, and
    /// updates the description/skill name boxes accordingly.
    /// </summary>
    /// <param name="num"> index of skill being pointed at </param>
    public void HighlightSkill(int num) {
        turnAllSkillPointersOff();
        skillListEntries[num].turnPointerOn();

        skillName.text = skillListEntries[num].getSkillName();
        skillDescription.text = skillListEntries[num].getSkillDesc();
    }

    private void turnAllSkillPointersOff() {
        foreach (SkillMenuRefs ticker in skillListEntries) {
            ticker.turnPointerOff();
        }
    }

    

    #endregion

    #endregion
}
