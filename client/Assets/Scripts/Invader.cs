using UnityEngine;

public class Invader : MonoBehaviour
{
    public float speed = 1f;
    public float moveDownDistance = 1f;

    private bool moveRight = true;
    private static float lowestPosition = Mathf.Infinity;

    private void Start()
    {
        InvokeRepeating("Move", 1f, 1f);
    }

    private void Move()
    {
        Vector3 movement = moveRight ? Vector3.right : Vector3.left;
        transform.Translate(movement * speed);

        if (transform.position.y < lowestPosition)
        {
            lowestPosition = transform.position.y;
        }

        CheckScreenEdgeCollision();
    }

    private void CheckScreenEdgeCollision()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, moveDownDistance, LayerMask.GetMask("Default"));

        if (hit.collider != null && hit.collider.CompareTag("ScreenEdge"))
        {
            GameController.Instance.InvaderHitEdge();
        }
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
            Destroy(gameObject);
            Destroy(collision.gameObject);
        }
    }
}