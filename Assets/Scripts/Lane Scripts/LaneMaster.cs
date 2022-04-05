using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Diagnostics;
using Melanchall.DryWetMidi.MusicTheory;
using UnityEngine.Serialization;

public class LaneMaster : MonoBehaviour
{
    [SerializeField] private SO_Midi_Data _midiData;
    public static LaneMaster Instance;
    
    private List<int> _ignoreIndexList = new List<int>();    
    
    // Start is called before the first frame update
    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Using the Midi data after the the Midi file has been loaded
    /// - Make an array that uses the note datas from the midi file
    /// </summary>
    public void CompileDataFromMidi(MidiFile midiFile)
    {
        var notes = midiFile.GetNotes(); //getting ICollection of the notes
        var array = new Melanchall.DryWetMidi.Interaction.Note[notes.Count];//making a new array to access the notes
        notes.CopyTo(array, 0);//copy the note data from ICollection to the array

        SetTimeStampsAllLane(array);
        
        //distribute the list to 4 lanes.
        DistributeNoteToLane();
    }


    private void SetTimeStampsAllLane(Melanchall.DryWetMidi.Interaction.Note[] array)
    {
        //localized vars for slider note array
        //int i = 0;
        for (int index = 0; index < array.Length; index++) //for every note in the note array
        {
            if (_ignoreIndexList.Contains(index)) continue;
            //Top Right Lane
            
            if (array[index].Octave == _midiData.laneOctave_TopRight)
            {
                AddNoteToLane(index, array, _midiData.AllNoteOnLaneList_TopRight, NoteData.LaneOrientation.TopRight, _midiData.laneOctave_TopRight);
            }
            else if (array[index].Octave == _midiData.laneOctave_BottomRight)
            {
                AddNoteToLane(index, array, _midiData.AllNoteOnLaneList_BottomRight, NoteData.LaneOrientation.BottomRight, _midiData.laneOctave_BottomRight);
            }
            else if (array[index].Octave == _midiData.laneOctave_TopLeft)
            {
                AddNoteToLane(index, array, _midiData.AllNoteOnLaneList_TopLeft, NoteData.LaneOrientation.TopLeft, _midiData.laneOctave_TopLeft);
            }
            else if (array[index].Octave == _midiData.laneOctave_BottomLeft)
            {
                AddNoteToLane(index, array, _midiData.AllNoteOnLaneList_BottomLeft, NoteData.LaneOrientation.BottomLeft, _midiData.laneOctave_BottomLeft);
            }
           
        }
    }

    private void AddNoteToLane(int index, Melanchall.DryWetMidi.Interaction.Note[] array, List<BaseNoteType> laneToAdd, NoteData.LaneOrientation orientation, int octaveIndex)
    {
        if (array[index].NoteName == _midiData.noteRestrictionNormalNote)
        {
            AddNormalNoteToList(index, array, laneToAdd, orientation, octaveIndex);//adding to TopRightLaneList
        }
        else if (array[index].NoteName == _midiData.noteRestrictionSliderNote)
        {
            AddSliderNoteToList(index, array, laneToAdd, orientation, octaveIndex);//adding to TopRightLaneList
        }
    }
    
    private void AddNormalNoteToList(int index, Melanchall.DryWetMidi.Interaction.Note[] array, List<BaseNoteType> allNoteOnLaneList, NoteData.LaneOrientation orientation, int octaveIndex)
    {
        //get the time stamp for that note. (note.Time does not return the format we want as it uses time stamp temp map in midi, so conversion is needed)
        var metricTimeSpan = TimeConverter.ConvertTo<MetricTimeSpan>(array[index].Time, SongManager.MidiFile.GetTempoMap());
        NoteNormalType noteNormalLocal = new NoteNormalType
        {
            octaveNum = octaveIndex,
            noteID = NoteData.NoteID.DefaultNote,
            timeStamp = (double)metricTimeSpan.Minutes * 60f + metricTimeSpan.Seconds + (double)metricTimeSpan.Milliseconds / 1000f
        };
        //Assigning note's lane orientation
        noteNormalLocal.laneOrientation = orientation;
        //adding the time stamp (in seconds) to the array of time stamp
        allNoteOnLaneList.Add(noteNormalLocal);  
    }

    private void AddSliderNoteToList(int index, Melanchall.DryWetMidi.Interaction.Note[] array, List<BaseNoteType> allNoteOnLaneList, NoteData.LaneOrientation orientation, int octaveIndex)
    {
        //get the time stamp for that note. (note.Time does not return the format we want as it uses time stamp temp map in midi, so conversion is needed)
        var metricTimeSpan = TimeConverter.ConvertTo<MetricTimeSpan>(array[index].Time, SongManager.MidiFile.GetTempoMap());
        /*Instead of relying on local val instantiated outside foreach loop
        create another loop inside when see current is slider note. Keep looping in the 
        note stream until you hit a note with the same octave and note restriction. That will
        be the end note
        
        Create a ignore note list so if the outer loop hit the same end note from the slider note
        it will ignore and continue
        */
        //For each 2 notes which is a slider, reset data for new slider note
        NoteData.SliderData sliderNoteData = new NoteData.SliderData
        {
            timeStampKeyDown = (double)metricTimeSpan.Minutes * 60f + metricTimeSpan.Seconds + (double)metricTimeSpan.Milliseconds / 1000f
        };
        for (int j = index+1; j < array.Length; j++)
        {
            //Check for next note on the same octave and on same line
            if (array[j].Octave != octaveIndex || array[j].NoteName != _midiData.noteRestrictionSliderNote) continue;
            var metricTimeSpan2 = TimeConverter.ConvertTo<MetricTimeSpan>(array[j].Time, SongManager.MidiFile.GetTempoMap());
            sliderNoteData.timeStampKeyUp = (double)metricTimeSpan2.Minutes * 60f + metricTimeSpan2.Seconds + (double)metricTimeSpan2.Milliseconds / 1000f;
            NoteSliderType noteSliderLocal = new NoteSliderType
            {
                octaveNum = octaveIndex,
                noteID = NoteData.NoteID.SliderNote,
                sliderData = sliderNoteData,
                //Assigning note's lane orientation
                laneOrientation = orientation
            };
            allNoteOnLaneList.Add(noteSliderLocal);
            _ignoreIndexList.Add(j);
            break;
        }
        
    }
    
    
    public void DistributeNoteToLane()
    {
        Lane[] laneArray = GetComponentsInChildren<Lane>();
        foreach (Lane lane in laneArray)
        {
            if (lane.CompareTag("TopRight_Lane"))
            {
                lane.SetLocalListOnLane(_midiData.AllNoteOnLaneList_TopRight);
            }
            else if (lane.CompareTag("BottomRight_Lane"))
            {
                lane.SetLocalListOnLane(_midiData.AllNoteOnLaneList_BottomRight);
            }
            else if (lane.CompareTag("TopLeft_Lane"))
            {
                lane.SetLocalListOnLane(_midiData.AllNoteOnLaneList_TopLeft);
            }
            else if (lane.CompareTag("BottomLeft_Lane"))
            {
                lane.SetLocalListOnLane(_midiData.AllNoteOnLaneList_BottomLeft);
            }
        }
    }
}
