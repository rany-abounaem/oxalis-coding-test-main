using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardEntryUI : MonoBehaviour
{
    [SerializeField] private Text m_usernameText;
    [SerializeField] private Text m_scoreText;

    public void Init(string username, float score)
    {
        m_usernameText.text = username.ToString();
        m_scoreText.text = score.ToString("F2");
    }
}
