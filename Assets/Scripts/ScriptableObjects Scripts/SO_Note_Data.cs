using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Note Data", order = 4)]
public class SO_Note_Data : ScriptableObject
{
    [Header("KnockBack Logic")]
    
    public Vector3 groundKnockBackVector;
    public Vector3 groundFallVector;
    
    public Vector3 airKnockBackVector;
    public Vector3 airFallVector;
    
    public Ease moveEase1;
    public Ease moveEase2;

    public float time1;
    public float time2;

    [Header("Rotate Logic")]
    
    public Ease rotateEase1;
    public Ease rotateEase2;

    public float rotateAmount1;
    public float rotateAmount2;

    [Header("Default Note Animation Names")]
    public string DefaultNoteGroundRun = "DefaultNoteGroundRun";
    public string DefaultNoteGroundHit01 = "DefaultNoteGroundHit01";
    public string DefaultNoteGroundHit02 = "DefaultNoteGroundHit02";
    [Space]
    public string DefaultNoteAirRun = "DefaultNoteAirRun";
    public string DefaultNoteAirHit01 = "DefaultNoteAirHit01";
    public string DefaultNoteAirHit02 = "DefaultNoteAirHit02";

    [Header("Slider Note Animation Names")]
    public string SliderNoteFrontRun = "SliderNoteFrontRun";
    public string SliderNoteFrontBlock = "SliderNoteFrontBlock";
    public string SliderNoteFrontHit01 = "SliderNoteFrontHit01";
    public string SliderNoteFrontHit02 = "SliderNoteFrontHit02";
}