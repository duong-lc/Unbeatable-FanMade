using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Side Character Data", order = 3)]
public class SO_SideCharacter_Data : ScriptableObject
{
    [Header("Animation State Names")] 
    public string BlockLoop = "QuaverCombatBlockLoop";
    public string Attack01 = "QuaverCombatAttack01";
    public string Attack02 = "QuaverCombatAttack02";
    
    [Header("Character Hit Points")] 
    public Vector3 topRight,
        bottomRight,
        topLeft,
        bottomLeft;

    [Header("Appearance Attributes")] 
    public float fadeInTime;
    public float fadeOutTime;
    //public float remainTime;
}
