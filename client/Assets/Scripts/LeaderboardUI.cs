using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardUI : MonoBehaviour
{
    [SerializeField] private RectTransform m_content;
    [SerializeField] private LeaderboardEntryUI m_entryPrefab;
    [SerializeField] private Button m_exitButton;

    private void Start()
    {
        m_exitButton.onClick.AddListener(() => 
        {
            foreach (Transform child in m_content)
            {
                Destroy(child.gameObject);
            }
            gameObject.SetActive(false);
        });
    }

    public void AddEntry(string username, float score)
    {
        var entry = Instantiate(m_entryPrefab, m_content);
        entry.Init(username, score);
    }
}
