using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StatsScreen : MonoBehaviour
{
    public TextMeshProUGUI numFrogs;
    public TextMeshProUGUI numFrogsThrown;
    public TextMeshProUGUI numFrogsTaken;
    public TextMeshProUGUI numWitchesBonked;
    public TextMeshProUGUI distanceTravelled;

    public AudioClip statsMusicClip;

    public bool visible
    {
        get { return transform.GetChild(0).gameObject.activeSelf; }
        set { transform.GetChild(0).gameObject.SetActive(value); }
    }

    void Start()
    {
        visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameTimer.TimeLeft <= 0)
        {
            if (!visible)
            {
                UpdateStats();
                Service.Get<AudioSystem>().CrossFadeToMusic(statsMusicClip, 0.5f);
                visible = true;
            }
            else
            {
                if (Input.GetButtonUp("Dash"))
                {
                    RestartGame();
                }
            }
        }
    }

    void UpdateStats()
    {
        numFrogs.text = $"{FrogController.GetNumberOfFrogs()}";
        numFrogsThrown.text = $"{GameStats.Stats.FrogsThrownIntoPonds}";
        numFrogsTaken.text = $"{FrogController.NumFrogsTaken}";
        numWitchesBonked.text = $"{GameStats.Stats.WitchesBonked}";
        distanceTravelled.text = $"{Mathf.RoundToInt(GameStats.Stats.DistanceMoved)}m";
    }

    public void RestartGame()
    {
        Service.Get<AudioSystem>().PlayEvent(AudioEvent.UIConfirm, Camera.main.transform.position);

        GameStats.Reset();
        FrogController.NumFrogsTaken = 0;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
