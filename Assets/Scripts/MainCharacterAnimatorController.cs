using System;
using System.Collections.Generic;
using EventTypes;
using UnityEngine;
using Character;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]

public class MainCharacterAnimatorController : MonoBehaviour
{
    [SerializeField] private SO_MainCharacter_Data _mainCharacterData;
    [SerializeField] private CharacterType _characterType = CharacterType.MainCharacter;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private RuntimeAnimatorController _runtimeAC;
    
    private string _currentAnimation;
    
    
    private List<string> _listGroundAttackAnim;
    private List<string> _listAirAttackAnim;
    private List<string> _listJumpAnim;
    private List<string> _listLandAnim;
    private List<string> _listHurtAnim;
    
    private float _timeEnd;
    private bool _idleCheckOnce = false;
    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _runtimeAC = _animator.runtimeAnimatorController;
        
        _listGroundAttackAnim = new List<string>()
        {
            _mainCharacterData.Attack1,
            _mainCharacterData.Attack2,
            _mainCharacterData.Attack3,
            _mainCharacterData.Attack4,
            _mainCharacterData.Attack5,
            _mainCharacterData.Attack6,
            _mainCharacterData.Attack7
        };
        
        _listAirAttackAnim = new List<string>()
        {
            _mainCharacterData.Attack1,
            _mainCharacterData.Attack2,
            _mainCharacterData.Attack3,
            _mainCharacterData.Attack5,
            _mainCharacterData.Attack7,
            _mainCharacterData.Attack8,
            _mainCharacterData.Attack9,
            _mainCharacterData.Attack10
        };

        _listJumpAnim = new List<string>()
        {
            _mainCharacterData.Jump1,
            _mainCharacterData.Jump2
        };
        
        _listLandAnim = new List<string>()
        {
            _mainCharacterData.JumpLand1,
            _mainCharacterData.JumpLand2
        };
        
        _listHurtAnim = new List<string>()
        {
            _mainCharacterData.Hurt1,
            _mainCharacterData.Hurt2,
            _mainCharacterData.Hurt3
        };
    }

    private void FixedUpdate()
    {
        if (_idleCheckOnce && MainCharacterController.Instance.isGrounded && !MainCharacterController.Instance.isBusy)
        {
            if (Time.time >= _timeEnd)
            {
                FlipSprite(MainCharacterController.Instance.PlayerOrientation);
                PlayIdleAnimation();
                _idleCheckOnce = false;
            }
        }
    }
    
    //listener: listen to events call from Lane.cs and MainCharacterController.cs

    //standard ground attack |01, 02, 03, 04, 05, 06, 07
    //standard air attack | attack 01, 02, 03, 05, 07, 08, 09, 10
    //jump - fall - land 2 variations ||| jump01 land01 jump02 land02
    //hurt / miss
    //ground hold / air hold
    //dash down from air | attack 04, 
    
    public void AnimationChangeEvent_Listener(AnimChange data)
    {
        if (data.type != _characterType) return;
        
        FlipSprite(data.orientation);
        switch (data.animationState)
        {
            case AnimState.GroundAttack:
                _timeEnd = Time.time +  PlayAnimationFromPlayList(_listGroundAttackAnim);
                _idleCheckOnce = true;
                break;
            case AnimState.AirAttack:
                _timeEnd = Time.time + PlayAnimationFromPlayList(_listAirAttackAnim);
                _idleCheckOnce = true;
                break;
            case AnimState.Jump:
                _idleCheckOnce = false;
                PlayAnimationFromPlayList(_listJumpAnim);
                break;
            case AnimState.GroundDash:
                _timeEnd = Time.time + PlayDashDownAnimation();
                _idleCheckOnce = true;
                break;
            case AnimState.Land:
                _timeEnd = Time.time + PlayAnimationFromPlayList(_listLandAnim);
                _idleCheckOnce = true;
                break;
            case AnimState.GroundBlock:
                _idleCheckOnce = false;
                PlayGroundBlockAnimation();
                break;
            case AnimState.AirBlock:
                _idleCheckOnce = false;
                PlayAirBlockAnimation();
                break;
            case AnimState.Hurt:
                _timeEnd = Time.time + PlayAnimationFromPlayList(_listHurtAnim);
                _idleCheckOnce = true;
                break;
            case AnimState.Idle:
                // _timeEnd = Time.time + PlayAnimationFromPlayList(_listHurtAnim);
                PlayIdleAnimation();
                _idleCheckOnce = true;
                break;
        }
    }

    private void FlipSprite(NoteData.LaneOrientation orientation)
    {
        switch (orientation)
        {
            case NoteData.LaneOrientation.TopRight:
            case NoteData.LaneOrientation.BottomRight:
                _spriteRenderer.flipX = false;
                break;
            case NoteData.LaneOrientation.TopLeft:
            case NoteData.LaneOrientation.BottomLeft:
                _spriteRenderer.flipX = true;
                break;
            case NoteData.LaneOrientation.Undefined:
            default:
                Debug.LogError("unassigned");
                break;
        }
    }
    private float PlayAnimationFromPlayList(List<string> playlist)
    {
        if (playlist == null) throw new ArgumentNullException(nameof(playlist));
        var anim = playlist[0];
        
        ChangeAnimationState(anim);
        
        playlist.RemoveAt(0);
        playlist.Add(anim);
        
        //return _animator.GetCurrentAnimatorClipInfo(0).Length;
        return GetAnimationLength(anim);
    }

    private void PlayGroundBlockAnimation()
    {
        ChangeAnimationState(_mainCharacterData.AttackGroundBlockIntro);
    }
    
    private void PlayAirBlockAnimation()
    {
        ChangeAnimationState(_mainCharacterData.AttackAirBlockIntro);
    }

    private void PlayIdleAnimation()
    {
        if (MainCharacterController.Instance.isBusy) return;
        ChangeAnimationState(_mainCharacterData.Idle);
    }

    private float PlayDashDownAnimation()//non hit
    {
        var anim = _mainCharacterData.Attack4;
        ChangeAnimationState(anim);
        return GetAnimationLength(anim);
    }

    private float GetAnimationLength(string clipName)
    {
        float time = 0;
        for (int i = 0; i < _runtimeAC.animationClips.Length; i++) //For all animations
        {
            if(_runtimeAC.animationClips[i].name == clipName)//If it has the same name as your clip
            {
                time = _runtimeAC.animationClips[i].length;
            }
        }

        return time;
    }
    
    private void ChangeAnimationState(string newAnimation)
    {
        if (_currentAnimation == newAnimation) return;

        _animator.Play(newAnimation);
        _currentAnimation = newAnimation;
    }
}
