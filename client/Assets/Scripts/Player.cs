using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed = 5f;
    public GameObject bulletPrefab;
    public Transform bulletSpawnPoint;
    public float bulletSpeed = 10f;
    public float bulletCooldown = 0.5f;

    private float lastBulletTime;

    private void Update()
    {
        MovePlayer();
        Shoot();
    }

    private void MovePlayer()
    {
        float moveInput = Input.GetAxis("Horizontal");
        transform.Translate(Vector3.right * moveInput * speed * Time.deltaTime);
    }

    private void Shoot()
    {
        if (Input.GetKeyDown(KeyCode.Space) && Time.time > lastBulletTime + bulletCooldown)
        {
            GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity);
            Rigidbody2D bulletRigidbody = bullet.GetComponent<Rigidbody2D>();
            bulletRigidbody.velocity = Vector2.up * bulletSpeed;
            lastBulletTime = Time.time;
        }
    }
}
