using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 6f;
    public int damage = 1;
    public float maxLifetime = 4f;

    private float age;

    private void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);
        age += Time.deltaTime;
        if (age >= maxLifetime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var zombie = other.GetComponent<Zombie>();
        if (zombie == null)
        {
            return;
        }

        zombie.TakeDamage(damage);
        Destroy(gameObject);
    }
}
