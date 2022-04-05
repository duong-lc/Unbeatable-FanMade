using System;

namespace Character
{
    public class CharacterData
    {
    }
    [Serializable]
    public enum RegisterState
    {
        KeyUp,
        KeyDown
    }
    
    public enum NoteHitStatus
    {
        Undefined,
        Success,
        Fail
    }
    
    [Serializable]
    public enum AppearancePriority
    { 
        First = 0,
        Second = 1,
        Third = 2
    }

    public enum AnimState
    {
        Idle,
        Jump,
        Land,
        AirBlock,
        GroundBlock,
        AirAttack,
        GroundAttack,
        Hurt,
        GroundDash
    }

    public enum CharacterType
    {
        MainCharacter,
        SideCharacter1,
        SideCharacter2,
        SideCharacter3
    }
}
