using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Character;
using UnityEngine;
using EventTypes;

public class SideCharacterMaster : MonoBehaviour
{
    public static SideCharacterMaster Instance;
    [SerializeField] public SideCharacterController[] sideCharactersArray;
    
    //list gets cleared every 10ms - time to auto clear listeners list
    //[SerializeField] private List<NoteHit> _listenerList = new List<NoteHit>();
    
    private void Awake()
    {
        Instance = this;
        
        sideCharactersArray = GetComponentsInChildren<SideCharacterController>();
        //re-organize the array
        for (int i = 0; i < sideCharactersArray.Length; ++i)
        {
            var newSideChar = sideCharactersArray[i];
            int newIndex = (int) newSideChar.Priority;
    
            var oldSideChar = sideCharactersArray[newIndex];
            sideCharactersArray[newIndex] = newSideChar;
            sideCharactersArray[i] = oldSideChar;
        }
    }
}
