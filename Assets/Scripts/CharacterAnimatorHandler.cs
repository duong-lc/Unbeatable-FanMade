using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public static class BeatCombatAir
{
    //Air Attack Animation Names
    public const string attack1 = "BeatCombatAirAttack01";
    public const string attack2 = "BeatCombatAirAttack02";
    public const string attack3 = "BeatCombatAirAttack03";
    public const string attackBlockIntro = "BeatCombatAirBlockIntro";
    public const string attackBlockLoop = "BeatCombatAirBlockLoop";
}
public static class BeatCombatGround{
    //Ground Attack Animation Names
    public const string attack1 = "BeatCombatGroundAttack01";
    public const string attack2 = "BeatCombatGroundAttack02";
    public const string attack3 = "BeatCombatGroundAttack03";
    public const string attack4 = "BeatCombatGroundAttack04";
    public const string attack5 = "BeatCombatGroundAttack05";
    public const string attack6 = "BeatCombatGroundAttack06";
    public const string attack7 = "BeatCombatGroundAttack07";
}
public static class BeatCombatHurt{
    //Combat Hurt Animation Names
    public const string hurt1 = "BeatCombatHurt01";
    public const string hurt2 = "BeatCombatHurt02";
    public const string hurt3 = "BeatCombatHurt03";
}
public static class BeatCombatJump{
    //Combat jump Animation Names
    public const string jump1 = "BeatCombatJump01";
    public const string jump2 = "BeatCombatJump02";
    public const string jumpLand = "BeatCombatJumpLand";
}
public static class BeatCombatIdle{
    //Other Animation Names
    public const string idle = "BeatCombatIdle";
    public const string intro = "BeatCombatIntro";
}

public class CharacterAnimatorHandler : MonoBehaviour
{  
    #region Variables
    private Animator animator;
    public string currentAnimation {get; private set;}
    #endregion
    
    private void Start()
    {
        animator = GetComponent<Animator>();
    }
    public void PlayJumpAnimation()
    {
        int num = Random.Range(0,2);
        switch(num)
        {
            case 0:
                ChangeAnimationState(BeatCombatJump.jump1);
                break;
            case 1:
                ChangeAnimationState(BeatCombatJump.jump2);
                break;
        }
    }
    public void PlayLandAnimation()
    {
        ChangeAnimationState(BeatCombatJump.jumpLand);
    }
    public void PlayHurtAnimation()
    {
        int num = Random.Range(0,3);
        switch(num)
        {
            case 0:
                ChangeAnimationState(BeatCombatHurt.hurt1);
                break;
            case 1:
                ChangeAnimationState(BeatCombatHurt.hurt2);
                break;
            case 2:
                ChangeAnimationState(BeatCombatHurt.hurt3);
                break;
        }
    }
    public void PlayGroundAttackAnimation()
    {
        int num = Random.Range(0, 7);
        //print($"{num}");
        switch(num)
        {
            case 0:
                ChangeAnimationState(BeatCombatGround.attack1);
                break;
            case 1:
                ChangeAnimationState(BeatCombatGround.attack2);
                break;
            case 2:
                ChangeAnimationState(BeatCombatGround.attack3);
                break;
            case 3:
                ChangeAnimationState(BeatCombatGround.attack4);
                break;
            case 4:
                ChangeAnimationState(BeatCombatGround.attack5);
                break;
            case 5:
                ChangeAnimationState(BeatCombatGround.attack6);
                break;
            case 6:
                ChangeAnimationState(BeatCombatGround.attack7);
                break;
        }
    }
    public void PlayAirAttackAnimation()
    {
        int num = Random.Range(0,3);
        //print($"{num}");
        switch(num)
        {
            case 0:
                ChangeAnimationState(BeatCombatAir.attack1);
                break;
            case 1:
                ChangeAnimationState(BeatCombatAir.attack2);
                break;
            case 2:
                ChangeAnimationState(BeatCombatAir.attack3);
                break;
        }
    }
    public void PlayAttackBlockIntroAnimation()
    {
        ChangeAnimationState(BeatCombatAir.attackBlockIntro);
    }
    public void PlayAttackBlockLoopAnimation()
    {
        ChangeAnimationState(BeatCombatAir.attackBlockLoop);
    }
    public void PlayIdleAnimation()
    {
        ChangeAnimationState(BeatCombatIdle.idle);
    }
    public void PlayIntroAnimation()
    {
        ChangeAnimationState(BeatCombatIdle.intro);
    }
    public float GetCurrentAnimationLength()
    {
        return animator.GetCurrentAnimatorStateInfo(0).length;
    }
    
    void ChangeAnimationState(string newAnimation)
    {
        if (currentAnimation == newAnimation) return;

        animator.Play(newAnimation);
        currentAnimation = newAnimation;
    }
    
}
