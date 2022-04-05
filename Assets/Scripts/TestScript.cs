using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TestScript : MonoBehaviour
{    
    public Vector3 test;
    public Vector3 down;
    
    public Ease _moveEase1;
    public Ease _moveEase2;
    
    public float _time1;
    public float _time2;
    
    private Vector3 _originalPos;
    private Quaternion _originalQuaternion;
    
    public Ease rotateEase1;
    public Ease rotateEase2;

    // // Update is called once per frame
    private void Start()
    {
        //_velocity = Vector3.right;
        _originalPos = transform.position;
        _originalQuaternion = transform.rotation;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            DoStunKnockback();
            DoRotateKnockBack();
        }
        else if (Input.GetMouseButtonDown(1))
        {
            transform.position = _originalPos;
            transform.rotation = _originalQuaternion;
        }
    }
    

    private void DoStunKnockback()
    {
        DOTween.Sequence()
            .Append(transform.DOMove( transform.position + test, _time1).SetEase(_moveEase1))
            .Append(transform.DOMove(transform.position + test + down, _time2).SetEase(_moveEase2));

        // float alpha = 0;
        // while (alpha < 1)
        // {
        //     print($"{alpha}");
        //     alpha += Time.deltaTime;
        //     //alpha = EasingFunction.EaseOutCubic(_originalPos.y, test.y, alpha);
        //     transform.position = Vector3.SmoothDamp(transform.position, test, ref _velocity, 0.3f);
        //     yield return null;
        // }
    }

    private void DoRotateKnockBack()
    {
        DOTween.Sequence()
            .Append(transform.DORotateQuaternion( Quaternion.Euler(0,0,30), _time1).SetEase(rotateEase1))
            .Append(transform.DORotateQuaternion(Quaternion.Euler(0,0,60), _time2).SetEase(rotateEase1));
    }
}
