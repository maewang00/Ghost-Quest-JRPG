using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance = null;
    private AudioManager audioManager;

    [SerializeField]
    [Tooltip("Reference to the event system")]
    private CustomEvents eventsys;

    IDictionary<string, string> interactions = new Dictionary<string, string>() {
        {"Chest","It's a chest!" + '\n' + "Seems like it's locked... |Unfortunately, ESP isn't super great for lockpicking. I wish I had hands..."},
        {"GreyHouse (i)", "It's a stone-walled house! |I wonder who lives there."},
        {"RedHouse (i)","It's a red--er, magenta-roofed house...? How unusual. |Seems like no one's home. I can't just barge in. That'd be criminal!"},
        {"Hornless", "Man, I ate so much cake!" +
            "|Birthdays are great. I'm glad I could bust out the horned helmet for this occasion."},
        {"Franklin", "Another day of tending to the flowers..." + '\n' + "Nothing quite like gardening to get the mind to relax." +
            "|It's the little things in life, y'know? I hope tomorrow will be a good day, too."},

        {"Elder", "You remind me of the child I never had. " +
            "|Hm? That's right, I  can see you. I've always been able to see your lot." +
            '\n' + "Not that anyone believes me, these days..." +
            "|Your body was stolen?" + '\n' + "...It's going to become the vessel for a \"demon\" king?" +
            "|...Is that what kids are into these days?" +
            "|I'll admit, I don't quite believe you. But, in the event you aren't pulling my leg here..." +
            "|The ruins to the east of town once belonged to a powerful lord." + '\n' + "They passed away decades ago." +
            "|If I recall, she was fascinated with the spirit realm. " +
            '\n' + "Perhaps you'll find something there?"},

        {"DevNotes1", "It's a crumpled up piece of paper. Something's written here..." +
            "|TO DO:" + '\n' + "We need more enemy types." +
            "|The world's a little quiet. And, yeah, we have to change the sound volume..." +
            "|It'd be nice if the players had a goal…" + '\n' + "(it'd be nice if I had a goal, too...... in life...)" +
            "|How long are we gonna use these placeholders?! (I mean, don't tell anyone, but we're, uh, borrowing from OpenGameArt.org)" +
            "|It'd be nice if you could use a controller, too. We've been hardcoding for arrow keys and keyboard. " +
            "|I'd also like to let players use WASD for navigating the battle menu, and shift to run." +
            "|oh yeah, gotta disable that debug overlay, too. Don't wanna have that in the final build!" +
            "|…The note ends there. What on earth did that mean?"},
        {"DevNotes2", "It's a crumpled up piece of paper. Something's sloppily written here…" +
            "|yknow we never explain why you have multiple party members…" + '\n' + "thats kinda bad, isnt it?" +
            "|we worked so hard on backend. and then the playtest results came in and yeah, people are expecting a story..." +
            "|…oh no. welp, were gonna have to think something up." +
            "|oh yeah. that chest is just there because I thought it'd look nice. :)" +
            "|…The note ends there. What a strange message!"},

        {"DungeonSign", "Old castle up ahead." + "|CAUTION!" + '\n' + "The structure is unstable: parts of the floor have started to collapse."},
        {"Tipper1", "If only I could float, like a ghost… then I'd be able to cross that gap, no sweat!"},
        {"SoulGrabber", "Y'know, I'm actually glad that upper management ordered us Snatchers and Catchers to team up. |This buddy system is pretty nice." +
            "|The free shades they gave us are pretty stylin', too! We should wear them at every opportunity." },
        {"SoulBagger", "(These sunglasses are nice, but... why are we wearing them in this dark, unlit castle?)" +
            "|(I doubt we'd be able to see the target if they were standing... er, floating right in front of us.)" +
            "|(...pretty sure we only got these shades to help us deal BRIGHT places, not pitch-black castles...!)" },
        {"sparkle",
            "There's something here... a journal? Diary?" + '\n' + "Let's take a look..." +
            "|\"Finally! After long months of research, I've finally found a way to enter the spirit realm.\"" +
            "|\"With this, I've come one step closer to proving that spirits really do exist.\"" +
            "|\"Ahah! AHAHAHAAAH!" + '\n' + "Don't think you can escape me, 'Demon Lord'! Fleeing to the spirit realm, like a little PANSY.\"" +
            "|\"No plane of existence is safe from my retribution--\"" +
            "|...I'm just gonna flip past this. ...Who writes out \"ahaha\"?" +
            "|\"Anyway. It's a good thing I set up my residence in this spiritually-charged village." +
            "|I'm going to have to ask the locals to help me set up the portal. Until then, dear diary!\"" +
            "|I think that's all the important stuff." + '\n' + "I should probably ask the village's elder about this..." },

        {"Boodle", "Woof woof." + '\n'  + "...Heh, just kidding." +
            "|Yo, master. 'Sup?" +
            "|It's pretty cool that we got to meet again in the afterlife like this." + '\n' + "Earlier than I'd have wanted, but..." +
            "|Anyway. It's been a while. I'm happy to be with you again." +
            "|I don't know much about \"demon lords\" or whatever, but you can count on me." +
            "|It's a dog's job to fetch, right? I'll help you fetch your body back." }
    };


    IDictionary<string, string> second_interaction = new Dictionary<string, string>() {

        {"Elder", "The ruins to the east of town once belonged to a powerful lord." + "|If I recall, she was fascinated with the spirit realm. " +
            '\n' + "Perhaps you'll find something there?" },
        {"sparkle", "The old diary from the long-dead noble who used to live here." +
            '\n' + "She was quite the nut, from the looks of things... " +
            "|I should ask the elder about what she wrote." },
        {"Boodle", "Now that I'm a ghost, I don't need to worry about my fur getting all mangled." +
            "|Funny how life works out. Er..." +
            "|Speaking of getting your body back... it might be a good idea to see what the locals have to say." }
    };


    IDictionary<string, int> load_what_interaction = new Dictionary<string, int>() {

        {"Elder", 0},
        {"sparkle", 0 }
    };

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

        GameObject gm = GameObject.FindWithTag("AudioManager");
        audioManager = gm.GetComponent<AudioManager>();
        eventsys = GameObject.FindWithTag("CustomEventManager").GetComponent<CustomEvents>();

        DontDestroyOnLoad(gameObject);
    }

    private void OnLevelWasLoaded(int level) {
        if (level != 3) { // level 3 = battle scene
            eventsys = GameObject.FindWithTag("CustomEventManager").GetComponent<CustomEvents>();
        }
    }

    #endregion

    #region interact_functions

    [SerializeField]
    private bool questflag = false;
    [SerializeField]
    private bool questover = false;

    private bool gameover = false;

    [SerializeField]
    private bool DEBUG_questflag = false;

    [SerializeField]
    private bool DEBUG_questover = false;

    

    public void Interact(string item)
    {
        int interactionNum = 0;
        if (load_what_interaction.ContainsKey(item)) {

            if (!gameover) { //if the "game is still in progress" (the quest is still active)
                if (item.Equals("sparkle") && !questflag || DEBUG_questflag) {
                    //The player is checking the quest thing in the dungeon's library.
                    Quest1();
                    questflag = true;
                    Debug.Log("Quest completed: go talk to the elder.");
                    DEBUG_questflag = false;
                }

                else if (item.Equals("Elder") && questflag && !questover || DEBUG_questover) {
                    //The player has checked the dungeon's library and is checking in with the elder.
                    questover = true;
                    Debug.Log("Checking in with the elder.");
                    DEBUG_questover = false;
                }

                if (questover) {
                    //The elder has said that he's gonna work on opening a portal. Dialogue is updated to "epilogue" status.
                    Quest1End();
                    Debug.Log("All quests completed. Updating dialogue.");
                    gameover = true;
                }
            }

            interactionNum = load_what_interaction[item];
        }

        string dialogue;

        if (interactionNum == 1) {
            dialogue = second_interaction[item];
        } else {
            dialogue = interactions[item];

            if (second_interaction.ContainsKey(item)) {
                load_what_interaction[item] = 1;
            }
        }

        audioManager.SFX_checkObject(item);
        eventsys.StartCutscene(dialogue);
    }

    private void ChangeDialogue1(string npcName, string newDialogue) {
        interactions.Remove(npcName);
        interactions.Add(npcName, newDialogue);
    }

    private void ChangeDialogue2(string npcName, string newDialogue) {
        second_interaction.Remove(npcName);
        second_interaction.Add(npcName, newDialogue);
    }

    //NOTE:
    /*
     * HOW TO USE THESE FUNCTIONS:
     * When changing dialogue, if a character had 2 interaction texts (they have an entry in both interactions and
     * second_interactions) make sure you update both.
     * Otherwise, the older interaction text (say you only update 1) will load in.
     *
     * Also, for some reason, if you wanna linebreak, you have to type \n in, not do the '\n' + thing that the dictionaries use.
     * I'm not sure why, but if it ain't broke...
     */

    private void Quest1() {
        //First and only quest of the game.
        //Controls dialogue changes for after the player has checked the sparkly thing in the dungeon library.
        
        ChangeDialogue1("Elder",
            "Well? Were you able to find anything?" +
            "|An old diary, you say? Do we have anyone who can open \"portals to other dimensions?\"" +
            "|And here I thought I was the one going senile." +
            "|Huh. I guess this is what that... what was it again? " +
            "|Oh yeah, that " +
            "\"Protocol for when you need to open a portal to the spirit realm\" thingy is for." +
            "|So I finally have a use for that thing! It's been passed down for generations." +
            "\nAlways wondered when it'd come in handy. " +
            "|So far, it's mostly just been a nice conversation starter. " +
            "\nI've also used it as a stepstool on more than one occasion." +
            "|...\"Why didn't I say anything sooner?\"" +
            "\nWell, genius, you never asked." +
            "|I'll fire up that portal for ya." + "\nBut it'll take a while." +
            "|And I mean a WHILE. Like, probably a good year of preperation..." +
             "\nMaybe more. Listen. Making a portal's not as easy as it sounds." +
             "|It takes a lot of heart, a lot of work, a lot of procrastinating on more important life tasks--" +
             "\nYou know what? I'm gonna stop right there." +
            "|Welp, no time like the present. Rome wasn't built in a day.");

        ChangeDialogue1("Boodle",
            "Good work getting that intel." +
            "|If that book's to be believed, it seems like you're not the first to " +
            "have crossed over to the spirit realm before their ferry left the dock.");

        ChangeDialogue2("Boodle", "I know I'm amazing, but you should probably go talk to the old guy, not to me.");
    }

    private void Quest1End() {
        //"Epilogue" where characters make meta commentary.

        ChangeDialogue1("Elder",
            "If I'm being honest, there's not much left for you do do here." +
            "|In the meanwhile, I guess you can just... dilly dally around.");
        ChangeDialogue2("Elder",
            "If I'm being honest, there's not much left for you do do here." +
            "|In the meanwhile, I guess you can just... dilly dally around.");
        ChangeDialogue1("Boodle", "So the elder's gonna work on opening a portal?" +
            "|Good work. If you need me, I'll still be here.");

        ChangeDialogue2("Boodle", "This reminds me of those cartoons you used to watch. Y'know, where once everything was over, they'd be all," +
            "|\"Our adventure has only just begun!\" " +
            "\neven if the story hadn't really been resolved..." +
            "|I gotta say, though. Those adventures were usually a little longer than this... " +
            "\nOh well." +
            "|Who knows what the future will hold?" +
            "|\"Deep thoughts for a dog,\" you say?" +
            "\nPlease, you flatter me.");

        ChangeDialogue1("sparkle", "I wonder if we'll ever get to meet the author of this book." +
            "|I guess it's like Boodle says, though. \"Our adventure has only just begun!\"" +
            "|...It's unfortunate that it'll take so long for the portal to be made. I wanted to keep adventuring.");
        ChangeDialogue2("sparkle", "\"What adventures await us, I wonder?\"" +
            "|\"The journey continues... in your heart!\"" +
            "|Haha. I wish... unless...?");
    }

    #endregion
}
