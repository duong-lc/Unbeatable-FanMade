using System.Collections.Generic;
using UnityEngine;
using System;
using Character;
using EventTypes;

public class SideCharacterAnimatorController : MonoBehaviour
{
    [SerializeField] private SO_SideCharacter_Data _sideCharacterData;
    [SerializeField] private CharacterType _characterType = CharacterType.SideCharacter1;

    private SpriteRenderer _spriteRenderer;
    private Animator _animator;
    private RuntimeAnimatorController _runtimeAC;


    public string _currentAnimation { get; private set; }

    private List<string> _listAttackAnim;
    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
        _runtimeAC = _animator.runtimeAnimatorController;
        
        _listAttackAnim = new List<string>()
        {
            _sideCharacterData.Attack01,
            _sideCharacterData.Attack02
        };
    }

    public void AnimationChangeEvent_Listener(AnimChange data)
    {
        if (data.type != _characterType) return;
        
        FlipSprite(data.orientation);
        switch (data.animationState)
        {
            case AnimState.GroundAttack:
            case AnimState.AirAttack:
                PlayAnimationFromPlayList(_listAttackAnim);
                break;
            case AnimState.AirBlock:
            case AnimState.GroundBlock:
                PlayBlockAnimation();
                break;
            default:
                throw new ArgumentOutOfRangeException();
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
    
    public float GetAnimationLength(string clipName)
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
    
    private void PlayBlockAnimation()
    {
        ChangeAnimationState(_sideCharacterData.BlockLoop);
    }
}
