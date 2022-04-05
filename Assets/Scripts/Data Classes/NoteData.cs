using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<Summary>
///'NoteType' is the parent note class type that all notes will be derived from.
///</Summary>
[Serializable]
public abstract class BaseNoteType
{
    public NoteData.NoteID noteID;//ID of current note
    public GameObject noteObj;//the game object it's possessing
    public NoteData.LaneOrientation laneOrientation = NoteData.LaneOrientation.Undefined;
    //Debug
    public int octaveNum;
}

///<Summary>
///'NoteNormalType' is a children class of 'NoteType'.
///</Summary>
[Serializable]
public class NoteNormalType : BaseNoteType
{
    //timestamp of when the note should be tapped
    public double timeStamp;
}
///<Summary>
///'NoteSliderType' is a children class of 'NoteType'.
///Contain struct "SlideData" which has 2 timestamps - KeyDownEvent timestamp and KeyUpEvent timestamp
///</Summary>
[Serializable]
public class NoteSliderType: BaseNoteType
{
    public NoteData.SliderData sliderData = new NoteData.SliderData();//timestamp for slider note
}

[Serializable]
public class NoteData : MonoBehaviour
{
    //NoteID enum to check for noteType
    [Serializable]
    public enum NoteID
    {
        None, DefaultNote, SliderNote
    }
    
    [Serializable]
    public struct SliderData
    {
        public double timeStampKeyDown;
        public double timeStampKeyUp;
    }

    [Serializable]
    public enum LaneOrientation
    {
        TopRight,
        BottomRight,
        TopLeft,
        BottomLeft,
        Undefined
    }
}
