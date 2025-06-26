using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] int maxHealth = 3;
    public float speed = 5f;
    public Transform bulletSpawnPoint;

    private int m_currentHealth;
    private float lastBulletTime;
    private PlayerData m_playerData = new PlayerData();

    public int CurrentHealth => m_currentHealth;
    public WeaponProperties CurrentWeapon { get; set; }
    public PlayerData PlayerData => m_playerData;

    private void Start()
    {
        m_currentHealth = maxHealth;
    }

    private void Update()
    {
        if (GameController.Instance.LoggedIn)
        {
            MovePlayer();
            Shoot();
        }
    }

    private void MovePlayer()
    {
        float moveInput = Input.GetAxis("Horizontal");
        if (moveInput < 0 && transform.position.x <= CameraExtensions.OrthographicBounds(Camera.main).min.x + 0.5f || moveInput > 0 && transform.position.x >= CameraExtensions.OrthographicBounds(Camera.main).max.x - 0.5f)
        {
            return; // Prevent player from moving out of camera bounds
        }
        transform.Translate(Vector3.right * moveInput * speed * Time.deltaTime);
    }

    private void Shoot()
    {
        if (Input.GetKeyDown(KeyCode.Space) && Time.time > lastBulletTime + CurrentWeapon.BulletCooldown)
        {
            switch (CurrentWeapon.Type)
            {
                case WeaponType.Missile:
                    ShootBullet(Vector2.up);
                    break;
                case WeaponType.MultiMissile:

                    int n = 3;
                    float spread = 30f;

                    for (int i = 0; i < n; i++)
                    {
                        float angle = (n == 1) ? 0 : -spread / 2 + (spread / (n - 1)) * i;
                        Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.up;
                        ShootBullet(direction);
                    }
                    break;
                default:
                    Debug.LogWarning("Unknown weapon type!");
                    break;
            }
        }
    }

    private void ShootBullet(Vector2 direction)
    {
        // Object pooling could be implemented here for better performance with life time on pooled objects to avoid memory leaks
        GameObject bullet = Instantiate(CurrentWeapon.BulletPrefab, bulletSpawnPoint.position, Quaternion.identity);
        Rigidbody2D bulletRigidbody = bullet.GetComponent<Rigidbody2D>();
        bulletRigidbody.velocity = direction * CurrentWeapon.BulletSpeed;

        lastBulletTime = Time.time;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("InvaderBullet"))
        {
            // 2.2: Logic for player health/hearts that is reflected in the UI
            m_currentHealth--;

            Debug.Log($"Player hit! Current Health: {m_currentHealth}");

            if (m_currentHealth <= 0)
            {
                GameController.Instance.GameOver();
            }

            Destroy(collision.gameObject);
        }

        if (collision.CompareTag("Invader"))
        {
            GameController.Instance.GameOver();
        }
    }
}
