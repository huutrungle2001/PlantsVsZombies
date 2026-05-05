using UnityEngine;

public class ZombieSpawner : MonoBehaviour
{
    public LaneGrid laneGrid;
    public float spawnInterval = 2.5f;
    public float minSpawnInterval = 1.2f;
    public float speed = 0.9f;
    public int health = 3;
    public Color zombieColor = new Color(0.55f, 0.75f, 0.55f);

    private float timer;

    private void Update()
    {
        if (laneGrid == null)
        {
            return;
        }

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnZombie();
            spawnInterval = Mathf.Max(minSpawnInterval, spawnInterval * 0.98f);
        }
    }

    private void SpawnZombie()
    {
        var laneIndex = Random.Range(0, laneGrid.laneCount);

        var zombie = new GameObject("Zombie");
        zombie.transform.position = new Vector3(laneGrid.zombieSpawnX, laneGrid.GetLaneY(laneIndex), 0f);
        zombie.transform.localScale = new Vector3(0.9f, 1.1f, 1f);

        var renderer = zombie.AddComponent<SpriteRenderer>();
        var controller = ArtLibrary.GetRandomZombieController();
        if (controller != null)
        {
            var animator = zombie.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
        }
        else
        {
            renderer.sprite = SimpleSprite.Create(zombieColor);
        }

        zombie.AddComponent<BoxCollider2D>();

        var zombieComponent = zombie.AddComponent<Zombie>();
        zombieComponent.speed = speed;
        zombieComponent.maxHealth = health;
        zombieComponent.despawnX = laneGrid.despawnX;
    }
}
