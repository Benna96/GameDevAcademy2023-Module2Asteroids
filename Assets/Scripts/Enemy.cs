using System;
using System.Collections.Generic;

using UnityEngine;

public class Enemy : WrapBehaviour
{
    [field: SerializeField] private int scoreChangeOnHit { get; set; }

    [field: SerializeField] private GameObject splitIntoObject { get; set; }
    [field: SerializeField] private int splitIntoNum { get; set; }

    protected override void AddToLevelManager()
    {
        if (!isGhost && LevelManager.instance != null)
            LevelManager.instance.enemies.Add(GetInstanceID(), this);
    }
    protected override void RemoveFromLevelManager()
    {
        if (!isGhost && LevelManager.instance != null)
            LevelManager.instance.enemies.Remove(GetInstanceID());
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Bullet"))
            return;

        if (GameManager.instance != null)
            GameManager.instance.UpdateScore(scoreChangeOnHit);

        Circle2D spawningCircle = new(gameObject.GetComponent<CircleCollider2D>());
        Destroy(collision.gameObject);
        if (LevelManager.instance == null)
        {
            Destroy(gameObject);
            return;
        }

        LevelManager.instance.Despawn(gameObject);
        if (splitIntoObject != null)
        {
            List<Circle2D> spawnedEnemies = new();
            CircleCollider2D circleCollider = splitIntoObject.GetComponent<CircleCollider2D>();
            for (int i = 0; i < splitIntoNum; i++)
                LevelManager.instance.SpawnWithin(circleCollider, spawningCircle, Array.Empty<Circle2D>(), spawnedEnemies, 0);
        }
    }

    protected override void OnDestroy()
    {
        if (!isGhost && LevelManager.instance != null)
            LevelManager.instance.spawnedList.Remove(gameObject.GetInstanceID());

        base.OnDestroy();
    }
}
