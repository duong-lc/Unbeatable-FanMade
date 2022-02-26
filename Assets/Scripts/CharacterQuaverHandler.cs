using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterQuaverHandler : MonoBehaviour
{
    public enum CombatState
    {
        attack, block, none
    }
    [SerializeField] private Lane lowerLane, upperLane;
    public Vector3 lowerLeft, lowerRight, upperLeft, upperRight;
    private KeyCode lowerLaneInput, upperLaneInput;
    private Lane.LaneSide curUpperSide, currLowerSide;
    [SerializeField] private float duration = 0.1f;
    private CharacterQuaverAnimatorHandler animatorQuaverHandler;
    public CharacterHandler.currentLaneOccupied quaverCurrentLane;
    private SpriteRenderer spriteRenderer;
    // Start is called before the first frame update
    void Start()
    {
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = true;

        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);
        quaverCurrentLane = CharacterHandler.currentLaneOccupied.none;
        //DisableQuaver();
        animatorQuaverHandler = GetComponent<CharacterQuaverAnimatorHandler>();
        lowerLaneInput = lowerLane.input;
        upperLaneInput = upperLane.input;
    }

    // Update is called once per frame
    void Update()
    {
        
        
    }

    public void EnableQuaverAtLane(string laneTag, CombatState state)
    {
        CycleAttackAnim();

        switch(laneTag)
        {
            case "UpperLane":
                curUpperSide = upperLane.playerCurrentSide;
                quaverCurrentLane = CharacterHandler.currentLaneOccupied.upperLane;
                switch(curUpperSide)
                {
                    case Lane.LaneSide.leftSide:
                        gameObject.transform.position = upperLeft;
                        spriteRenderer.flipX = true;
                        break;
                    case Lane.LaneSide.rightSide:
                        gameObject.transform.position = upperRight;
                        spriteRenderer.flipX = false;
                        break;
                }
                break;
            case "LowerLane":
                currLowerSide = lowerLane.playerCurrentSide;
                quaverCurrentLane = CharacterHandler.currentLaneOccupied.lowerLane;
                switch(currLowerSide)
                {
                    case Lane.LaneSide.leftSide:
                        gameObject.transform.position = lowerLeft;
                        spriteRenderer.flipX = true;
                        break;
                    case Lane.LaneSide.rightSide:
                        gameObject.transform.position = lowerRight;
                        spriteRenderer.flipX = false;
                        break;
                }
                break;
        }

        gameObject.GetComponent<Animator>().speed = 1f;
        StopCoroutine("LerpSpriteAlpha");
        StartCoroutine(LerpSpriteAlpha(1f, state, laneTag));

        
    }

    // public void EndAnimReached()
    // {
    //     print($"xd");
    // }
    public void DisableQuaver()
    {
        quaverCurrentLane = CharacterHandler.currentLaneOccupied.none;
        if(spriteRenderer.color.a != 0f)
            StartCoroutine(LerpSpriteAlpha(0f, CombatState.none, "none"));   
       // gameObject.SetActive(false);
       gameObject.GetComponent<Animator>().speed = 0f;
    }


    private IEnumerator LerpSpriteAlpha(float endValue, CombatState state, string laneTag)
    {
        float elapsedTime = 0f;
        float startValue = spriteRenderer.color.a;

        while (elapsedTime < duration)    
        {   
            elapsedTime += Time.deltaTime;
            if(elapsedTime/duration >= 1f)
            {
                switch(state)
                {
                    case CombatState.attack:
                        switch(laneTag)
                        {
                            case "UpperLane":
                                PlayAttack();
                                break;
                            case "LowerLane":
                                PlayAttack();
                                break;
                        }
                        break;
                    case CombatState.block:
                        PlayBlockLoop();
                        break;
                }
            }
            
            

            float newAlpha = Mathf.Lerp(startValue, endValue, elapsedTime/duration);
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, newAlpha);
            yield return null;
        }
       
        //print($"konichiwa");
    }

    public void CycleAttackAnim()
    {
        animatorQuaverHandler.num++;
    }

    public void PlayAttack()
    {
        animatorQuaverHandler.PlayAttackAnimation(false);
    }
    public void PlayBlockLoop()
    {
        animatorQuaverHandler.PlayBlockLoop();
    }
}
