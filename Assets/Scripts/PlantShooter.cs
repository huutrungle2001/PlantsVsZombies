using UnityEngine;

public class PlantShooter : MonoBehaviour
{
    public float fireInterval = 1.2f;
    public float projectileSpeed = 6f;
    public int projectileDamage = 1;
    public float projectileScale = 0.25f;
    public float projectileOffsetX = 0.6f;
    public Color projectileColor = new Color(1f, 0.9f, 0.2f);

    private float timer;

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= fireInterval)
        {
            timer = 0f;
            Fire();
        }
    }

    private void Fire()
    {
        var projectile = new GameObject("Projectile");
        projectile.transform.position = transform.position + new Vector3(projectileOffsetX, 0f, 0f);
        projectile.transform.localScale = new Vector3(projectileScale, projectileScale, 1f);

        var renderer = projectile.AddComponent<SpriteRenderer>();
        renderer.sprite = SimpleSprite.Create(projectileColor);

        var collider = projectile.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;

        var body = projectile.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;

        var projectileComponent = projectile.AddComponent<Projectile>();
        projectileComponent.speed = projectileSpeed;
        projectileComponent.damage = projectileDamage;
    }
}
