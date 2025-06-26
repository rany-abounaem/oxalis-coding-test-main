using UnityEngine;


// 2.3 & 2.4: Different invader types with different health and points rewarded
public enum InvaderType
{
    Amateur,
    Skillful,
    Master
}

[CreateAssetMenu(fileName = "InvaderProperties", menuName = "Resources/InvaderProperties", order = 0)]
public class InvaderTypeProperties : ScriptableObject
{
    [SerializeField] private InvaderType m_type;
    [SerializeField] private int m_points;
    [SerializeField] private int m_health;
    [SerializeField] private Sprite m_image;

    public InvaderType Type => m_type;
    public int Points => m_points;
    public int Health => m_health;
    public Sprite Sprite => m_image;
}