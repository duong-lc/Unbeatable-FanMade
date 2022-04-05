using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Diagnostics;
using Melanchall.DryWetMidi.MusicTheory;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Midi Data", order = 1)]
public class SO_Midi_Data : ScriptableObject
{
    [Header("MIDI Related & Time Offset Data")]
    public AudioClip songClip;
    
    public float songDelayInSeconds; //delay the song after a certain amount of time
    public double marginOfError;//how much off the player can press the note and still consider to be a hit (in seconds)
    public int inputDelayInMilliseconds; //it's the issue with the keyboard and we need to have input delay 
   
    public string fileLocation; //file location for the MIDI file
    public float noteTime; //how much time the note is going to be on the screen
    public float noteSpawnX; //the X position for the note to be spawned at
    public float noteTapX; //the X position where the player should press the note
    public float noteDespawnX => noteTapX - (noteSpawnX - noteTapX); //De-spawn position for notes
    
    [Header("Note Prefabs")]
    public GameObject noteNormalPrefab;
    public GameObject noteSliderPrefab;

    [Header("Note Hit Locations")] 
    public Vector3 hitPointTopRight;
    public Vector3 hitPointBottomRight;
    public Vector3 hitPointTopLeft;
    public Vector3 hitPointBottomLeft;

    
    [Header("Lane Data")] 
    public const int LaneCount = 4;
    public Melanchall.DryWetMidi.MusicTheory.NoteName noteRestrictionNormalNote;
    public Melanchall.DryWetMidi.MusicTheory.NoteName noteRestrictionSliderNote;

    
    


    [Header("Lane Top Right")]
    public KeyCode inputTopRight;//key input for that lane
    //note on midi file to spawn and note object prefab to spawn
    [Range(0, 8)]
    public int laneOctave_TopRight;
    public List<BaseNoteType> AllNoteOnLaneList_TopRight = new List<BaseNoteType>();//list of timestamps of all the notetypes 
    [Header("Lane Bottom Right")]
    public KeyCode inputBottomRight;//key input for that lane
    //note on midi file to spawn and note object prefab to spawn
    [Range(0, 8)]
    public int laneOctave_BottomRight;
    public List<BaseNoteType> AllNoteOnLaneList_BottomRight = new List<BaseNoteType>();//list of timestamps of all the notetypes 
    [Header("Lane Top Left")]
    public KeyCode inputTopLeft;//key input for that lane
    //note on midi file to spawn and note object prefab to spawn
    [Range(0, 8)]
    public int laneOctave_TopLeft;
    public List<BaseNoteType> AllNoteOnLaneList_TopLeft = new List<BaseNoteType>();//list of timestamps of all the notetypes 
    [Header("Lane Bottom Left")]
    public KeyCode inputBottomLeft;//key input for that lane
    //note on midi file to spawn and note object prefab to spawn
    [Range(0, 8)]
    public int laneOctave_BottomLeft;
    public List<BaseNoteType> AllNoteOnLaneList_BottomLeft = new List<BaseNoteType>();//list of timestamps of all the notetypes 
    private void OnEnable()
    {
        
    }
    
    
}
