using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

///<summary>
///handles lerping position of the note from spawn position to despawn position
///</summary>
public class Note : MonoBehaviour
{
    #region Variables
    //public TMPro.TextMeshPro textTimeStampKeyDown;
    double timeInstantiated; //time to instantiate the note
    public double assignedTime;//the time the note needs to be tapped by the player
    private Lane parentLane;

    private Vector3 tapPos;
    public Lane.LaneSide side;//enum to check which side this note belongs to
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        parentLane = transform.parent.GetComponent<Lane>();
        GetComponent<SpriteRenderer>().enabled = false;
        timeInstantiated = SongManager.GetAudioSourceTime();
        
        if(parentLane.currentSide == Lane.LaneSide.leftSide)
        {
            side = Lane.LaneSide.leftSide;
            tapPos = parentLane.leftLaneHitPoint;
        }
        else
        {
            side = Lane.LaneSide.rightSide;
            tapPos = parentLane.rightLaneHitPoint;
        }
            
    }

    // Update is called once per frame
    void Update()
    {
        //lerping note position
        LerpingNotePos();
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

    #region Note Movement Functions
    ///<summary>
    ///Handles moving the note gameobject from spawn position to despawn position
    ///</summary>
    private void LerpingNotePos()
    {
        //localizing instances
        //textTimeStampKeyDown.text = assignedTime.ToString();
        double timeSinceInstantiated = 0;
        timeSinceInstantiated = SongManager.GetAudioSourceTime() - timeInstantiated;
        //divide that with the time between the spawn Y and despawn Y to get the alpha position of the note relative to its total travel dist
        float alpha = (float)(timeSinceInstantiated / (SongManager.Instance.noteTime * 2));

        
        if (alpha > 1)//if alpha > 1, destroy the object
        {
            Destroy(gameObject);
        }
        else//otherwise, the position of note will be lerped between the spawn position and despawn position based on the alpha
        {
            switch(side)
            {
                case Lane.LaneSide.rightSide:
                    transform.position = Vector3.Lerp(tapPos+(Vector3.right * SongManager.Instance.noteSpawnX), tapPos+(Vector3.right * SongManager.Instance.noteDespawnX), alpha); 
                    break;
                case Lane.LaneSide.leftSide:
                    transform.position = Vector3.Lerp(tapPos+(Vector3.left * SongManager.Instance.noteSpawnX), tapPos+(Vector3.left * SongManager.Instance.noteDespawnX), alpha); 
                    break;
            }              
            //transform.localPosition = Vector3.Lerp(startPos.position, Vector3.right * SongManager.Instance.noteDespawnY, alpha); 
            GetComponent<SpriteRenderer>().enabled = true;
        }
    }
    #endregion

    #region Utility Functions
    ///<summary>
    ///fade out enum to fade the node out and destroy if player misses
    ///</summary>
    public IEnumerator FadeOut()
    {
        float dest = 0f, time = 0.06f;
        float curr = GetComponent<SpriteRenderer>().material.color.a;
        for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / time)
        {
            Color newColor = new Color(1, 1, 1, Mathf.Lerp(curr,dest,t));
            GetComponent<SpriteRenderer>().material.color = newColor;
            yield return null;
        }
        Destroy(gameObject);
    }

    #endregion

}
