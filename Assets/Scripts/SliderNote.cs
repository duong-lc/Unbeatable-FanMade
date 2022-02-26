using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

///<summary>
///Handles lerping position from Spawn to End
///Handles Key Input
///</summary>
public class SliderNote : MonoBehaviour
{
    #region Variables
    //public TMPro.TextMeshPro textTimeStampKeyDown, textTimeStampKeyUp;
    public NoteSliderType.SliderData data;//Data of hitting time stamp to activate
    public GameObject startNote, endNote;//game object of start and end note
    public double startNoteSpawnTime, endNoteSpawnTime;//Spawn time stamp of start and end notes
    KeyCode input;//key input of the slider
    Lane parentLane;//Spawner ref
    private Vector3 tapPos;
    public Lane.LaneSide side;//enum to determine which side this note belongs to
    [SerializeField] private Transform[] points;
    public LR_LineController lineController;

    public bool isStartNoteHitCorrectly = false, isEndNoteHitCorrectly = false, isHolding = false;//booleans to check conditions to register notes
    public bool isStartNoteMovable = true, isEndNoteMovable = true;
    #endregion
    

    ///<Summary>
    ///Setting up Spawn times, key inputs, re-parenting for accurate lerp speed
    ///</Summary>
    void Start()
    {
        lineController.SetUpLine(points);
        parentLane = gameObject.transform.parent.GetComponent<Lane>(); 
        side = parentLane.currentSide;
        //setting spawn time stamp
        startNoteSpawnTime = data.timeStampKeyDown - SongManager.Instance.noteTime;
        endNoteSpawnTime = data.timeStampKeyUp - SongManager.Instance.noteTime;
        //Setting preSpawn position for line renderer and 
        switch(side)
        {
            case Lane.LaneSide.rightSide:
                side = Lane.LaneSide.rightSide;
                tapPos = parentLane.rightLaneHitPoint;
                startNote.transform.position = tapPos+(Vector3.right * SongManager.Instance.noteSpawnX);
                endNote.transform.position = tapPos+(Vector3.right * SongManager.Instance.noteSpawnX);
                break;
            case Lane.LaneSide.leftSide:
                side = Lane.LaneSide.leftSide;
                tapPos = parentLane.leftLaneHitPoint;
                startNote.transform.position = tapPos+(Vector3.left * SongManager.Instance.noteSpawnX);
                endNote.transform.position = tapPos+(Vector3.left * SongManager.Instance.noteSpawnX);
                break;
        }
        
        //deactivate at start
        startNote.SetActive(false); 
        endNote.SetActive(false);
        lineController.gameObject.SetActive(false);
        //setting reference and key input
        //laneSlider = gameObject.transform.parent.GetComponent<LaneSlider>();
        input = gameObject.transform.parent.GetComponent<Lane>().input; 
    }

    ///<Summary>
    ///Update and handle lerping start note and end note from spawn to hit point. 
    ///Handlling Key Input, determine hit and miss condition of slider
    ///</Summary>
    void Update()
    {
        //Lerping Start Note
        LerpingStartNotePos();
        //Lerping End Note
        LerpingEndNotePos();
    }

    private void OnDestroy() 
    {
        switch(parentLane.gameObject.tag)
        {
            case "UpperLane":
                if(parentLane.player.beatCurrentLane == CharacterHandler.currentLaneOccupied.upperLane)
                    parentLane.player.beatCurrentLane = CharacterHandler.currentLaneOccupied.none;
                break;
            case "LowerLane":
                if(parentLane.player.beatCurrentLane == CharacterHandler.currentLaneOccupied.lowerLane)
                        parentLane.player.beatCurrentLane = CharacterHandler.currentLaneOccupied.none;
                break;
        }
    }
    

    #region Note Movemennt Functions
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
            float alphaStart = (float)(timeSinceStartNoteInstantiated / (SongManager.Instance.noteTime * 2));
            
            //if it reaches the despawn position
            if (alphaStart > 1)//if alpha > 1, destroy the object
            {
                Destroy(startNote);
            }
            else if( startNote != null)//otherwise, the position of note will be lerped between the spawn position and despawn position based on the alpha
            {
                //checking which side the note is spawned on
                switch(side) 
                {
                    case Lane.LaneSide.rightSide://if the note is spawned on the right side
                        //lerp the note from right to left
                        startNote.transform.position = Vector3.Lerp(tapPos+(Vector3.right * SongManager.Instance.noteSpawnX), tapPos+(Vector3.right * SongManager.Instance.noteDespawnX), alphaStart); 
                        break;
                    case Lane.LaneSide.leftSide://if the note is spawned on the left side
                        //Lerp the note from left to right
                        startNote.transform.position = Vector3.Lerp(tapPos+(Vector3.left * SongManager.Instance.noteSpawnX), tapPos+(Vector3.left * SongManager.Instance.noteDespawnX), alphaStart); 
                        break;
                }  
                //activate the note and show the line renderer
                startNote.SetActive(true);
                lineController.gameObject.SetActive(true);
            }
        }
    }

    ///<Summary>
    ///Lerping position of endnote from spawn to despawn position
    ///</Summary>
    private void LerpingEndNotePos()
    {
        //when the end note is spawnable/movable
        if(SongManager.GetAudioSourceTime() >= endNoteSpawnTime && isEndNoteMovable)
        {
            //getting the current time stamp since the song is played
            double timeSinceEndNoteInstantiated = SongManager.GetAudioSourceTime() - endNoteSpawnTime;
            //divide that with the time between the spawn Y and despawn Y to get the alpha position of the note relative to its total travel dist
            float alphaEnd = (float)(timeSinceEndNoteInstantiated / (SongManager.Instance.noteTime * 2));
            //if it reaches the despawn position
            if (alphaEnd > 1)//if alpha > 1, destroy the object
            {
                Destroy(gameObject);
                Destroy(endNote);
            }
            else if (endNote != null)//otherwise, the position of note will be lerped between the spawn position and despawn position based on the alpha
            { 
                //checking which side the note spawned on
                switch(side)
                {
                    case Lane.LaneSide.rightSide://if the note is on the right side
                        if(endNote.transform.position.x > tapPos.x)//as long as the note is to the right of the right side hit point
                        {
                            //lerp the position to the despawn position
                            endNote.transform.position = Vector3.Lerp(tapPos+(Vector3.right * SongManager.Instance.noteSpawnX), tapPos+(Vector3.right * SongManager.Instance.noteDespawnX), alphaEnd); 
                            endNote.SetActive(true);
                        }else
                        {
                            //if it reaches the hit point position, halt the movement
                            endNote.transform.position = tapPos+(Vector3.right * SongManager.Instance.noteSpawnX);
                            isEndNoteMovable = false;
                        }
                        break;
                    case Lane.LaneSide.leftSide://if the note is on the left side
                        if(endNote.transform.position.x < tapPos.x)//as long as the note is to the left of left side hit point
                        {
                            //lerp the positioon to despawn position
                            endNote.transform.position = Vector3.Lerp(tapPos+(Vector3.left * SongManager.Instance.noteSpawnX), tapPos+(Vector3.left * SongManager.Instance.noteDespawnX), alphaEnd); 
                            endNote.SetActive(true);
                        }else
                        {
                            //if it reaches the hit point position, halt the movement
                            endNote.transform.position = tapPos+(Vector3.left * SongManager.Instance.noteSpawnX);
                            isEndNoteMovable = false;
                        }
                        break;
                }
            }
        }
    }

    #endregion
}
