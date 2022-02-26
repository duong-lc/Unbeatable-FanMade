using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<Summary>
///Handle Note Spawning for sliders
///</Summary>
public class LaneSlider : MonoBehaviour
{
    #region Structs
    [Serializable]
    public struct SliderData
    {
        public double timeStampKeyDown;
        public double timeStampKeyUp;     
    }
    #endregion

    #region Variables
    public Melanchall.DryWetMidi.MusicTheory.NoteName noteRestriction;//restrict note in midi file to certain key
    public KeyCode input;//whatever input for this lane
    public GameObject sliderNotePrefab;//note prefab we're going to spawn in
    public List<SliderData> sliderTimeStamps = new List<SliderData>(); //List of timestamps where the player should press the input to score a hit
    int spawnIndex = 0;//Spawn index to loop through and spawn the sliders
    #endregion

    #region Unity Functions
    ///<Summary>
    ///Setting the array of time stamps where the player should hit the key
    ///</Summary>
    public void SetTimeStamps(Melanchall.DryWetMidi.Interaction.Note[] array)
    {
        //Localized variables
        int i = 0;
        SliderData sliderNoteData = new SliderData();
        //Looping through notes in the midi note array
        foreach(var note in array)//for every note in the note array
        {
            if(note.NoteName == noteRestriction)//check note name if it's on the note restriction 
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
                    //reset counter to start counting for new slider note pair
                    i = 0;
                    //Add the data to the list
                    sliderTimeStamps.Add(sliderNoteData);   
                    //reset the local data for next pair
                    sliderNoteData = new SliderData();
                }
            }
        }
    }

    ///<Summary>
    ///This updates every frame spawn the slider where possible, 
    ///and pass data for Slider Note to process for each slider it spawns
    ///</Summary>    
    void Update()
    {
        if (spawnIndex < sliderTimeStamps.Count)//looping through time stamps to spawn
        {
            //Spawn a slider if the spawn time of first note of the slider is reached
            if(SongManager.GetAudioSourceTime() >= sliderTimeStamps[spawnIndex].timeStampKeyDown - SongManager.Instance.noteTime)
            {
                //Spawn a slider prefab
                // var sliderNote = Instantiate(sliderNotePrefab, transform);
                // //Passing data to the newly spawned slider 
                // sliderNote.GetComponent<SliderNote>().data = sliderTimeStamps[spawnIndex];
                // spawnIndex++;//increment to next note
            }
        }
    }
    #endregion

    #region Utility Functions 
    public void Hit()
    {
        ScoreManager.Hit();
    }
    public void Miss()
    {
        ScoreManager.Miss();
    }
    #endregion
}
