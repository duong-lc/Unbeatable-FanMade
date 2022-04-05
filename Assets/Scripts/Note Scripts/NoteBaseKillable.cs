using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Random = System.Random;
public abstract class NoteBaseKillable : NoteBase
{
    public Vector3 knockBackVector;
    public Vector3 fallVector;
    public float rotate1, rotate2;
    private bool runOnce = true;
    private void DoHitKnockBack()
    {
        DOTween.Sequence()
            .Append(transform.DOMove(transform.position + knockBackVector, noteData.time1).SetEase(noteData.moveEase1))
            .Append(transform.DOMove(transform.position + knockBackVector + fallVector, noteData.time2).SetEase(noteData.moveEase2));
    }

    private void DoHitRotateKnockBack()
    {
        //print($"{rotate1} {rotate2}");
        DOTween.Sequence()
            .Append(transform.DORotateQuaternion( Quaternion.Euler(0,0,rotate1), noteData.time1).SetEase(noteData.rotateEase1))
            .Append(transform.DORotateQuaternion(Quaternion.Euler(0,0,rotate2), noteData.time2).SetEase(noteData.rotateEase2));
    }

    private IEnumerator WaitAndDestroyRoutine()
    {
        yield return new WaitForSeconds(noteData.time1 + noteData.time2);
        Destroy(gameObject);
    }

    protected void OnHit()
    {
        if (!runOnce) return;
        //print($"onHit");
        runOnce = false;
        CanMove = false;
        DoHitKnockBack();
        DoHitRotateKnockBack();
        StartCoroutine(WaitAndDestroyRoutine());

    }

    protected void UpdateKnockBackAttributes()
    {
        float xOffset = GetRandomOffset(0.2f);
        float yOffset = GetRandomOffset(0.1f);
        float zAngleOffset = GetRandomOffset(10f);
        //float zAngleOffset = 0;
        switch (noteOrientation)
        {
            case NoteData.LaneOrientation.TopRight:
                knockBackVector = noteData.airKnockBackVector + new Vector3(xOffset, yOffset, 0);
                fallVector = noteData.airFallVector + new Vector3(0, yOffset, 0);
                rotate1 = noteData.rotateAmount1 + zAngleOffset;
                rotate2 = noteData.rotateAmount2 - zAngleOffset;
                break;
            case NoteData.LaneOrientation.BottomRight:
                knockBackVector = noteData.groundKnockBackVector + new Vector3(xOffset, yOffset, 0);
                fallVector = noteData.groundFallVector + new Vector3(0, yOffset, 0);
                rotate1 = noteData.rotateAmount1 + zAngleOffset;
                rotate2 = noteData.rotateAmount2 - zAngleOffset;
                break;
            case NoteData.LaneOrientation.TopLeft:
                knockBackVector = new Vector3(-noteData.airKnockBackVector.x + xOffset, noteData.airKnockBackVector.y + yOffset, noteData.airKnockBackVector.z);
                fallVector = new Vector3(-noteData.airFallVector.x, noteData.airFallVector.y + yOffset, noteData.airFallVector.z);
                rotate1 = -noteData.rotateAmount1 + zAngleOffset;
                rotate2 = -noteData.rotateAmount2 - zAngleOffset;
                break;
            case NoteData.LaneOrientation.BottomLeft:
                knockBackVector = new Vector3(-noteData.groundKnockBackVector.x + xOffset, noteData.groundKnockBackVector.y + yOffset, noteData.groundKnockBackVector.z);
                fallVector = new Vector3(-noteData.groundFallVector.x, noteData.groundFallVector.y + yOffset, noteData.groundFallVector.z);
                rotate1 = -noteData.rotateAmount1 + zAngleOffset;
                rotate2 = -noteData.rotateAmount2 - zAngleOffset;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static float GetRandomOffset(float offsetRange)
    {
        Random rand = new Random();
        double min = -Mathf.Abs(offsetRange);
        double max = Mathf.Abs(offsetRange);
        double range = max - min;

        double sample = rand.NextDouble();
        double scaled = (sample * range) + min;
        return (float)scaled;
    }
}
