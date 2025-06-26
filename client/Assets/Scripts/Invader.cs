using UnityEngine;

public class Invader : MonoBehaviour
{
    [SerializeField] private GameObject m_bulletPrefab;
    [SerializeField] private SpriteRenderer m_spriteRenderer;
    [SerializeField] private float m_bulletSpeed;

    public float speed = 1f;
    public float moveDownDistance = 1f;

    private bool moveRight = true;
    private int health;
    private InvaderTypeProperties m_invaderType;

    public InvaderTypeProperties InvaderType => m_invaderType;

    public void Init(InvaderTypeProperties invaderType)
    {
        m_invaderType = invaderType;
        health = invaderType.Health;
        m_spriteRenderer.sprite = invaderType.Sprite;
    }

    public void ChangeDirection(bool newDirection)
    {
        moveRight = newDirection;
        transform.Translate(Vector3.down * moveDownDistance);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Bullet"))
        {
            health--;
            if (health <= 0)
            {
                GameController.Instance.OnInvaderDeath(this);
            }
            Destroy(collision.gameObject);
        }
    }

    public bool IsClearBulletPath()
    {
        var hits = Physics2D.RaycastAll(transform.position, Vector2.down, 100f);
        foreach (var hit in hits)
        {
            if (hit.collider != null && hit.collider.CompareTag("Invader") && hit.collider.gameObject != gameObject)
            {
                return false; // Bullet path is blocked by another invader
            }
        }
        return true;
    }


    // 2.2: Logic for invader shooting
    public void Shoot()
    {
        GameObject bullet = Instantiate(m_bulletPrefab, transform.position, Quaternion.identity);
        Rigidbody2D bulletRigidbody = bullet.GetComponent<Rigidbody2D>();
        bulletRigidbody.velocity = Vector2.down * m_bulletSpeed;
    }
}