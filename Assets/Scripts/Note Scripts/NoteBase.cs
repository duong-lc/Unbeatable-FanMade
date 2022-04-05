using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;


public abstract class NoteBase : MonoBehaviour
{
    [Header("Base Note Attributes")]
    protected SpriteRenderer[] NoteSpriteArray;
    protected bool CanMove = true;
    protected Animator animator;

    [SerializeField] protected SO_Midi_Data midiData;
    [SerializeField] protected SO_Note_Data noteData;
    public int octaveNum;
    public NoteData.LaneOrientation noteOrientation;

    // public float marginOffsetX; //+-0.2
    // public float marginOffsetY;//+- 0.1

 


    private void Awake()
    {
        NoteSpriteArray = GetComponentsInChildren<SpriteRenderer>();
    }
    
    protected void EnableSpriteRenderer(bool isEnabled)
    {
        foreach (SpriteRenderer sprite in NoteSpriteArray)
        {
            sprite.enabled = isEnabled;
        }
    }

    protected float SpriteFlip()
    {
        //print($"flip");
        switch (noteOrientation)
        {
            case NoteData.LaneOrientation.BottomRight:
            case NoteData.LaneOrientation.TopRight:
                foreach (var sprite in NoteSpriteArray)
                {
                    sprite.flipX = false;
                }
                return 1;
                //break;
            case NoteData.LaneOrientation.BottomLeft:
            case NoteData.LaneOrientation.TopLeft:
                foreach (var sprite in NoteSpriteArray)
                {
                    sprite.flipX = true;
                }
                return -1;
                //break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

 
    
    //protected float 
    
}
