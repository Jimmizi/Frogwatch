using UnityEngine;
using TMPro;
public class GameTimer : MonoBehaviour
{
    private float timerDuration = 5f * 60f;

    private float nextThreshold;

    //private float timer;
    [SerializeField] private TextMeshProUGUI firstMinute;
    [SerializeField] private TextMeshProUGUI secondMinute;
    [SerializeField] private TextMeshProUGUI seperator;
    [SerializeField] private TextMeshProUGUI firstSecond;
    [SerializeField] private TextMeshProUGUI secondSecond;

    public static float TimeLeft => timeLeft;
    private static float timeLeft;

    public delegate void TimeCrossedMinuteThresholdDel();

    public static TimeCrossedMinuteThresholdDel OnMinuteCrossed;

    void Awake()
    {
        OnMinuteCrossed = null;
    }

    void Start()
    {
        ResetTimer();
    }

    void Update()
    {
        if (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            UpdateTimerDisplay(timeLeft);

            if (timeLeft <= nextThreshold)
            {
                nextThreshold -= 60.0f;
                OnMinuteCrossed?.Invoke();
            }
        }
    }
    private void ResetTimer()
    {
        timeLeft = timerDuration;
        nextThreshold = timerDuration - 60.0f;
    }
    private void UpdateTimerDisplay(float time)
    {
        float minutes = Mathf.FloorToInt(time / 60);
        float seconds = Mathf.FloorToInt(time % 60);

        string currentTime = string.Format("{00:00}{1:00}", minutes, seconds);
        firstMinute.text = currentTime[0].ToString();
        secondMinute.text = currentTime[1].ToString();
        firstSecond.text = currentTime[2].ToString();
        secondSecond.text = currentTime[3].ToString();
    }
}
