using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Main Character Data", order = 2)]
public class SO_MainCharacter_Data : ScriptableObject
{
    [Header("Animation Names")]
    public string AttackAirBlockIntro = "BeatCombatAirBlockIntro";
    public string AttackAirBlockLoop = "BeatCombatAirBlockLoop";
    public string AttackGroundBlockIntro = "BeatCombatGroundBlockIntro";
    public string AttackGroundBlockLoop = "BeatCombatGroundBlockLoop";
    [Space]
    public string Attack1 = "BeatCombatAttack01";
    public string Attack2 = "BeatCombatAttack02";
    public string Attack3 = "BeatCombatAttack03";
    public string Attack4 = "BeatCombatAttack04";
    public string Attack5 = "BeatCombatAttack05";
    public string Attack6 = "BeatCombatAttack06";
    public string Attack7 = "BeatCombatAttack07";
    public string Attack8 = "BeatCombatAttack08";
    public string Attack9 = "BeatCombatAttack09";
    public string Attack10 = "BeatCombatAttack10";
    [Space]
    //Combat Hurt Animation Names
    public string Hurt1 = "BeatCombatHurt01";
    public string Hurt2 = "BeatCombatHurt02";
    public string Hurt3 = "BeatCombatHurt03";
    [Space]
    //Combat jump Animation Names
    public string Jump1 = "BeatCombatJump01";
    public string JumpLand1 = "BeatCombatJump01Land";
    public string Jump2 = "BeatCombatJump02";
    public string JumpLand2 = "BeatCombatJump02Land";
    [Space]
    //Other Animation Names
    public string Idle = "BeatCombatIdle";
    public string Intro = "BeatCombatIntro";

    [Header("Character Hit Points")] 
    public Vector3 topRight;
    public Vector3 bottomRight;
    public Vector3 topLeft;
    public Vector3 bottomLeft;
    [Space]
    public float groundHitOffset = 0.5f;
    
    [Header("Misc Properties")] 
    public float gravityScale = 0.275f;
    public float gravityAcceleration = 9.8f;
    public float jumpForce = 15f;
    public float dampRatio = 0.85f;
    public float dampSecondsInterval = 0.015f;
    public float heightLimitMarginOfError = 0.1f;
    public float marginOverloadMultiplierEntry = 5f;
    public float cooldownGap = 0.090f;
    public float airFreezeMultiplier = 3;
    public float lerpTimeDash = 0.3f;
}
