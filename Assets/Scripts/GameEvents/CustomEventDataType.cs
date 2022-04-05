using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Character;
using UnityEngine.Serialization;

namespace EventTypes
{
    [Serializable] public struct Void { }

    [Serializable]
    public struct NoteHit
    {
        public NoteData.LaneOrientation orientation;
        public NoteData.NoteID noteID;
        public RegisterState regState;
        public GameObject noteObject;
        public double timeStamp;
        //determine if note hit successfully or failed
        public NoteHitStatus hitStatus;
    }

    [Serializable]
    public struct AnimChange
    {
        public AnimState animationState;
        public NoteData.LaneOrientation orientation;
        public CharacterType type;
    }

    [Serializable]
    public struct SideCharacterAppearance
    {
        public NoteHit noteHit;
        public AppearancePriority priority;
    }
}
