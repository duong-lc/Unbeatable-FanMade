using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterQuaverAnimatorHandler : MonoBehaviour
{
    private Animator animator;
    private string currentAnimation;
    private string attack1 = "QuaverCombatAttack01";
    private string attack2 = "QuaverCombatAttack02";
    private string attackJump1 = "QuaverCombatAttackJump01";
    private string attackJump2 = "QuaverCombatAttackJump02";
    private string blockLoop = "QuaverCombatBlockLoop";
    public int num = 1;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void PlayAttackAnimation(bool isJump)
    {
        //int num = Random.Range(0, 2);
        //num++;
        switch (isJump)
        {
            case true:
                if(num % 2 == 0)
                {
                    ChangeAnimationState(attack1);
                    //animator.SetTrigger("attack1");
                }else{
                    ChangeAnimationState(attack2);
                    //animator.SetTrigger("attack2");
                } 
                break;
            case false:
                if(num % 2 == 0)
                {
                    ChangeAnimationState(attack1);
                    //animator.SetTrigger("attack1");
                }else{
                    ChangeAnimationState(attack2);
                    //animator.SetTrigger("attack2");
                } 
                break;
        }
       
    }
    public void PlayBlockLoop()
    {
        ChangeAnimationState(blockLoop);
    }

    void ChangeAnimationState(string newAnimation)
    {
        if (currentAnimation == newAnimation) return;

        animator.Play(newAnimation);
        currentAnimation = newAnimation;
    }
}
