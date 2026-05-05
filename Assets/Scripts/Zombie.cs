using UnityEngine;

public class Zombie : MonoBehaviour
{
    public float speed = 0.9f;
    public int maxHealth = 3;
    public float despawnX = -8.5f;

    private int health;

    private void Awake()
    {
        health = maxHealth;
    }

    private void Update()
    {
        transform.Translate(Vector3.left * speed * Time.deltaTime);
        if (transform.position.x <= despawnX)
        {
            Destroy(gameObject);
        }
    }

    public void TakeDamage(int amount)
    {
        health -= amount;
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
