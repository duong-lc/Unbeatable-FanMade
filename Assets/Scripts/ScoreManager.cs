using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<Summary>
///This class handles Sound effects and Combos
///</Summary>
public class ScoreManager : MonoBehaviour
{
    #region Variables
    public static ScoreManager Instance;//Instance to access
    public AudioSource hitSFX;//hit sfx
    public AudioSource missSFX;//miss sfx
    public TMPro.TextMeshPro scoreText;//Score text
    static int comboScore;//combo score

    #endregion

    #region Unity Functions
    private void Start() {
        Instance = this;//self ref
        comboScore = 0;//set score at 0 at start
    }

    ///<summary>
    ///Incrementing combo score by one 
    ///Play hit sound fx
    ///</summary>
    public static void Hit()
    {
        comboScore += 1;
        Instance.hitSFX.Play();
    }

    ///<summary>
    ///Reset combo score to 0
    ///PLay miss sound fx
    ///</summary>
    public static void Miss()
    {
        comboScore = 0;
        Instance.missSFX.Play();
    }

    ///<summary>
    ///Update the score text
    ///</summary>
    private void Update()
    {
        scoreText.text = comboScore.ToString();
    }
    #endregion
}
