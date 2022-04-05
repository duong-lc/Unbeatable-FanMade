using System;
using DG.Tweening;
using System.Collections;
using Character;
using UnityEngine;
using Random = System.Random;


///<summary>
///handles lerping position of the note from spawn position to despawn position
///</summary>
public class NoteDefault : NoteBaseKillable
{
    [Header("Default Note Attributes")]
    private double _timeInstantiated; //time to instantiate the note
    public double assignedTime;//the time the note needs to be tapped by the player

    private Vector3 _startPos, _endPos;
    
    // Start is called before the first frame update
    private void Start()
    {
        animator = GetComponent<Animator>();
        //_parentLane = transform.parent.GetComponent<Lane>();
        EnableSpriteRenderer(true);
        SpriteFlip();
        UpdateKnockBackAttributes();

        PlayState(isHit : false);
        
        _timeInstantiated = SongManager.GetAudioSourceTime();

        switch (noteOrientation)
        {
            case NoteData.LaneOrientation.TopRight:
                _startPos = midiData.hitPointTopRight + (Vector3.right * midiData.noteSpawnX);
                _endPos = midiData.hitPointTopRight + (Vector3.right * midiData.noteDespawnX);
                break;
            case NoteData.LaneOrientation.BottomRight:
                _startPos = midiData.hitPointBottomRight + (Vector3.right * midiData.noteSpawnX);
                _endPos = midiData.hitPointBottomRight + (Vector3.right * midiData.noteDespawnX);
                break;
            case NoteData.LaneOrientation.TopLeft:
                _startPos = midiData.hitPointTopLeft + (Vector3.left * midiData.noteSpawnX);
                _endPos = midiData.hitPointTopLeft + (Vector3.left * midiData.noteDespawnX) ;
                break;
            case NoteData.LaneOrientation.BottomLeft:
                _startPos = midiData.hitPointBottomLeft + (Vector3.left * midiData.noteSpawnX);
                _endPos = midiData.hitPointBottomLeft + (Vector3.left * midiData.noteDespawnX);
                break;
            case NoteData.LaneOrientation.Undefined:
                Debug.LogError("Note Unassigned");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
    }

    // Update is called once per frame
    private void Update()
    {
        if (!CanMove) return;
        //lerp-ing note position
        LerpingNotePos();
    }
    
    ///<summary>
    ///Handles moving the note gameobject from spawn position to despawn position
    ///</summary>
    private void LerpingNotePos()
    {
        //localizing instances
        //textTimeStampKeyDown.text = assignedTime.ToString();
        double timeSinceInstantiated = SongManager.GetAudioSourceTime() - _timeInstantiated;
        //divide that with the time between the spawn Y and despawn Y to get the alpha position of the note relative to its total travel dist
        float alpha = (float)(timeSinceInstantiated / (midiData.noteTime * 2));

        if(alpha <= 1f)//otherwise, the position of note will be lerped between the spawn position and despawn position based on the alpha
        {
            transform.position = Vector3.Lerp(_startPos, _endPos, alpha);
        }
        // else if (alpha > 0.5f)//if alpha > 1, destroy the object
        // {
        //     Destroy(gameObject);
        // }
    }

    ///<summary>
    ///fade out enum to fade the node out and destroy if player misses
    ///</summary>
    public IEnumerator FadeOut()
    {
        //print($"coroutine execute");
        foreach (SpriteRenderer sprite in NoteSpriteArray)
        {
            sprite.DOFade(0, 0.06f);
        }
        yield return new WaitForSeconds(0.06f);
        Destroy(gameObject);
    }

    public new void OnHit()
    {
        PlayState(isHit : true);
        base.OnHit();
    }

    private void PlayState(bool isHit)
    {
        switch (noteOrientation)
        {
            case NoteData.LaneOrientation.TopRight:
            case NoteData.LaneOrientation.TopLeft:
                if(!isHit)
                    animator.Play(noteData.DefaultNoteAirRun);
                else
                {
                    animator.Play(GetRandInt() == 1 
                        ? noteData.DefaultNoteAirHit01 
                        : noteData.DefaultNoteAirHit02);
                }
                break;
            default:
                if(!isHit)
                    animator.Play(noteData.DefaultNoteGroundRun);
                else
                {
                    animator.Play(GetRandInt() == 1
                        ? noteData.DefaultNoteGroundHit01
                        : noteData.DefaultNoteGroundHit02);
                }
                break;
        }
    }

    private int GetRandInt()
    {
        Random rnd = new Random();
        return rnd.Next(1, 3);
    }
}
