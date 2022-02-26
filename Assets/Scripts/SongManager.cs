using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System.IO;
using UnityEngine.Networking;
using System;

///<summary>
///this class handles music player, data extract of midi files, speed of notes on screen
///</summary>
public class SongManager : MonoBehaviour
{
    #region Variables
    //public TMPro.TextMeshPro currTime;
    public static SongManager Instance; //instance variable to be accessed by other classes
    public AudioSource audioSource; //audio source to play the song
    public Lane[] lanes;//array of lanes
    //public LaneSlider[] sliderLanes;//array of slider lanes 
    public float songDelayInSeconds; //delay the song after a certain amount of time
    public double marginOfError;//how much off the player can press the note and still consider to be a hit (in seconds)
    public int inputDelayInMilliseconds; //it's the issue with the keyboard and we need to have input delay 
   
    public string fileLocation; //file location for the MIDI file
    public float noteTime; //how much time the note is going to be on the screen
    public float noteSpawnX; //the X position for the note to be spawned at
    public float noteTapX; //the X position where the player should press the note

    public float noteDespawnX
    {
        get
        {
            return noteTapX - (noteSpawnX-noteTapX);
        }
    }
    public static MidiFile midiFile;//static ref to midi file, this is where it will load on run

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        /*
        checking if the "StreamingAssets" path is a website or not, depending on the platform that loads the midi file
                for example, windows, mac, linux = file location where as webgl = website
        if not look in local folder
        */ 
        if (Application.streamingAssetsPath.StartsWith("http://") || Application.streamingAssetsPath.StartsWith("https://")){
            StartCoroutine(ReadFromWebsite());//start coroutine to wait to load
        }else{
            ReadFromFile();
        }
    }

    /// <summary>
    /// Read Midi file from website, auto request through coroutine.
    /// </summary>
    private IEnumerator ReadFromWebsite()
    {
        //requesting unity web request the midi file
        using (UnityWebRequest www = UnityWebRequest.Get(Application.streamingAssetsPath + "/" + fileLocation)){
            yield return www.SendWebRequest();
            
            //checking to see if there's any network errors
            if(www.isNetworkError || www.isHttpError)
            {
                Debug.LogError(www.error);
            }
            //if no error, load result from data
            //then send these results to memory stream
            //then load the stream onto midi file
            else
            {
                byte[] results = www.downloadHandler.data;
                using (var stream = new MemoryStream(results))
                {
                    midiFile = MidiFile.Read(stream);
                    GetDataFromMidi();
                }
            }
        }
    }
    
    /// <summary>
    /// Read Midi file from local file dir.
    /// </summary>
    private void ReadFromFile()
    {
        midiFile = MidiFile.Read(Application.streamingAssetsPath + "/" + fileLocation);
        GetDataFromMidi();
    }


    /// <summary>
    /// Using the Midi data after the the Midi file has been loaded
    /// - Make an array that uses the note datas from the midi file
    /// - Invoke function start song (have delay to start song)
    /// </summary>
    public void GetDataFromMidi()
    {
        var notes = midiFile.GetNotes(); //getting ICollection of the notes
        var array = new Melanchall.DryWetMidi.Interaction.Note[notes.Count];//making a new array to access the notes
        notes.CopyTo(array, 0);//copy the note data from ICollection to the array

        foreach (var lane in lanes)
            lane.SetTimeStamps(array);
            
        // foreach (var sliderLane in sliderLanes)
        //     sliderLane.SetTimeStamps(array);


        //StartCoroutine(StartSong(songDelayInSeconds));
        Invoke("StartSong", songDelayInSeconds);
    }


    ///<summary>
    ///This function plays the audio source
    ///</summary>  
    public void StartSong()
    {
        audioSource.Play();
    }
    
    ///<summary>
    ///This is an utility function to return the audio source time
    ///Instead of using "AudioSource.time" we returning a double of playback pos in PCM sample divided by freq (Hz) for the accuracy 
    ///</summary>
    public static double GetAudioSourceTime()
    {
        return (double)Instance.audioSource.timeSamples / Instance.audioSource.clip.frequency;
    }

}
