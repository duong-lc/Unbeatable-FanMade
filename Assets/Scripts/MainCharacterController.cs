using System;
using System.Collections;
using UnityEngine;
using EventTypes;
using Character;
using UnityEngine.Serialization;

[RequireComponent(typeof(NoteHitListener))]
[RequireComponent(typeof(Rigidbody))]

public class MainCharacterController : MonoBehaviour
{
    [SerializeField] private SO_MainCharacter_Data _mainCharacterData;
    [SerializeField] private SO_Midi_Data _midiData;

    private readonly NoteHit _emptyNote = new NoteHit();
    private GameObject _sliderObjectHolding;
    
    [Header("Gravity Variables")]
    private Rigidbody _rigidBody;

    [FormerlySerializedAs("_isGrounded")] [Header("Jumping Variables")]
    
    public bool isGrounded = true;
    private float _latestFreezeTimeStamp;
    private bool _canJump = true;
    private bool _canLerp = true;
    private const float GroundCheckMarginOfError = 0.05f;
    private bool _runCheckGravityOnce = true;
    private bool _canCheck = true;
    [SerializeField] private CharacterType _characterType = CharacterType.MainCharacter;
    
    public bool isBusy /*{ private set; get; }*/ = false;
    public static MainCharacterController Instance;

    public NoteData.LaneOrientation PlayerOrientation;
    //Cache Values from Scriptable Objects
    private float _gravityScale;
    private float _gravityScaleOriginal;
    private float _gConstant => _mainCharacterData.gravityAcceleration;
    private float _jumpForce => _mainCharacterData.jumpForce;
    private float _dampRatio => _mainCharacterData.dampRatio;
    private float _dampSecondsInterval => _mainCharacterData.dampSecondsInterval;
    private float _heightLimitMarginOfError => _mainCharacterData.heightLimitMarginOfError;
    private float _marginOverloadMultiplierEntry => _mainCharacterData.marginOverloadMultiplierEntry;
    private float _cooldownGap => _mainCharacterData.cooldownGap;
    private float _airFreezeMultiplier => _mainCharacterData.airFreezeMultiplier;
    private float _lerpTimeDash => _mainCharacterData.lerpTimeDash;
    

    private Vector3 _topRight => _mainCharacterData.topRight;
    private Vector3 _bottomRight => _mainCharacterData.bottomRight;
    private Vector3 _topLeft => _mainCharacterData.topLeft;
    private Vector3 _bottomLeft => _mainCharacterData.bottomLeft;

    private KeyCode _inputTopRight, _inputBottomRight, _inputTopLeft, _inputBottomLeft;

    private Vector3 _gravityVector;
    private Vector3 _velocity = Vector3.up * 5f;
    //private Vector3 _velocity = Vector3.zero;

    [Header("Events")] 
    [SerializeField] private AnimChangeEvent onAnimEvent;
    [SerializeField] private SideCharacterAppearanceEvent onSideAppearEvent;
    
    private void Awake()
    {
        Instance = this;
        _rigidBody = GetComponent<Rigidbody>();
        
        //caching variables from scriptable object
        _gravityScale = _mainCharacterData.gravityScale;

        _inputTopRight = _midiData.inputTopRight;
        _inputBottomRight = _midiData.inputBottomRight;
        _inputTopLeft = _midiData.inputTopLeft;
        _inputBottomLeft = _midiData.inputBottomLeft;
    }
    
    private void Start()
    {
        _rigidBody.constraints = RigidbodyConstraints.FreezeRotationX | 
                            RigidbodyConstraints.FreezeRotationY |
                            RigidbodyConstraints.FreezeRotationZ|
                            RigidbodyConstraints.FreezePositionZ;
        _gravityScaleOriginal = _gravityScale;
    }

    private void FixedUpdate()
    {
        //Check if over max y value, then correct the player
        if (_canCheck)
        {
            if (transform.position.y > _topRight.y)
            {
                transform.position = new Vector3(transform.position.x, _topRight.y, transform.position.z);
                StartCoroutine(CheckOverRoutine());
            }
        }
        
        //Ground Check when hitting ground
        if (_rigidBody.velocity.y < -1 &&
            transform.position.y < _bottomRight.y + GroundCheckMarginOfError)
        {
            if (!isGrounded)
            {
                PlayerOrientation = PlayerOrientation switch
                {
                    NoteData.LaneOrientation.TopLeft => NoteData.LaneOrientation.BottomLeft,
                    NoteData.LaneOrientation.TopRight => NoteData.LaneOrientation.BottomRight,
                    _ => PlayerOrientation
                };

                isGrounded = true;

                if (!isBusy)
                {
                    AnimChange anim = new AnimChange() {animationState = AnimState.Land};
                    onAnimEvent.Raise(anim);
                }
            }
        }
        //Damping velocity when hitting max height
        if (_rigidBody.velocity.y > 1 &&
            transform.position.y > _topRight.y - _heightLimitMarginOfError * _marginOverloadMultiplierEntry)
        {
            StartCoroutine(DampVelocityRoutine());
        }
    }

    private void Update()
    {
        //These are movements that doesn't hit any notes
        if(!isBusy)
        {
            if (Input.GetKeyDown(_inputTopRight))
            {
                StartCoroutine(LerpDashUpRoutine(NoteData.LaneOrientation.TopRight, _emptyNote, false, false));
            }
            if (Input.GetKeyDown(_inputTopLeft))
            {
                StartCoroutine(LerpDashUpRoutine(NoteData.LaneOrientation.TopLeft, _emptyNote, false, false));
            }
            if (Input.GetKeyDown(_inputBottomRight))
            {
                StartCoroutine(LerpDashDownRoutine(NoteData.LaneOrientation.BottomRight, _emptyNote, false));
            }
            if (Input.GetKeyDown(_inputBottomLeft))
            {
                StartCoroutine(LerpDashDownRoutine(NoteData.LaneOrientation.BottomLeft, _emptyNote, false));
            }
        }

        //Check to Simulate dashing in air and air temp stall
        //if (!isOccupied)
        if(!isBusy)
        {
            if (Time.time < _latestFreezeTimeStamp + 0.1f * _airFreezeMultiplier)
            {
                _gravityScale = 0;
                _runCheckGravityOnce = true;
            }
            else
            {
                if (_runCheckGravityOnce)
                {
                    //print($"release");
                    _gravityScale = _gravityScaleOriginal;
                    _runCheckGravityOnce = false;
                }
            }
        }
        
        
    }
    //For dashing down, apply direction vector. Calculate from position to destination. then apply dash force to that direction
    //if is grounded then just horizontal directional vector.
    
    private void LateUpdate()
    {
        CustomGravity();
    }
    
    private void CustomGravity()
    {
        _gravityVector = -_gConstant * _gravityScale * Vector3.up;
        _rigidBody.AddForce(_gravityVector, ForceMode.Acceleration);
    }
    
    private IEnumerator DampVelocityRoutine()
    {
        float dampRatioLocal = _dampRatio;
        while(true)
        {
            Vector3 velocity = _rigidBody.velocity;
            _rigidBody.velocity = new Vector3(velocity.x, velocity.y * dampRatioLocal, velocity.z);
            if (velocity.y <= 0.1f)
            {
                yield break;
            }
            yield return new WaitForSeconds(_dampSecondsInterval);
        }
    }
    
    //dashing using lerp while lerp dashing, cannot overwrite it and dash again. must wait til finish.
    //jump is still physics based. (vertical only)
    //dashing down is lerp calculate direction from current position to bottomPosition. Then dash in that direction down. Cannot jump
    //or dash else where when in the lerp.

    private Vector3 UpdatePlayerOrientation(NoteData.LaneOrientation newOrientation)
    {
        PlayerOrientation = newOrientation;
        switch (newOrientation)
        {
            case NoteData.LaneOrientation.TopRight:
                return _topRight;
            case NoteData.LaneOrientation.TopLeft:
                return _topLeft;
            case NoteData.LaneOrientation.BottomRight:
                return _bottomRight;
            case NoteData.LaneOrientation.BottomLeft:
                return _bottomLeft;
            case NoteData.LaneOrientation.Undefined:
                Debug.LogError("unassigned type");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newOrientation), newOrientation, null);
        }

        return Vector3.zero;
    }
    
    private IEnumerator LerpDashDownRoutine(NoteData.LaneOrientation noteOrientation, NoteHit note, bool isOverride)
    {
        if (isOverride) _canLerp = true;
        if (!_canLerp) yield break;

        AnimChange animData = note.noteID == NoteData.NoteID.SliderNote
             ? new AnimChange() {animationState = AnimState.GroundBlock}
             : isGrounded
                 ? new AnimChange() {animationState = AnimState.GroundAttack}
                 : new AnimChange() {animationState = AnimState.GroundDash};
        animData.orientation = noteOrientation;
        animData.type = _characterType;
        
        onAnimEvent.Raise(animData);
         
        Vector3 bottomPosition = UpdatePlayerOrientation(noteOrientation);
        
        float alpha = 0f;
        float rate = 1f / _lerpTimeDash;
        Vector3 startPosition = transform.position;
        
        _canLerp = false;
        while (alpha < 1f)
        {
            alpha += Time.deltaTime * rate;
            transform.position = Vector3.Lerp(startPosition, bottomPosition, alpha);
            yield return null;
        }
        
        //if object doesn't exist, assign and hold 
        AttemptToHold(note, bottomPosition);
        
        isGrounded = true;
        _canLerp = true;
        if (note.noteID != NoteData.NoteID.SliderNote)
        {
            //isBusy = false;
            SetBusy(false, note);
        }
    }

    private IEnumerator LerpDashUpRoutine(NoteData.LaneOrientation noteOrientation, NoteHit note, bool skipGroundCheck, bool isOverride)
    { 
        if (isOverride) _canLerp = true;
        if (!_canLerp) yield break;
        if (!isGrounded && !skipGroundCheck) yield break;
        
        
        AnimChange animData = note.noteID == NoteData.NoteID.SliderNote
            ? new AnimChange() {animationState = AnimState.AirBlock}
            : isOverride
                ? new AnimChange() {animationState = AnimState.AirAttack}
                : new AnimChange() {animationState = AnimState.Jump};
        animData.orientation = noteOrientation;
        animData.type = _characterType;
        
        onAnimEvent.Raise(animData);

        Vector3 topPosition = UpdatePlayerOrientation(noteOrientation);
        
        float alpha = 0f;
        //float alpha = Time.time;
        float rate = 1f / _lerpTimeDash;
        Vector3 startPosition = transform.position;
        topPosition = new Vector3(topPosition.x, topPosition.y - _heightLimitMarginOfError * 2f, topPosition.z);
        
        _canLerp = false;
        while (alpha < 1)
        {
            alpha += Time.deltaTime * rate;
            transform.position = Vector3.Lerp(startPosition, topPosition, alpha);
            yield return null;
        }
        
        //if object doesn't exist, assign and hold 
        AttemptToHold(note, topPosition);
        
        
        _canLerp = true;
        if (note.noteID != NoteData.NoteID.SliderNote)
        {
            //isBusy = false;
            SetBusy(false, note);
        }

        ApplyJumpPhysics();
    }

    private void AttemptToHold(NoteHit note, Vector3 position)
    {
        
        //if object doesn't exist, assign and hold 
        if (note.noteID == NoteData.NoteID.SliderNote && note.regState == RegisterState.KeyDown)
        {
            if (_sliderObjectHolding == null)
            {
                isBusy = true;
                _sliderObjectHolding = note.noteObject;
                //begin hold at that position
                _gravityScale = 0;

                transform.position = position;
            }
        }
    }
    
    //listener of NoteHitListener
    public void MainCharacter_NoteHitListener(NoteHit noteHit)
    {
        //Get the note hit data, temporary disable the input system in Update() using boolean
        //then inject appropriate dashing behavior read from noteHit data.
        //Once lerp-ing behavior is finished, then enable boolean again.
        if (noteHit.noteID == NoteData.NoteID.None) return;
        
        SideCharacterAppearance appearanceData = new SideCharacterAppearance()
        {
            noteHit = noteHit,
            priority = AppearancePriority.First
        };
        
        if (noteHit.hitStatus == NoteHitStatus.Success)//hit note successfully
        {
            //if (!isOccupied && noteHit.regState == RegisterState.KeyDown)
            if (!isBusy && noteHit.regState == RegisterState.KeyDown)
            {
                //isHit = true;
                isBusy = true;
                var noteOrientation = noteHit.orientation;
                switch (noteOrientation)
                {
                    case NoteData.LaneOrientation.TopRight:
                    case NoteData.LaneOrientation.TopLeft:
                        StartCoroutine(LerpDashUpRoutine(noteOrientation, noteHit, true, true));
                        break;
                    case NoteData.LaneOrientation.BottomRight:
                    case NoteData.LaneOrientation.BottomLeft:
                        StartCoroutine(LerpDashDownRoutine(noteOrientation, noteHit, true));
                        break;
                    case NoteData.LaneOrientation.Undefined:
                        Debug.LogError("unassigned type");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();

                }
            }
            else if(isBusy && noteHit.noteObject == _sliderObjectHolding &&
                    noteHit.regState == RegisterState.KeyUp)
            {
                //isOccupied = false;
                
                AnimChange animData = isGrounded
                        ? new AnimChange() {animationState = AnimState.GroundAttack}
                        : new AnimChange() {animationState = AnimState.AirAttack};
                animData.orientation = noteHit.orientation;
                animData.type = _characterType;
                
                onAnimEvent.Raise(animData);
                
                //isBusy = false;
                SetBusy(false, noteHit);
                _sliderObjectHolding = null;
                _runCheckGravityOnce = true;
            }
            else//player is busy and not hitting slider note on key up. Key hit success case
            {
                onSideAppearEvent.Raise(appearanceData);
            }
        }
        else//fail at hit note
        {
            if (noteHit.noteID == NoteData.NoteID.SliderNote && noteHit.regState == RegisterState.KeyUp)
            {
                onSideAppearEvent.Raise(appearanceData);
            }
            if (noteHit.orientation != PlayerOrientation) return;
            
            AnimChange animData = new AnimChange() 
                {
                    animationState = AnimState.Hurt, 
                    orientation = noteHit.orientation,
                    type = _characterType
                };
            onAnimEvent.Raise(animData);

            //isBusy = false;
            SetBusy(false, noteHit);
            _sliderObjectHolding = null;
            _runCheckGravityOnce = true;
        }
    }

    //Quickly lerp to the upper position then apply a light force there, do not add jump force from start!!!!
    //This produce tons of inconsistencies. 
    private void ApplyJumpPhysics()
    {
        Vector3 velocity = _rigidBody.velocity;
        //if (_isGrounded) return;
        if (_canJump)
        {
            //print($"jump");
            _rigidBody.velocity = new Vector3(velocity.x, 0f, velocity.z);
            _rigidBody.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
            isGrounded = false;
            _latestFreezeTimeStamp = Time.time;
            StartCoroutine(JumpCoolDownRoutine());
        }
        
    }

    private void SetBusy(bool status, NoteHit noteHit)
    {
        isBusy = status;
        
        // AnimChange animData = new AnimChange() 
        // {
        //     animationState = AnimState.Idle, 
        //     orientation = noteHit.orientation,
        //     type = _characterType
        // };
        // onAnimEvent.Raise(animData);
    }
    
    private IEnumerator JumpCoolDownRoutine()
    {
        _canJump = false;
        yield return new WaitForSeconds(_cooldownGap);
        _canJump = true;
    }

    private IEnumerator CheckOverRoutine()
    {
        _canCheck = false;
        yield return new WaitForSeconds(0.015f);
        _canCheck = true;
    }
    
}
