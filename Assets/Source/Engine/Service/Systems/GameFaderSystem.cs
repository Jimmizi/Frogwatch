using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class GameFaderSystem : SystemObjectWithVars<GameFaderVars>
{
    public enum ScreenFadeState
    {
        Visible,
        Hidden,
        Paused,

        Showing,
        Hiding,
        Pausing,
        Resuming,
    }

    public delegate void VoidCallback();

    #region Public Variables

    // Called just after the screen has been fully faded in
    public VoidCallback OnGameShown;

    // Called just after the screen has been fully faded out
    public VoidCallback OnGameHidden;

    // Called just after the screen has been paused
    public VoidCallback OnGamePaused;

    // Called just after the screen has been resumed from a pause
    public VoidCallback OnGameResumed;

    #endregion

    #region Public Functions

    public bool IsVisible => currentFadeState == ScreenFadeState.Visible;
    public bool IsPaused => currentFadeState == ScreenFadeState.Paused;
    public bool IsHidden => currentFadeState == ScreenFadeState.Hidden;
    public bool IsCurrentlyFading => currentFadeState >= ScreenFadeState.Showing;

    public bool StartHide()
    {
        return false;
    }

    #endregion

    #region Private Variables

    private ScreenFadeState currentFadeState;

    [SerializeField]
    private float fadeTime = 1.0f;

    [SerializeField]
    private float pauseFadeTime = 0.5f;

    

    private float currentTimer = 0.0f;

    #endregion
    
    public override void AwakeService()
    {
        Debug.Assert(GetVars().BlackScreenGroup);


    }

    public override void StartService()
    {
        
    }

    public override void UpdateService()
    {
        

        switch (currentFadeState)
        {
            case ScreenFadeState.Visible:
                break;
            case ScreenFadeState.Hidden:
                break;
            case ScreenFadeState.Paused:
                break;
            case ScreenFadeState.Showing:
                ProcessShowingGame();
                break;
            case ScreenFadeState.Hiding:
                ProcessHidingGame();
                break;
            case ScreenFadeState.Pausing:
                ProcessPausingGame();
                break;
            case ScreenFadeState.Resuming:
                ProcessPausingGame();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public override void FixedUpdateService()
    {
        
    }

    private void ProcessShowingGame()
    {

    }

    private void ProcessHidingGame()
    {

    }

    private void ProcessPausingGame()
    {

    }

    private void ProcessResumingGame()
    {

    }

    
}
