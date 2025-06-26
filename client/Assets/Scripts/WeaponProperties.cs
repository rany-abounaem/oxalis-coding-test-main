using UnityEngine;


// 2.5: Weapons with different types and properties
public enum WeaponType
{
    Missile,
    MultiMissile,
}

[CreateAssetMenu(fileName = "WeaponProperties", menuName = "Resources/WeaponProperties", order = 1)]
public class WeaponProperties : ScriptableObject
{
    [SerializeField] private WeaponType m_type;
    [SerializeField] private int m_pointsToUnlock;
    [SerializeField] private int m_grade;
    [SerializeField] private GameObject m_bulletPrefab;
    [SerializeField] private float m_bulletSpeed;
    [SerializeField] private float m_bulletCooldown;

    public WeaponType Type => m_type;
    public int PointsToUnlock => m_pointsToUnlock;
    public int Grade => m_grade;
    public GameObject BulletPrefab => m_bulletPrefab;
    public float BulletSpeed => m_bulletSpeed;
    public float BulletCooldown => m_bulletCooldown;
}