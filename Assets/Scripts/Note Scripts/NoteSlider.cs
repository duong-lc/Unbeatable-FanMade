using UnityEngine;
using System;
using Character;
using Random = System.Random;

///<summary>
///Handles lerp-ing position from Spawn to End
///</summary>
public class NoteSlider : NoteBaseKillable
{

    [Header("Slider Note Attributes")]
    public NoteData.SliderData data;//Data of hitting time stamp to activate
    public GameObject startNote, endNote;//game object of start and end note
    public double startNoteSpawnTime, endNoteSpawnTime;//Spawn time stamp of start and end notes

    [SerializeField] private Transform[] points;
    private LR_LineController[] _lineControllers;

    public bool isStartNoteHitCorrectly = false, isEndNoteHitCorrectly = false, isHolding = false;//booleans to check conditions to register notes
    public bool isStartNoteMovable = true;

    private Vector3 _startPosStartNote, _endPosStartNote, _startPosEndNote, _endPosEndNote;
    private float _sliderHitPointX;
    private bool _canMoveEndNote = true;

    private void Start()
    {
        animator = startNote.GetComponent<Animator>();
        _lineControllers = GetComponentsInChildren<LR_LineController>();
        EnableSpriteRenderer(true);
        SpriteFlip();
        UpdateKnockBackAttributes();
        
        foreach(var line in _lineControllers)
            line.SetUpLine(points);
        
        //setting spawn time stamp
        startNoteSpawnTime = data.timeStampKeyDown - base.midiData.noteTime;
        endNoteSpawnTime = data.timeStampKeyUp - midiData.noteTime;
        
        switch (noteOrientation)
        {
            case NoteData.LaneOrientation.TopRight:
                _startPosStartNote = midiData.hitPointTopRight + (Vector3.right * midiData.noteSpawnX);
                _endPosStartNote = midiData.hitPointTopRight + (Vector3.right * midiData.noteDespawnX);
                _startPosEndNote = midiData.hitPointTopRight+(Vector3.right * midiData.noteSpawnX);
                _endPosEndNote = midiData.hitPointTopRight+(Vector3.right * midiData.noteDespawnX);
                _sliderHitPointX = midiData.hitPointTopRight.x;
                break;
            case NoteData.LaneOrientation.BottomRight:
                _startPosStartNote = midiData.hitPointBottomRight + (Vector3.right * midiData.noteSpawnX);
                _endPosStartNote = midiData.hitPointBottomRight + (Vector3.right * midiData.noteDespawnX);
                _startPosEndNote = midiData.hitPointBottomRight+(Vector3.right * midiData.noteSpawnX);
                _endPosEndNote = midiData.hitPointBottomRight+(Vector3.right * midiData.noteDespawnX);
                _sliderHitPointX = midiData.hitPointBottomRight.x;
                break;
            case NoteData.LaneOrientation.TopLeft:
                _startPosStartNote = midiData.hitPointTopLeft + (Vector3.left * midiData.noteSpawnX);
                _endPosStartNote = midiData.hitPointTopLeft + (Vector3.left * midiData.noteDespawnX);
                _startPosEndNote = midiData.hitPointTopLeft+(Vector3.left * midiData.noteSpawnX);
                _endPosEndNote = midiData.hitPointTopLeft+(Vector3.left * midiData.noteDespawnX);
                _sliderHitPointX = midiData.hitPointTopLeft.x;
                break;
            case NoteData.LaneOrientation.BottomLeft:
                _startPosStartNote = midiData.hitPointBottomLeft + (Vector3.left * midiData.noteSpawnX);
                _endPosStartNote = midiData.hitPointBottomLeft + (Vector3.left * midiData.noteDespawnX);
                _startPosEndNote = midiData.hitPointBottomLeft+(Vector3.left * midiData.noteSpawnX);
                _endPosEndNote = midiData.hitPointBottomLeft+(Vector3.left * midiData.noteDespawnX);
                _sliderHitPointX = midiData.hitPointBottomLeft.x;
                break;
            case NoteData.LaneOrientation.Undefined:
                Debug.LogError("Note Unassigned");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        startNote.transform.position = endNote.transform.position = _startPosStartNote;
        //deactivate at start
        endNote.transform.parent = startNote.transform.parent;
        endNote.SetActive(false);
        foreach(var line in _lineControllers)
            line.gameObject.SetActive(false);
    }

    ///<Summary>
    ///Update and handle lerping start note and end note from spawn to hit point. 
    ///Handlling Key Input, determine hit and miss condition of slider
    ///</Summary>
    void Update()
    {
        if (!CanMove) return;
        //Lerping Start Note
        LerpingStartNotePos();
        //Lerping End Note
        if (!_canMoveEndNote) return;
        LerpingEndNotePos();
    }
    
    ///<Summary>
    ///Lerping position of startnote from spawn to despawn position
    ///</Summary>
    private void LerpingStartNotePos()
    {
        //Lerping Start Note
        if(SongManager.GetAudioSourceTime() >= startNoteSpawnTime && isStartNoteMovable)
        {
            //getting the current time stamp since the song is played
            double timeSinceStartNoteInstantiated = SongManager.GetAudioSourceTime() - startNoteSpawnTime;
            //divide that with the time between the spawn Y and despawn Y to get the alpha position of the note relative to its total travel dist
            float alphaStart = (float)(timeSinceStartNoteInstantiated / (midiData.noteTime * 2));
            
            //if it reaches the despawn position
            if (alphaStart > 1)//if alpha > 1, destroy the object
            {
                Destroy(startNote);
            }
            else if(startNote)//otherwise, the position of note will be lerped between the spawn position and despawn position based on the alpha
            {
                startNote.transform.position = Vector3.Lerp(_startPosStartNote, _endPosStartNote, alphaStart);
                //activate the note and show the line renderer
                startNote.SetActive(true);
                foreach(var line in _lineControllers)
                    line.gameObject.SetActive(true);
            }
        }
    }

    ///<Summary>
    ///Lerping position of endnote from spawn to despawn position
    ///</Summary>
    private void LerpingEndNotePos()
    {
        //when the end note is spawnable/movable
        if(SongManager.GetAudioSourceTime() >= endNoteSpawnTime)
        {
            //getting the current time stamp since the song is played
            double timeSinceEndNoteInstantiated = SongManager.GetAudioSourceTime() - endNoteSpawnTime;
            //divide that with the time between the spawn Y and despawn Y to get the alpha position of the note relative to its total travel dist
            float alphaEnd = (float)(timeSinceEndNoteInstantiated / (midiData.noteTime * 2));
            
            if (endNote)//otherwise, the position of note will be lerped between the spawn position and despawn position based on the alpha
            {
                //print($"alphaEnd = {alphaEnd}");
                if (Math.Abs(endNote.transform.position.x - _sliderHitPointX) > 0 && alphaEnd < 0.5f)
                {
                    endNote.transform.position = Vector3.Lerp(_startPosEndNote, _endPosEndNote, alphaEnd);
                    endNote.SetActive(true);
                }
                else
                {
                    _canMoveEndNote = false;
                    endNote.transform.position = startNote.transform.position;
                }
            }
            // // //if it reaches the despawn position
            // if (alphaEnd > 0.5f)//if alpha > 1, destroy the object
            // {
            //     // Destroy(gameObject);
            //     // Destroy(endNote);
            // }
        }
        else
        {
            endNote.transform.position = _startPosEndNote;
        }
    }

    public void OnStartNoteHitCorrect(Vector3 hitPoint)
    {
        //Setting condition for endNote evaluation
        isStartNoteHitCorrectly = true;
        isStartNoteMovable = false;
        //Locking note position to hit point side based on its current side (left or right)
        startNote.transform.position = hitPoint;
        
        animator.Play(noteData.SliderNoteFrontBlock);
    }

    public new void OnHit()
    {
        endNote.transform.parent = startNote.transform;
        animator.Play(GetRandInt() == 1
            ? noteData.SliderNoteFrontHit01
            : noteData.SliderNoteFrontHit02);
        base.OnHit();
    }

    public void DestroyNote()
    {
        endNote.transform.parent = startNote.transform;
        Destroy(gameObject);
    }
    
    private int GetRandInt()
    {
        Random rnd = new Random();
        return rnd.Next(1, 3);
    }
    
    
}
