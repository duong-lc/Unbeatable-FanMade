using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region Other Classes and Inheritances

///<Summary>
///'NoteType' is the parent note class type that all notes will be derived from.
///</Summary>
[Serializable]
public class NoteType
{
    //NoteID enum to check for noteType
    [Serializable]
    public enum NoteID
    {
        NormalNote, SliderNote
    }
    public NoteID noteID;//ID of current note
    public GameObject noteObj;//the game object it's possessing

}
///<Summarry>
///'NoteNormalType' is a children class of 'NoteType'.
///</Summary>
[Serializable]
public class NoteNormalType : NoteType
{
    //timestamp of when the note should be tapped
    public double timeStamp;
}
///<Summarry>
///'NoteSliderType' is a children class of 'NoteType'.
///Contain struct "SlideData" which has 2 timestamps - KeyDownEvent timestamp and KeyUpEvent timestamp
///</Summary>
[Serializable]
public class NoteSliderType: NoteType
{
    [Serializable]
    public struct SliderData
    {
        public double timeStampKeyDown;
        public double timeStampKeyUp;
    }
    public SliderData sliderData = new SliderData();//timestamp for slider note
}
#endregion

///<Summary>    
///Handle Note Spawning and Player Input
///</Summary>
public class Lane : MonoBehaviour
{
    [Serializable]
    public enum LaneSide{
        leftSide, rightSide
    }
    [Serializable]
    public struct SideSwitch{
        public double timeStamp;//time stamp to switch side
        public LaneSide side;//which side to switch to
    }

    #region Variables
    public KeyCode input;//kkey input for that lane
    
    [Header("Normal Note")]//note on midi file to spawn and note object prefab to spawn
    public Melanchall.DryWetMidi.MusicTheory.NoteName noteRestrictionNormalNote;
    public GameObject noteNormalPrefab;

    [Header("Slider Note")]//note on midi file to spawn and note object prefab to spawn
    public Melanchall.DryWetMidi.MusicTheory.NoteName noteRestrictionSliderNote;
    public GameObject noteSliderPrefab;

    [Header("Universal")]
    public List<NoteType> allNoteOnLaneList = new List<NoteType>();//list of timestamps of all the notetypes 
    public Vector3 rightLaneHitPoint, leftLaneHitPoint;//Vector point determine where the hit point is
    public LaneSide currentSide;//current side (left orr right) the note will be spawned
    public LaneSide playerCurrentSide = LaneSide.rightSide;
    public CharacterHandler player;
    public CharacterQuaverHandler playerQuaver;
    public Lane opposingLane;

    bool isSpawn = true;
    int spawnIndex = 0;//index spawn to loop through the timestamp array to spawn notes based on timestamp
    public int inputIndex {get; private set;}//input index to loop through the timestamp array to form note input queue 
    int switchIndex = 0;
   // bool isHoldingSlider = false;

    public List<SideSwitch> switchList = new List<SideSwitch>();//list of timestamp and which side to switch to (left or right)
    #endregion


    ///<Summary>
    ///Setting the array of time stamps where the player should hit the key
    ///</Summary>
    public void SetTimeStamps(Melanchall.DryWetMidi.Interaction.Note[] array)
    {
        //localized vars for slider note array
        int i = 0;
        NoteSliderType.SliderData sliderNoteData = new NoteSliderType.SliderData();
        foreach (var note in array)//for every note in the note array
        {           
            //setting up array of normal note timestamps
            if(note.NoteName == noteRestrictionNormalNote)//check note name if it's on the note restriction 
            {
                //get the time stamp for that note. (note.Time does not return the format we want as it uses time stamp temp map in midi, so conversion is needed)
                var metricTimeSpan = TimeConverter.ConvertTo<MetricTimeSpan>(note.Time, SongManager.midiFile.GetTempoMap());
                var noteNormalLocal = new NoteNormalType();
                noteNormalLocal.noteID = NoteType.NoteID.NormalNote;
                noteNormalLocal.timeStamp = (double)metricTimeSpan.Minutes * 60f + metricTimeSpan.Seconds + (double)metricTimeSpan.Milliseconds / 1000f;
                //adding the time stamp (in seconds) to the array of time stamp
                allNoteOnLaneList.Add(noteNormalLocal);  
            }
            //setting up array of slider note timestamps
            else if (note.NoteName == noteRestrictionSliderNote)
            {
                //get the time stamp for that note. (note.Time does not return the format we want as it uses time stamp temp map in midi, so conversion is needed)
                var metricTimeSpan = TimeConverter.ConvertTo<MetricTimeSpan>(note.Time, SongManager.midiFile.GetTempoMap());
                if(i == 0)
                    sliderNoteData.timeStampKeyDown = (double)metricTimeSpan.Minutes * 60f + metricTimeSpan.Seconds + (double)metricTimeSpan.Milliseconds / 1000f;
                else
                    sliderNoteData.timeStampKeyUp = (double)metricTimeSpan.Minutes * 60f + metricTimeSpan.Seconds + (double)metricTimeSpan.Milliseconds / 1000f;
                i++;//incrrement to next note in midi note array        
                
                //For each 2 notes which is a slider, reset data for new slider note
                if(i>= 2)
                {
                    var noteSliderLocal = new NoteSliderType();
                    noteSliderLocal.noteID = NoteType.NoteID.SliderNote;
                    noteSliderLocal.sliderData = sliderNoteData;
                    //reset counter to start counting for new slider note pair
                    i = 0;
                    //Add the data to the list
                    allNoteOnLaneList.Add(noteSliderLocal);   
                    //reset the local data for next pair
                    sliderNoteData = new NoteSliderType.SliderData();
                }
            }
        }
    }

    private void Awake() {
        inputIndex = 0;
    }

    ///<Summary>    
    ///Update through the note timestamp array and compare with current song time to Spawn Note.
    ///As well as handling Note Input and hit and miss condition
    ///</Summary>
    void Update()
    {
        double time = SongManager.GetAudioSourceTime();
        if(switchIndex < switchList.Count)
        {
            if(switchList[switchIndex].timeStamp < time)
            {
                //print($"lane switched");
                currentSide = switchList[switchIndex].side;
                switchIndex++;
            }
        } 
        //Spawning the notes
        SpawingNotesFromList();
        //Handling Input of the current lane
        InputNoteQueueHandler();   
        AllNoteInputHandle_CharacterInteration_KeyUp();
    }


    
    #region Note Spawning Functions

    ///<Summary>
    ///Handles spawning all the note types from the 'allNoteOnLaneList' list
    ///</Summary>
    private void SpawingNotesFromList()
    { 
        //print($"{gameObject.name} {spawnIndex} / {allNoteOnLaneList.Count - 1}");
        /*
        *Spawning the normal Notes          
        */
        if(isSpawn)
        {
            if(allNoteOnLaneList[spawnIndex].noteID == NoteType.NoteID.NormalNote)//looping through time stamps to spawn
            {
                NoteNormalType noteNormalCast = (NoteNormalType)allNoteOnLaneList[spawnIndex];
                double spawnTime = noteNormalCast.timeStamp - SongManager.Instance.noteTime;
                
                if(SongManager.GetAudioSourceTime() >= spawnTime)//if current song time reaches point to spawn a note
                {
                    //Spawn a note
                    var noteObj = Instantiate(noteNormalPrefab, transform);
                    //updating the notes list of
                    allNoteOnLaneList[spawnIndex].noteObj = noteObj;
                    //get the time the note should be tapped by player and add to the array
                    noteObj.GetComponent<Note>().assignedTime = noteNormalCast.timeStamp;
                    //increment the index
                    if(spawnIndex + 1 <= allNoteOnLaneList.Count - 1)
                        spawnIndex++;
                    else
                        isSpawn = false;
                }  
            }
            /*
            *Spawning the slider Notes
            */
            if(allNoteOnLaneList[spawnIndex].noteID == NoteType.NoteID.SliderNote)
            {
                NoteSliderType noteSliderCast = (NoteSliderType)allNoteOnLaneList[spawnIndex];
                double spawnTime = noteSliderCast.sliderData.timeStampKeyDown - SongManager.Instance.noteTime;

                if(SongManager.GetAudioSourceTime() >= spawnTime)
                {
                    //Spawn a slider prefab
                    var sliderNoteObj = Instantiate(noteSliderPrefab, transform);
                    allNoteOnLaneList[spawnIndex].noteObj = sliderNoteObj;
                    //Passing data to the newly spawned slider 
                    sliderNoteObj.GetComponent<SliderNote>().data = noteSliderCast.sliderData;
                    if(spawnIndex + 1 < allNoteOnLaneList.Count - 1)
                        spawnIndex++;//increment to next note
                    else
                        isSpawn = false;
                }
            }
        }
        
    }
    #endregion
    #region Note Input Functions


    ///<Summarry>
    ///Handle the input of the current note that is going to be tapped by the player.
    ///Will handle and note type and channel them according to their input behavior.
    ///</Summary>
    private void InputNoteQueueHandler()
    {
        if (inputIndex < allNoteOnLaneList.Count)//Loopiing through the time stamp list
        {
            //check for note type of the current note that's going to be tapped
            switch(allNoteOnLaneList[inputIndex].noteID)
            {
                case NoteType.NoteID.NormalNote://if note is normal 
                    if(allNoteOnLaneList[inputIndex].noteObj != null && allNoteOnLaneList[inputIndex].noteObj.GetComponent<Note>().side == LaneSide.leftSide)
                        playerCurrentSide = LaneSide.leftSide;
                    else
                        playerCurrentSide = LaneSide.rightSide;
                    NormalNoteInputHandle();
                    break;
                case NoteType.NoteID.SliderNote://if note is a slider note
                    if(allNoteOnLaneList[inputIndex].noteObj != null && allNoteOnLaneList[inputIndex].noteObj.GetComponent<SliderNote>().side == LaneSide.leftSide)
                        playerCurrentSide = LaneSide.leftSide;
                    else
                        playerCurrentSide = LaneSide.rightSide;
                    SliderNoteInputHandle();            
                    break;
            }
        }else if(Input.GetKeyDown(input) && gameObject.tag == "UpperLane")
        {
            player.PlayJump("UpperLane", false);
        }
    }
    ///<summary>
    ///Handles input notes for normal notes.
    ///Note: unlike slider notes that aren't supposed to be streamed by designed, normal notes are and 
    ///      Therefore can lead to multiple notes destroyed counting as hit at once (due to margin of error)
    ///      should input handling is on note gameobject and not properly sorted through.
    ///Note 2: Turns out slider note will have to be monitored by lane list to see if it's the current note to be tapped
    ///        because if you put the note reg function onto the note object instead of putting them in queue to wait,
    ///        2 notes will reg at the same time bc of them being too close leading marrgin of error recognzing them twice.
    ///</summary>
    private void NormalNoteInputHandle()
    {
        //localizing instances
        NoteNormalType note = (NoteNormalType)allNoteOnLaneList[inputIndex];//cast the current note in the list to 'NoteNormalType'
        double timeStamp = note.timeStamp;//get normal note timestamp
        double marginOfError = SongManager.Instance.marginOfError;//cacheing the margin of error
        double audioTime = SongManager.GetAudioSourceTime() - (SongManager.Instance.inputDelayInMilliseconds / 1000.0);//get current audio time with calibration with input delay (in seconds)

        if(Input.GetKeyDown(input)) //Get input
        {
            if (Math.Abs(audioTime - timeStamp) < marginOfError)//hitting the note within the margin of error
            {
                Hit();//Hit
                player.PlayJump(gameObject.tag, false);
                Destroy(allNoteOnLaneList[inputIndex].noteObj);//Destroy note
                inputIndex++;//next note in queue

                var Tuple = NormalNoteInputHandle_Quaver_BehaviorRoutineCheck(note);
                if(Tuple.canQuaver)
                    NormalNoteInputHandle_QuaverInteraction_KeyDown();
                NormalNoteInputHandle_BeatInteraction_KeyDown(Tuple.dashNum);
            }
            else if(gameObject.tag == "UpperLane" && player.beatCurrentLane == CharacterHandler.currentLaneOccupied.none)
            {
                player.PlayJump("UpperLane", false);
            }  
        }
        if (timeStamp + marginOfError <= audioTime)//if the player does not hit at all and passes the margin of error completely
        {
            Miss();//Miss
            allNoteOnLaneList[inputIndex].noteObj.GetComponent<Note>().StartCoroutine("FadeOut");//Fade out effect if misses
            inputIndex++;//next note in queue
        } 
    }

    ///<summary>
    ///Regulate when Quaver can appear depending on what note type and note time stamp that Beat is on
    ///Disable Quaver during the "Taiko" and "Skrtt" Patterns
    ///Enable Quaver to be allowed to appear when at "Morse Code" Pattern
    ///return int canQuaver (1) 
    ///return int dashNum (2) - 1  = dash up, 2 = dash down, 0 or else = normal
    ///</summary>
    private (bool canQuaver,int dashNum) NormalNoteInputHandle_Quaver_BehaviorRoutineCheck(NoteNormalType note)
    {
        if(opposingLane.inputIndex >= opposingLane.allNoteOnLaneList.Count)
        {
            return (false, 0);
        }else{
            NoteType otherNote = opposingLane.allNoteOnLaneList[opposingLane.inputIndex];//note on the opposite lane that the is being compared to
            if(otherNote.noteObj != null)
            {
                switch (otherNote.noteID)
                {
                    case NoteType.NoteID.NormalNote://normal note with normal note interaction
                        double otherTimeStamp = otherNote.noteObj.GetComponent<Note>().assignedTime;
                        double x = GetAbsoluteValue(note.timeStamp - otherTimeStamp);
                        if(x <= 0.01)// The "Taiko Pattern"
                        {
                            switch(player.isGrounded)
                            {
                                //case CharacterHandler.currentLaneOccupied.upperLane:
                                case false:
                                    //upper->lower || dashing down atk
                                    return (false, 1);
                                case true:
                                    //lower->upper || dashing up atk
                                    return (false, 2);
                            }
                        }
                            //tell beat to do taiko type animation
                        //else is just the Skrt pattern so just do normal dashing up and down as intended

                        break;
                    case NoteType.NoteID.SliderNote://pressing normal note when the other lane is holding down a slider
                        double sliderStartTime = otherNote.noteObj.GetComponent<SliderNote>().data.timeStampKeyDown;
                        double sliderEndTime = otherNote.noteObj.GetComponent<SliderNote>().data.timeStampKeyUp;
                        if (note.timeStamp >= sliderStartTime && note.timeStamp <= sliderEndTime)
                        {
                            if (otherNote.noteObj.GetComponent<SliderNote>().isHolding)//check for if beat is succesfully holding the note
                                return (true,0);
                            else
                                return (false,0);
                        }else
                            return (false,0);
                }
            }
        }
        
        return (false,0);
    }


    ///<summary>
    ///Handles Quaver's Interactions (animations and behavior conditions) with Normal Note Key Down Inputs
    ///</summary>
    private void NormalNoteInputHandle_QuaverInteraction_KeyDown()
    {
        if(player.beatCurrentLane != CharacterHandler.currentLaneOccupied.none &&
        playerQuaver.quaverCurrentLane == CharacterHandler.currentLaneOccupied.none)
        {
            playerQuaver.EnableQuaverAtLane(gameObject.tag, CharacterQuaverHandler.CombatState.attack);
            //Invoke("DisQuaver", playerQuaver.gameObject.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length);
            //playerQuaver.PlayAttack();
        }
    }

    ///<summary>
    ///Handles Beat's Interactions (animations and behavior conditions) with Normal Note Key Down Inputs
    ///</summary>
    private void NormalNoteInputHandle_BeatInteraction_KeyDown(int dashNum)
    {       
        //print($"{dashNum}");
        switch(dashNum)
        {
            case 1://from lower lane to upper lane
                player.PlayDashLowerToUpper_TaikoPattern();
                //print($"up");
                break;
            case 2://from upper lane to lower lane
                player.PlayDashUpperToLower_TaikoPattern();
                //print($"down");
                break;
            default://not dashing at all
                if(player.beatCurrentLane == CharacterHandler.currentLaneOccupied.none)
                {
                    switch(gameObject.tag)
                    {
                        case "UpperLane":
                            //print($"jump");
                            player.beatCurrentLane = CharacterHandler.currentLaneOccupied.upperLane;
                            player.PlayJump(gameObject.tag, false);
                            break;
                        case "LowerLane":
                           // print($"down");
                            player.beatCurrentLane = CharacterHandler.currentLaneOccupied.lowerLane;
                            break;
                    }
                }
                break;
        }
    }
    

    ///<summary>
    ///Handle input notes for slider notes
    ///</summary>
    private void SliderNoteInputHandle()
    {
        NoteSliderType note = (NoteSliderType)allNoteOnLaneList[inputIndex];//casting current note in list to slider note type
        double marginOfError = SongManager.Instance.marginOfError;//cacheing the margin of error
        double audioTime = SongManager.GetAudioSourceTime() - (SongManager.Instance.inputDelayInMilliseconds / 1000.0);//get current audio time with calibration with input delay (in seconds)
        NoteSliderType.SliderData data = note.sliderData;//getting slider note timestamp data
        SliderNote noteCompo;//getting slider note component of gameobject the current slider note in the list is possessing 
        
        if(note.noteObj != null)
        {
            noteCompo = note.noteObj.GetComponent<SliderNote>();
            
            //Handling Key Down Events
            SliderNoteInput_Event_KeyDown(note, marginOfError, audioTime, data, noteCompo);
            //Handling Holding Key Down Events
            SliderNoteInput_Event_KeyDownHold(noteCompo);
            //Handling Key Up Events
            SliderNoteInput_Event_KeyUp(note, marginOfError, audioTime, data, noteCompo);
            //*Handling slider bar sprite stretches and movements
            if (data.timeStampKeyUp <= audioTime && noteCompo.lineController.gameObject != null)
                noteCompo.lineController.gameObject.SetActive(false);
            
        }  
    }
    ///<summary>
    ///Handles Key Down event for slider registeration
    ///</summary>
    private void SliderNoteInput_Event_KeyDown(NoteSliderType note, double marginOfError, double audioTime, NoteSliderType.SliderData data, SliderNote noteCompo)
    {
        
        //Handling on time
        if(Input.GetKeyDown(input))
        {
            if (Math.Abs(audioTime - data.timeStampKeyDown) < marginOfError)//if start note is in time to tap
            {
                //Hit
                Hit();
                //Setting condition for endNote evaluation
                noteCompo.isStartNoteHitCorrectly = true;
                noteCompo.isStartNoteMovable = false;
                //Locking note position to hit point side based on its current side (left or right)
                switch(noteCompo.side)
                {
                    case LaneSide.rightSide:
                        noteCompo.startNote.transform.position = rightLaneHitPoint;
                        break;
                    case LaneSide.leftSide:
                        noteCompo.startNote.transform.position = leftLaneHitPoint;
                        break;
                }
                //Quaver note input handle must be before beat due condition checking in execution order
                bool canQuaver = SliderNoteInputHandle_Quaver_BehaviorRoutineCheck(note);
                if(canQuaver)
                    SliderNoteInputHandle_QuaverInteraction_KeyDown();
                SliderNoteInputHandle_BeatInteraction_KeyDown(canQuaver);
            }
            else if(gameObject.tag == "UpperLane" && player.beatCurrentLane == CharacterHandler.currentLaneOccupied.none)
            {
                player.PlayJump("UpperLane", false);
            }
        }
        //Handing late input
        if(data.timeStampKeyDown + marginOfError <= audioTime && !noteCompo.isStartNoteHitCorrectly)
        {
            //Miss
            Miss();
            Destroy(noteCompo.gameObject);  
            inputIndex++;//next note in queue
        }
    }
    ///<summary>
    ///Handles Holding Key Down event for slider registeration
    ///</summary>
    private void SliderNoteInput_Event_KeyDownHold(SliderNote noteCompo)
    {
        if(Input.GetKey(input) && noteCompo.isStartNoteHitCorrectly)
            noteCompo.isHolding = true;
    }
    ///<summary>
    ///Handles Key Up event for slider registration
    ///</summary>
    private void SliderNoteInput_Event_KeyUp(NoteSliderType note, double marginOfError, double audioTime, NoteSliderType.SliderData data, SliderNote noteCompo) 
    {
        //Handling releasing on time and early
            if(Input.GetKeyUp(input) && noteCompo.isStartNoteHitCorrectly && noteCompo.isHolding)
            {
                if (Math.Abs(audioTime - data.timeStampKeyUp) < marginOfError)//hitting the note within the margin of error
                {   
                    //Hit
                    Hit();
                    
                    noteCompo.isHolding = false;
                    noteCompo.isEndNoteHitCorrectly = true;
                    noteCompo.isStartNoteHitCorrectly = false;
                    Destroy(noteCompo.gameObject);
                    inputIndex++;//next note in queue
                }else//release too early
                {
                    SliderNoteInput_Event_KeyUp_ReleaseEarly(noteCompo);
                }

                SliderNoteInputHandle_BeatInteraction_KeyUp_EndAnimation();
            }
            //Handling late input
            if (data.timeStampKeyUp + marginOfError <= audioTime)//if the player does not hit at all and passes the margin of error completely
            {
                SliderNoteInputHandle_BeatInteraction_KeyUp_EndAnimation();

                if((gameObject.tag == "UpperLane" && playerQuaver.quaverCurrentLane == CharacterHandler.currentLaneOccupied.upperLane)
                ||(gameObject.tag == "LowerLane" && playerQuaver.quaverCurrentLane == CharacterHandler.currentLaneOccupied.lowerLane))
                    playerQuaver.PlayAttack();
                    //playerQuaver.DisableQuaver();
                

                noteCompo.isHolding = false;
                if(!noteCompo.isEndNoteHitCorrectly)//Did not hit note correctly before
                {
                    //print($"release too late");
                    Miss(); 
                    Destroy(noteCompo.gameObject);
                    inputIndex++;//next note in queue
                }
            }
    }
    ///<summary>
    ///A macro function branched out from "SliderNoteInput_Event_KeyUp" that handles early releases when holding down sliders
    ///</summary>
    public void SliderNoteInput_Event_KeyUp_ReleaseEarly(SliderNote noteCompo)
    {
        Miss();
        Destroy(noteCompo.gameObject);
        noteCompo.isHolding = false;

        noteCompo.isEndNoteHitCorrectly = false;
        noteCompo.isStartNoteHitCorrectly = false;
        inputIndex++;//next note in queue
    }
    ///<summary>
    ///Regulate when Quaver can appear depending on what note type and note time stamp that Beat is on
    ///When holding a slider, check the other lane if another type of note is being pressed at the during the duration of holding.
    ///In this Behavior check for Quaver to appear when the 2 lanes are at "Slider Stream" pattern
    ///</summary>
    private bool SliderNoteInputHandle_Quaver_BehaviorRoutineCheck(NoteSliderType note)
    {
        if(opposingLane.inputIndex >= opposingLane.allNoteOnLaneList.Count)//Handles out of range exceptions
            return false;
        else{
            NoteType otherNote = opposingLane.allNoteOnLaneList[opposingLane.inputIndex];//note on the opposite lane that the is being compared to
            if(otherNote.noteObj != null)
            {
                switch (otherNote.noteID)
                {
                    case NoteType.NoteID.NormalNote://if the other lane has a normal to press, ignore. Bc it's already been handled        
                        break;
                    case NoteType.NoteID.SliderNote://pressing normal note when the other lane is holding down a slider
                        double sliderStartTime = otherNote.noteObj.GetComponent<SliderNote>().data.timeStampKeyDown;
                        double sliderEndTime = otherNote.noteObj.GetComponent<SliderNote>().data.timeStampKeyUp;
                        if (note.sliderData.timeStampKeyDown >= sliderStartTime && note.sliderData.timeStampKeyDown < sliderEndTime)
                        {
                            if (otherNote.noteObj.GetComponent<SliderNote>().isHolding)//check for if beat is succesfully holding the note
                                return true;
                            else
                                return false;
                        }else
                            return false;
                }
            }
        }
        
        return false;
    }

    ///<summary>
    ///Handles Quaver's Interactions (animations and behavior conditions) with Slider Note Key Down Inputs
    ///</summary>
    private void SliderNoteInputHandle_QuaverInteraction_KeyDown()
    {  
        if(player.beatCurrentLane != CharacterHandler.currentLaneOccupied.none)
            playerQuaver.EnableQuaverAtLane(gameObject.tag, CharacterQuaverHandler.CombatState.block);
    }
    ///<summary>
    ///Handles Beat's Interactions (animations and behavior conditions) with Slider Note Key Down Inputs
    ///Also handles misses and Beat's animation when holding down a key for the current slider for too long 
    ///and presses the key for the next slider on the other lane during the "Slider Hop" pattern
    ///</summary>
    private void SliderNoteInputHandle_BeatInteraction_KeyDown(bool canQuaver)
    {
        switch(canQuaver)
        {
            case true:
                break;
            case false:
                if(player.beatCurrentLane != CharacterHandler.currentLaneOccupied.none)
                {
                    switch(player.beatCurrentLane)
                    {
                        case CharacterHandler.currentLaneOccupied.lowerLane:
                            if(opposingLane.gameObject.tag == "LowerLane" && opposingLane.allNoteOnLaneList[opposingLane.inputIndex].noteID == NoteType.NoteID.SliderNote)
                            {
                                SliderNote noteCompo = opposingLane.allNoteOnLaneList[opposingLane.inputIndex].noteObj.GetComponent<SliderNote>();
                                if(noteCompo.isHolding)
                                {
                                    opposingLane.SliderNoteInput_Event_KeyUp_ReleaseEarly(noteCompo);
                                }
                            }
                            break;
                        case CharacterHandler.currentLaneOccupied.upperLane:
                            if(opposingLane.gameObject.tag == "UpperLane" && opposingLane.allNoteOnLaneList[opposingLane.inputIndex].noteID == NoteType.NoteID.SliderNote)
                            {
                                SliderNote noteCompo = opposingLane.allNoteOnLaneList[opposingLane.inputIndex].noteObj.GetComponent<SliderNote>();
                                if(noteCompo.isHolding)
                                {
                                    opposingLane.SliderNoteInput_Event_KeyUp_ReleaseEarly(noteCompo);
                                }
                            }
                            break;
                    }
                    player.beatCurrentLane = CharacterHandler.currentLaneOccupied.none;
                }
                break;
        }

        if(player.beatCurrentLane == CharacterHandler.currentLaneOccupied.none)
        {
            switch(gameObject.tag)
            {
                case "UpperLane":
                    player.beatCurrentLane = CharacterHandler.currentLaneOccupied.upperLane;
                    player.PlayJump(gameObject.tag, true);
                    break;
                case "LowerLane":
                    player.beatCurrentLane = CharacterHandler.currentLaneOccupied.lowerLane;
                    player.RemoveStalling();
                    player.PlayBlockIntro();
                    player.PlayDashDown();
                    break;
            }   
        }
    }
    ///<summary>
    ///Handles Beat's Interactions (end animation of slider conditions and behavior) with Slider Note Key Up Input
    ///</summary>
    private void SliderNoteInputHandle_BeatInteraction_KeyUp_EndAnimation()
    {
        if(gameObject.tag == "UpperLane" && player.beatCurrentLane == CharacterHandler.currentLaneOccupied.upperLane )
        {
            player.beatCurrentLane = CharacterHandler.currentLaneOccupied.none;
            player.PlayAirAttack();
            player.RemoveStalling();
        }
        else if (gameObject.tag == "LowerLane" && player.beatCurrentLane == CharacterHandler.currentLaneOccupied.lowerLane )
        {
            player.beatCurrentLane = CharacterHandler.currentLaneOccupied.none;
            player.PlayGroundAttack();
            player.RemoveStalling();
        } 
    }
    
    
    ///<summary>
    ///Handles Quaver and Beat's Interactions (animations and behavior conditions) with all note types Key Up Inputs
    ///</summary>
    private void AllNoteInputHandle_CharacterInteration_KeyUp()
    {
        if(Input.GetKeyUp(input))
        {
            switch(gameObject.tag)
            {
                case "UpperLane":
                    if(player.beatCurrentLane == CharacterHandler.currentLaneOccupied.upperLane)
                    {
                        player.RemoveStalling();
                        player.beatCurrentLane = CharacterHandler.currentLaneOccupied.none;
                    } 
                    if(playerQuaver.quaverCurrentLane == CharacterHandler.currentLaneOccupied.upperLane)
                    {
                        playerQuaver.quaverCurrentLane = CharacterHandler.currentLaneOccupied.none;
                        if (allNoteOnLaneList[inputIndex-1].noteID == NoteType.NoteID.SliderNote)
                            playerQuaver.PlayAttack();
                    }
                        
                    break;
                case "LowerLane":
                    if(player.beatCurrentLane == CharacterHandler.currentLaneOccupied.lowerLane)
                    {
                        player.RemoveStalling();
                        player.beatCurrentLane = CharacterHandler.currentLaneOccupied.none;
                    }
                    if(playerQuaver.quaverCurrentLane == CharacterHandler.currentLaneOccupied.lowerLane)
                    {
                        playerQuaver.quaverCurrentLane = CharacterHandler.currentLaneOccupied.none;
                        if (allNoteOnLaneList[inputIndex-1].noteID == NoteType.NoteID.SliderNote)
                            playerQuaver.PlayAttack();
                    }
                        
                    break;
            }
        }
    }
    
    
    #endregion
   
    #region Utility Functions
    private void Hit()
    {
        ScoreManager.Hit();
    }
    private void Miss()
    {
        ScoreManager.Miss();
    }

    private void OnDrawGizmosSelected() 
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(leftLaneHitPoint, 0.2f);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(rightLaneHitPoint, 0.2f);
    }
    private static double GetAbsoluteValue(double number)
    {
        if(number < 0)
        {
            number = number * -1;
        }
        return number;
    }
    #endregion

}
