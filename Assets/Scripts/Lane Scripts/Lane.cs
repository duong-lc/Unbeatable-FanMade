using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EventTypes;
using Character;
using UnityEditor;

///<Summary>    
///Handle Note Spawning and Player Input
///</Summary>
public class Lane : MonoBehaviour
{
    #region Variables
    [SerializeField] private SO_Midi_Data _midiData;
    
    public List<BaseNoteType> AllNoteOnLaneList = new List<BaseNoteType>();
    //public Lane opposingLane;

    private KeyCode _input;
    private bool _isSpawn = true;
    private int _spawnIndex = 0;//index spawn to loop through the timestamp array to spawn notes based on timestamp
    private int _inputIndex;//input index to loop through the timestamp array to form note input queue 
    private NoteData.LaneOrientation _orientation;
    
    //Variables for caching
    private Vector3 _laneHitPoint;
    private GameObject _DefaultNotePrefab;
    private GameObject _NoteSliderPrefab;
    private float _travelTime;
    private double _marginOfError;
    private float _inputDelay;
    
    [Header("Events")] 
    [SerializeField] private NoteHitEvent onKeyInput;
    
    #endregion

    private void Awake()
    {
        _laneHitPoint = gameObject.tag switch
        {
            "TopRight_Lane" => _midiData.hitPointTopRight,
            "BottomRight_Lane" => _midiData.hitPointBottomRight,
            "TopLeft_Lane" => _midiData.hitPointTopLeft,
            "BottomLeft_Lane" => _midiData.hitPointBottomLeft,
            _ => new Vector3(0,0,0)
        };

        _DefaultNotePrefab = _midiData.noteNormalPrefab;
        _NoteSliderPrefab = _midiData.noteSliderPrefab;
        _travelTime = _midiData.noteTime;
        _marginOfError = _midiData.marginOfError;
        _inputDelay = _midiData.inputDelayInMilliseconds;
    }
    private void Start()
    {
        _inputIndex = 0;
    }

    public void SetLocalListOnLane(List<BaseNoteType> listToSet)
    {
        AllNoteOnLaneList = listToSet;
        SetKeyInput();
    }

    private void SetKeyInput()
    {
        if (gameObject.CompareTag("TopRight_Lane"))
        {
            _orientation = NoteData.LaneOrientation.TopRight;
            _input = _midiData.inputTopRight;
        }
        else if (gameObject.CompareTag("TopLeft_Lane"))
        {
            _orientation = NoteData.LaneOrientation.TopLeft;
            _input = _midiData.inputTopLeft;
        }
        else if (gameObject.CompareTag("BottomRight_Lane"))
        {
            _orientation = NoteData.LaneOrientation.BottomRight;
            _input = _midiData.inputBottomRight;
        }
        else if (gameObject.CompareTag("BottomLeft_Lane"))
        {
            _orientation = NoteData.LaneOrientation.BottomLeft;
            _input = _midiData.inputBottomLeft;
        }
    }
    
    ///<Summary>    
    ///Update through the note timestamp array and compare with current song time to Spawn Note.
    ///As well as handling Note Input and hit and miss condition
    ///</Summary>
    private void Update()
    {
        //Spawning the notes
        SpawningNotesFromList();
        //Handling Input of the current lane
        InputNoteQueueHandler();
    }


    
    #region Note Spawning Functions

    ///<Summary>
    ///Handles spawning all the note types from the 'allNoteOnLaneList' list
    ///</Summary>
    private void SpawningNotesFromList()
    {
        if (!_isSpawn) return;
        if (AllNoteOnLaneList.Count <= 0)
        {
            //print($"no notes");
            return;
        }
        else
        {
            //print($"list size {allNoteOnLaneList.Count}");
        }

        switch (AllNoteOnLaneList[_spawnIndex].noteID)
        {
            case NoteData.NoteID.DefaultNote:
                NoteNormalType noteNormalCast = (NoteNormalType) AllNoteOnLaneList[_spawnIndex];

                //if current song time reaches point to spawn a note
                if (SongManager.GetAudioSourceTime() >= noteNormalCast.timeStamp - _travelTime)
                {
                    //print($"{gameObject.name}");
                    //Spawn a note
                    var noteObj = Instantiate(_DefaultNotePrefab, transform);
                    //updating the game object ref in the note
                    AllNoteOnLaneList[_spawnIndex].noteObj = noteObj;
                    noteObj.GetComponent<NoteDefault>().octaveNum = AllNoteOnLaneList[_spawnIndex].octaveNum;
                    //pass the orientation property
                    noteObj.GetComponent<NoteDefault>().noteOrientation = AllNoteOnLaneList[_spawnIndex].laneOrientation;
                    //get the time the note should be tapped by player and add to the array
                    noteObj.GetComponent<NoteDefault>().assignedTime = noteNormalCast.timeStamp;
                    
                    //increment the index
                    if(_spawnIndex + 1 <= AllNoteOnLaneList.Count - 1)
                        _spawnIndex++;
                    else
                        _isSpawn = false;

                }

                break;
            case NoteData.NoteID.SliderNote:
                NoteSliderType noteSliderCast = (NoteSliderType) AllNoteOnLaneList[_spawnIndex];

                if (SongManager.GetAudioSourceTime() >=
                    noteSliderCast.sliderData.timeStampKeyDown - _travelTime)
                {
                    //print($"{gameObject.name}");
                    //Spawn a slider prefab
                    var NoteSliderObj = Instantiate(_NoteSliderPrefab, transform);
                    //updating the game object ref in the note
                    AllNoteOnLaneList[_spawnIndex].noteObj = NoteSliderObj;
                    NoteSliderObj.GetComponent<NoteSlider>().octaveNum = AllNoteOnLaneList[_spawnIndex].octaveNum;
                    //pass the orientation property
                    NoteSliderObj.GetComponent<NoteSlider>().noteOrientation = AllNoteOnLaneList[_spawnIndex].laneOrientation;
                    //Passing data to the newly spawned slider 
                    NoteSliderObj.GetComponent<NoteSlider>().data = noteSliderCast.sliderData;
                    
                    //increment the index
                    if(_spawnIndex + 1 <= AllNoteOnLaneList.Count - 1)
                        _spawnIndex++;
                    else
                        _isSpawn = false;

                }

                break;
        }

    }
    #endregion
    #region Note Input Functions


    ///<Summarry>
    ///Handle the input of the current note that is going to be tapped by the player.
    ///Will handle and note type and channel them according to their input behavior.
    /// <returns>boolean to determine if the lane reach the end note of its lane</returns>
    ///</Summarry>
    private bool InputNoteQueueHandler()
    {
        if (_inputIndex < AllNoteOnLaneList.Count)//Loopiing through the time stamp list
        {
            //check for note type of the current note that's going to be tapped
            switch(AllNoteOnLaneList[_inputIndex].noteID)
            {
                case NoteData.NoteID.DefaultNote://if note is normal 
                    DefaultNoteInputHandle();
                    break;
                case NoteData.NoteID.SliderNote://if note is a slider note
                    NoteSliderInputHandle();            
                    break;
            }

            return true;
        }

        return false;
        // else if(Input.GetKeyDown(input) && gameObject.tag == "UpperLane")
        // {
        //     player.PlayJump("UpperLane", false);
        // }
    }
    ///<summary>
    ///Handles input notes for normal notes.
    ///Note: unlike slider notes that aren't supposed to be streamed by designed, normal notes are and 
    ///      Therefore can lead to multiple notes destroyed counting as hit at once (due to margin of error)
    ///      should input handling is on note game object and not properly sorted through.
    ///Note 2: Turns out slider note will have to be monitored by lane list to see if it's the current note to be tapped
    ///        because if you put the note reg function onto the note object instead of putting them in queue to wait,
    ///        2 notes will reg at the same time bc of them being too close leading marrgin of error recognzing them twice.
    ///</summary>
    private void DefaultNoteInputHandle()
    {
        //localizing instances
        NoteNormalType note = (NoteNormalType)AllNoteOnLaneList[_inputIndex];//cast the current note in the list to 'NoteNormalType'
        double timeStamp = note.timeStamp;//get normal note timestamp
        double audioTime = SongManager.GetAudioSourceTime() - (_inputDelay / 1000.0);//get current audio time with calibration with input delay (in seconds)

        if(Input.GetKeyDown(_input))
        {
            if (Math.Abs(audioTime - timeStamp) < _marginOfError)//hitting the note within the margin of error
            {
                NoteHit noteHit = new NoteHit
                {
                    orientation = _orientation,
                    noteID = note.noteID,
                    regState = RegisterState.KeyDown,
                    timeStamp = timeStamp,
                    noteObject = note.noteObj,
                    hitStatus = NoteHitStatus.Success
                };
                onKeyInput.Raise(noteHit);
                
                Hit();//Hit
                // player.PlayJump(gameObject.tag, false);
                note.noteObj.GetComponent<NoteDefault>().OnHit();//Destroy note
                _inputIndex++;//next note in queue
                
            }
        }
        if (timeStamp + _marginOfError <= audioTime)//if the player does not hit at all and passes the margin of error completely
        {
            NoteHit noteHit = new NoteHit
            {
                orientation = _orientation,
                noteID = NoteData.NoteID.DefaultNote,
                hitStatus = NoteHitStatus.Fail
            };
            onKeyInput.Raise(noteHit);
            Miss();//Miss
            note.noteObj.GetComponent<NoteDefault>().StartCoroutine(nameof(NoteDefault.FadeOut));//Fade out effect if misses
            _inputIndex++;//next note in queue
        } 
    }

    ///<summary>
    ///Handle input notes for slider notes
    ///</summary>
    private void NoteSliderInputHandle()
    {
        NoteSliderType note = (NoteSliderType)AllNoteOnLaneList[_inputIndex];//casting current note in list to slider note type
        double audioTime = SongManager.GetAudioSourceTime() - (_inputDelay / 1000.0);//get current audio time with calibration with input delay (in seconds)
        NoteData.SliderData data = note.sliderData;//getting slider note timestamp data

        if(note.noteObj != null)
        {
            NoteSlider noteComponent = note.noteObj.GetComponent<NoteSlider>();//getting slider note component of gameobject the current slider note in the list is possessing 
            
            //Handling Key Down Events
            NoteSliderInput_KeyDown(note, _marginOfError, audioTime, data, noteComponent);
            //Handling Holding Key Down Events
            NoteSliderInput_Event_KeyDownHold(noteComponent);
            //Handling Key Up Events
            NoteSliderInput_KeyUp(note, _marginOfError, audioTime, data, noteComponent);
            //*Handling slider bar sprite stretches and movements
            // if (data.timeStampKeyUp <= audioTime && noteComponent.lineController.gameObject != null)
            //     noteComponent.lineController.gameObject.SetActive(false);
            
        }  
    }
    ///<summary>
    ///Handles Key Down event for slider registration
    ///</summary>
    private void NoteSliderInput_KeyDown(NoteSliderType note, double marginOfError, double audioTime, NoteData.SliderData data, NoteSlider noteCompo)
    {
        //Handling on time
        if(Input.GetKeyDown(_input))
        {
            if (Math.Abs(audioTime - data.timeStampKeyDown) < marginOfError)//if start note is in time to tap
            {
                NoteHit noteHit = new NoteHit
                {
                    orientation = _orientation,
                    noteID = note.noteID,
                    regState = RegisterState.KeyDown,
                    timeStamp =  data.timeStampKeyDown,
                    noteObject = note.noteObj,
                    hitStatus = NoteHitStatus.Success
                };
                onKeyInput.Raise(noteHit);
                
                //Hit
                Hit();
                noteCompo.OnStartNoteHitCorrect(_laneHitPoint);
                
            }
        }
        //Handing late input
        if(data.timeStampKeyDown + marginOfError <= audioTime && !noteCompo.isStartNoteHitCorrectly)
        {
            NoteHit noteHit = new NoteHit
            {
                orientation = _orientation,
                noteID = NoteData.NoteID.SliderNote,
                hitStatus = NoteHitStatus.Fail
            };
            onKeyInput.Raise(noteHit);
            
            //Miss
            Miss();
            Destroy(noteCompo.gameObject);  
            _inputIndex++;//next note in queue
        }
    }
    ///<summary>
    ///Handles Holding Key Down event for slider registeration
    ///</summary>
    private void NoteSliderInput_Event_KeyDownHold(NoteSlider noteCompo)
    {
        if ((Input.GetKey(_input)) && noteCompo.isStartNoteHitCorrectly)
        {

            noteCompo.isHolding = true;
        }
    }
    ///<summary>
    ///Handles Key Up event for slider registration
    ///</summary>
    private void NoteSliderInput_KeyUp(NoteSliderType note, double marginOfError, double audioTime, NoteData.SliderData data, NoteSlider noteCompo) 
    {
        //Handling releasing on time and early
        if((Input.GetKeyUp(_input)) && noteCompo.isStartNoteHitCorrectly && noteCompo.isHolding)
        {
            NoteHit noteHit = new NoteHit
            {
                orientation = _orientation,
                noteID = NoteData.NoteID.SliderNote,
                regState = RegisterState.KeyUp,
                timeStamp =  data.timeStampKeyUp,
                noteObject = note.noteObj,
                hitStatus = NoteHitStatus.Undefined
            };
            if (Math.Abs(audioTime - data.timeStampKeyUp) < marginOfError)//hitting the note within the margin of error
            {   
                //Hit
                Hit();

                noteHit.hitStatus = NoteHitStatus.Success;
                if (noteCompo.isHolding)
                {
                    onKeyInput.Raise(noteHit);
                }
                
                noteCompo.isHolding = false;
                noteCompo.isEndNoteHitCorrectly = true;
                noteCompo.isStartNoteHitCorrectly = false;
                //Destroy(noteCompo.gameObject);
                noteCompo.GetComponent<NoteSlider>().OnHit();
                _inputIndex++;//next note in queue
            }else//release too early
            {
                noteHit.hitStatus = NoteHitStatus.Fail;
                onKeyInput.Raise(noteHit);
                NoteSliderInput_KeyUp_ReleaseEarly(noteCompo);
            }

            // NoteSliderInputHandle_BeatInteraction_KeyUp_EndAnimation();
        }
        //Handling late input
        if (data.timeStampKeyUp + marginOfError <= audioTime)//if the player does not hit at all and passes the margin of error completely
        {
            NoteHit noteHit = new NoteHit
            {
                orientation = _orientation,
                noteID = NoteData.NoteID.SliderNote,
                regState = RegisterState.KeyUp,
                timeStamp =  data.timeStampKeyUp,
                noteObject = note.noteObj,
                hitStatus = NoteHitStatus.Fail
            };
            if (noteCompo.isHolding)
            {
                onKeyInput.Raise(noteHit);
            }

            noteCompo.isHolding = false;
            if(!noteCompo.isEndNoteHitCorrectly)//Did not hit note correctly before
            {
                //print($"release too late");
                Miss(); 
                noteCompo.DestroyNote();
                _inputIndex++;//next note in queue
            }
        }
    }
    ///<summary>
    ///A macro function branched out from "NoteSliderInput_Event_KeyUp" that handles early releases when holding down sliders
    ///</summary>
    public void NoteSliderInput_KeyUp_ReleaseEarly(NoteSlider noteCompo)
    {
        Miss();
        noteCompo.DestroyNote();
        noteCompo.isHolding = false;

        noteCompo.isEndNoteHitCorrectly = false;
        noteCompo.isStartNoteHitCorrectly = false;
        _inputIndex++;//next note in queue
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
    
    #endregion

}
