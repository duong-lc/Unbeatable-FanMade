using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<Summary>
///Handles Controlling the character
///</Summary>
public class CharacterHandler : MonoBehaviour
{
    #region Variables
    public enum currentLaneOccupied
    {
        upperLane, lowerLane, none
    }
    
    [SerializeField] private Lane lowerLane, upperLane;
    public Vector3 lowerLeft, lowerRight, upperLeft, upperRight;

    private KeyCode lowerLaneInput, upperLaneInput;
    private Lane.LaneSide curUpperSide, currLowerSide;
    private Rigidbody rb;
    public bool isGrounded; 
    public CharacterAnimatorHandler animatorHandler {get; private set;}
    [SerializeField] private float groundCheckMarginOfError;
    public Vector3 currPos;
    private SpriteRenderer spriteRenderer;
    private double keyUpTime, keyDownTime;


    [SerializeField] private float lerpTimeNoteJump;
    public currentLaneOccupied beatCurrentLane;
    //public TMPro.TextMeshPro laneText;

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        beatCurrentLane = currentLaneOccupied.none;
        spriteRenderer = GetComponent<SpriteRenderer>();
        animatorHandler = GetComponent<CharacterAnimatorHandler>();
        rb = GetComponent<Rigidbody>();
        lowerLaneInput = lowerLane.input;
        upperLaneInput = upperLane.input;
        currPos = lowerRight;
        isGrounded = true;
    }

    // Update is called once per frame
    void Update()
    {
        currLowerSide = lowerLane.playerCurrentSide;
        curUpperSide = upperLane.playerCurrentSide;

        //damping fall speed
        if(transform.position.y >= upperRight.y - 0.3f)
        {
            rb.mass = 0.5f;
        }else
            rb.mass = 1f;

        //ground check
        if(rb.velocity.y < -1 && transform.position.y <= lowerRight.y + groundCheckMarginOfError){
            currPos = CurrSideBothLanesCheck().lowerSidePos; 
               
            //print($"{currPos}");     
            if(!isGrounded)
            {
                isGrounded = true;
                PlayLand();
            }
        }
        InputHandler();    
    }

    private void InputHandler()
    {
        if(Input.GetKeyDown(lowerLaneInput))
        {
            keyDownTime = SongManager.GetAudioSourceTime();
            //if two keys are not in conflict
            if(beatCurrentLane != currentLaneOccupied.upperLane)
            {
                isGrounded = true;
                PlayDashDown();
                PlayGroundAttack();
            }
        }
        if(Input.GetKeyDown(upperLaneInput) && isGrounded)
        {
            keyUpTime = SongManager.GetAudioSourceTime();
            //If the two keys are not in conflict
            if(beatCurrentLane != currentLaneOccupied.lowerLane)
                isGrounded = false;
        }
    }
    public void PlayBlockIntro()
    {
        animatorHandler.PlayAttackBlockIntroAnimation();
    }
    public void PlayBlockLoop()
    {
        animatorHandler.PlayAttackBlockLoopAnimation();
    }
    public void PlayGroundAttack()
    {
        animatorHandler.PlayGroundAttackAnimation();
    }
    public void PlayAirAttack()
    {
        animatorHandler.PlayAirAttackAnimation();
    }
    public void PlayJump()
    {   
        animatorHandler.PlayJumpAnimation();
    }
    
    public void PlayDashUpperToLower_TaikoPattern()
    {
        //var path = CurrSideBothLanesCheck();
        //StartCoroutine(LerpFromTo(path.lowerSidePos, path.upperSidePos, false));
    }
    public void PlayDashLowerToUpper_TaikoPattern()
    {
        //var path = CurrSideBothLanesCheck();
        //StartCoroutine(LerpFromTo(path.upperSidePos, path.lowerSidePos, false));
    }
    public (Vector3 upperSidePos, Vector3 lowerSidePos) CurrSideBothLanesCheck()
    {
        Vector3 lowerSidePos;
        Vector3 upperSidePos;
        //Lower Side Check
        switch(currLowerSide){
            case Lane.LaneSide.rightSide:
                lowerSidePos = lowerRight;
                break;
            default:
                lowerSidePos = lowerLeft;
                break;
        }
        //Upper Side Check
        switch(curUpperSide){
            case Lane.LaneSide.rightSide:
                upperSidePos = upperRight;
                break;
            default:
                upperSidePos = upperLeft;
                break;
        }
        return (upperSidePos,lowerSidePos);
    }
    private void FlipSprite(Vector3 end)
    {
        float x = end.x;//transform.position.x;
        if(x <= 0.55f && x > 0)//rught
                spriteRenderer.flipX = false;
        else
            spriteRenderer.flipX = true;
    }
    public void PlayDashDown()
    {
        currPos = CurrSideBothLanesCheck().lowerSidePos;
        StartCoroutine(LerpFromTo(currPos, transform.position, false));
    }
    public void PlayJump(string laneTag, bool isStalling)
    {
        if(laneTag == "UpperLane" && beatCurrentLane != currentLaneOccupied.lowerLane)
        {
            rb.velocity = new Vector3 (rb.velocity.x, 0, rb.velocity.z);
            currPos = CurrSideBothLanesCheck().upperSidePos;
            StartCoroutine(LerpFromTo(currPos, transform.position, isStalling));
            
            if(!isStalling)
                PlayAirAttack();
            else
                PlayBlockIntro();    
        }
    }
    public void PlayIdle()
    {
        StartCoroutine(IdleCheckingCycle());
    }

    // public void ForceNoneCurrentLane()
    // {
    //     //beatCurrentLane = CharacterHandler.currentLaneOccupied.none;
    // }

    public IEnumerator IdleCheckingCycle()
    {
        while (true)
        {
            if(isGrounded && beatCurrentLane == currentLaneOccupied.none){
                animatorHandler.PlayIdleAnimation();
                break;
            }
            yield return null;
        }
    }
    public void PlayLand()
    {
        animatorHandler.PlayLandAnimation();
    }

    public void AddStalling()
    {
        //print($"stalling");
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }
    public void RemoveStalling()
    {
        //print($"gravity");
        rb.constraints = RigidbodyConstraints.None;
        rb.constraints = RigidbodyConstraints.FreezePositionZ |RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
 
    }

    ///<Summary>
    ///Checkpoint = a position out of 4 position in the rhythm level the player need to tap (top left, top right, bottom left, bottom right)
    ///checkpointStart = last position out of 4 check point positions the player was left off
    ///checkPointEnd = to be position out of 4 check point positions
    ///start = current starting position to lerp
    ///</Summary> 
    private IEnumerator LerpFromTo(Vector3 checkPointEnd, Vector3 start, bool isStalling)
    {
        
        float alpha = 0.0f;
        float rate = 1.0f/lerpTimeNoteJump;
        FlipSprite(checkPointEnd);

        while (alpha < 1.0f)    
        { 
            alpha += Time.deltaTime * rate;
            if(alpha > 1.0f && isStalling)
            {
                AddStalling();
            }
            transform.position = Vector3.Lerp(start, checkPointEnd, alpha);
            yield return null;
        }
    }

}
