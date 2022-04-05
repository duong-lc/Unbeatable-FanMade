using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Character;
using UnityEngine;
using EventTypes;

[RequireComponent(typeof(SideCharacterAnimatorController))]

public class SideCharacterController : MonoBehaviour
{
    //public SideCharacterMaster SideCharacterMaster;

    [SerializeField] private SO_SideCharacter_Data _sideCharacterData;
    [SerializeField] private CharacterType _characterType = CharacterType.SideCharacter1;
    private SpriteRenderer _spriteRenderer;
    private SideCharacterAnimatorController _animController;
    
    public AppearancePriority Priority;
    public bool isBusy = false;

    [SerializeField] private GameObject _sliderObjectHolding;
    private double _currentNoteTimeStamp = 0;

    private float _fadeInTime => _sideCharacterData.fadeInTime;
    private float _fadeOutTime => _sideCharacterData.fadeOutTime;
    private SideCharacterMaster _sideCharacterMaster => SideCharacterMaster.Instance;
    
    public NoteData.LaneOrientation SideCharacterOrientation_DEBUG;
    public NoteData.NoteID currentNoteOccupied_DEBUG = NoteData.NoteID.None;

    [Header("Events")]
    [SerializeField] private SideCharacterAppearanceEvent onSideAppearEvent;

    [SerializeField] private AnimChangeEvent onAnimEvent;
    private void Awake()
    {
        _animController = GetComponent<SideCharacterAnimatorController>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.enabled = true;
        Color color = _spriteRenderer.color;
        _spriteRenderer.color = new Color(color.r, color.g, color.b, 0);
        
    }

    public void SideCharacter_AppearanceListener(SideCharacterAppearance appearanceData)
    {
        if (appearanceData.priority != Priority) return;
        var noteHit = appearanceData.noteHit;
        
        //if all 3 appears and register as busy and another note hits force make the first side character in
        //priority hierarchy reappear despite not fully disappear
        //isBusy while the note hitting isn't the same note that already hit, and success
        if (isBusy && Math.Abs(noteHit.timeStamp - _currentNoteTimeStamp) > 0.05
                   && noteHit.hitStatus == NoteHitStatus.Success 
                   && noteHit.regState != RegisterState.KeyUp
                   && _sliderObjectHolding == null)
        {
           isBusy = false;
        }
        //if note hits successfully on keydown (which is all notes except for slider note)
        if (!isBusy && noteHit.regState == RegisterState.KeyDown 
                    && noteHit.hitStatus == NoteHitStatus.Success)
        {
            EnableSideCharacter(noteHit);
        }
        //if it's a keyUp and the current character is holding the same note object as the slider that they're keyUp-ing
        else if (noteHit.noteObject == _sliderObjectHolding && noteHit.regState == RegisterState.KeyUp)
        {
            DisplaySideCharacter(appearanceData.noteHit);
        }
        //if the current character is busy and not that last character in priority, go to next character in priority
        //or if not busy but current note is a keyUp of a slider note but register with a different character then
        //go to next character in priority until match
        else if (isBusy && Priority != Enum.GetValues(typeof(AppearancePriority)).Cast<AppearancePriority>().Max() || 
                 !isBusy  && noteHit.noteObject != _sliderObjectHolding
                          && noteHit.regState == RegisterState.KeyUp 
                          && noteHit.noteID == NoteData.NoteID.SliderNote)
        {
            appearanceData.priority++;
            onSideAppearEvent.Raise(appearanceData);
        }
        else
        {
            //if any cases that fall outside of intended usage, log out error
            if (noteHit.hitStatus == NoteHitStatus.Success && noteHit.noteID != NoteData.NoteID.SliderNote)
            {
                Debug.LogError(appearanceData.priority + " " + noteHit.regState + " " + noteHit.noteID);
            }
            // Debug.LogError(appearanceData.priority 
            //                + " " + noteHit.regState 
            //                + " " + noteHit.noteID
            //                + " " + noteHit.hitStatus
            //                + " " + isBusy);
        }
    }
    
    private void EnableSideCharacter(NoteHit noteHit)
    {
        isBusy = true;
        SideCharacterOrientation_DEBUG = noteHit.orientation;
        currentNoteOccupied_DEBUG = noteHit.noteID;
        transform.position = noteHit.orientation switch
        {
            NoteData.LaneOrientation.TopRight => _sideCharacterData.topRight,
            NoteData.LaneOrientation.BottomRight => _sideCharacterData.bottomRight,
            NoteData.LaneOrientation.TopLeft => _sideCharacterData.topLeft,
            NoteData.LaneOrientation.BottomLeft => _sideCharacterData.bottomLeft,
            NoteData.LaneOrientation.Undefined => Vector3.zero,
            _ => throw new ArgumentOutOfRangeException()
        };
        DisplaySideCharacter(noteHit);
    }

    private void DisplaySideCharacter(NoteHit noteHit)
    {
        //to make cases for success notes or fail note but when it's slider
        if (noteHit.hitStatus == NoteHitStatus.Fail && noteHit.noteID != NoteData.NoteID.SliderNote) { return; }
        //print($"{noteHit.hitStatus} {noteHit.noteID} ");
        _currentNoteTimeStamp = noteHit.timeStamp;
        
        if (noteHit.noteID == NoteData.NoteID.SliderNote)//slider note
        {
            if (noteHit.regState == RegisterState.KeyUp)//keyup
            {
                if (_sliderObjectHolding == noteHit.noteObject)
                {
                    _sliderObjectHolding = null;
                    AnimChange animData = new AnimChange() 
                    {
                        animationState = AnimState.GroundAttack, 
                        orientation = noteHit.orientation,
                        type = _characterType
                    };
                    onAnimEvent.Raise(animData);
                    isBusy = false;
                    StartCoroutine(DisappearSliderRoutine());
                }
                else
                {
                    Debug.LogError("Key Up Slider Problem");
                }
            }
            else//key down
            {
                if (_sliderObjectHolding == null)
                {
                    _sliderObjectHolding = noteHit.noteObject;
                    AnimChange animData = new AnimChange() 
                    {
                        animationState = AnimState.AirBlock, 
                        orientation = noteHit.orientation,
                        type = _characterType
                    };
                    onAnimEvent.Raise(animData);
                    AppearSlider();
                }
                else
                    Debug.LogError("Key Down Slider Problem");
            }
        }
        else//not slider note
        {
            AnimChange animData = new AnimChange() 
            {
                animationState = AnimState.GroundAttack, 
                orientation = noteHit.orientation,
                type = _characterType
            };
            onAnimEvent.Raise(animData);
            
            StopAllCoroutines();
            StartCoroutine(AppearNormalRoutine());
        }

    }

    private IEnumerator AppearNormalRoutine()
    {
        StartCoroutine(LerpAlphaRoutine(1f, _fadeInTime));
        
        var waitTime = _animController.GetAnimationLength(_animController._currentAnimation);
        yield return new WaitForSeconds(waitTime);
        
        isBusy = false;
        currentNoteOccupied_DEBUG = NoteData.NoteID.None;
        
        StartCoroutine(LerpAlphaRoutine(0f, _fadeOutTime));
    }
    
    

    private void AppearSlider()
    {
        StopAllCoroutines();
        StartCoroutine(LerpAlphaRoutine(1f, _fadeInTime));
    }
    
    private IEnumerator DisappearSliderRoutine()
    {
        //isBusy = false;
        currentNoteOccupied_DEBUG = NoteData.NoteID.None;
        var waitTime = _animController.GetAnimationLength(_animController._currentAnimation);
        yield return new WaitForSeconds(waitTime);
        
        StartCoroutine(LerpAlphaRoutine(0f, _fadeOutTime));
    }

    private IEnumerator LerpAlphaRoutine(float endValue, float fadeTime)
    {
        Color color = _spriteRenderer.color;
        float elapsedTime = 0f;
        float startValue = color.a;

        float ratio = endValue == 0 
            ? (startValue == 0) ? 1 : startValue 
            : (startValue > 0.99f) ? 1 : (1 - startValue);
        //float ratio = 1;
        
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startValue, endValue, elapsedTime / (fadeTime * ratio));
            _spriteRenderer.color = new Color(color.r, color.g, color.b, newAlpha);
            yield return null;
        }
    }
}
