using UnityEngine;
using UnityEngine.UI;

public struct RoundTime
{
    public float timeLeft;
}

public class TimerUI : MonoBehaviour
{
    [SerializeField] private Text m_timerText;
    private GameController m_gameController;
    private float m_requestCooldown = 0.5f;

    private void Start()
    {
        m_gameController = GameController.Instance;
    }

	/// <summary>
    /// This method makes use of HTTP polling to retrieve the round time from the server.
    /// Could be replaced with a WebSocket subscription for real-time updates.
    /// </summary>
    private void Update()
    {
        m_requestCooldown -= Time.deltaTime;
        if (m_requestCooldown <= 0f)
        {
            StartCoroutine(m_gameController.RetrieveRoundTime(UpdateTimerText));
            m_requestCooldown = 0.5f;
        }

    }

    private void UpdateTimerText(string timeLeft)
    {
        if (string.IsNullOrEmpty(timeLeft))
        {
            m_timerText.text = "Time Left: N/A";
            return;
        }

        if (JsonUtility.FromJson<RoundTime>(timeLeft) is RoundTime roundTime)
        {
            float time = roundTime.timeLeft;

            if (time < 0f)
            {
                time = 0f;
                GameController.Instance.GameOver();
            }
            
            m_timerText.text = $"Time Left: {time:F2} seconds";
        }
        else
        {
            m_timerText.text = "Time Left: Invalid Data";
        }
    }
}
