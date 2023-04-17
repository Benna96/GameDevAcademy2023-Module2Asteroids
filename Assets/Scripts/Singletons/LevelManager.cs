using System;
using System.Collections.Generic;
using System.Linq;

using DrWPF.Windows.Data;

using UnityEngine;
using UnityEngine.SceneManagement;

using Random = UnityEngine.Random;

#nullable enable

public class LevelManager : Singleton<LevelManager>
{
    [field: SerializeField] private bool dummy { get; set; } = false;
    [field: SerializeField] private Level[] levels { get; set; } = default!;
    private Level currentLevel { get; set; }
    private uint currentLevelIndex { get; set; }

    [field: SerializeField] private GameObject backgroundContainer { get; set; } = null!;
    [field: SerializeField] private GameObject backgroundPrefab { get; set; } = null!;

    private Bounds spawnZone { get; set; }
    private List<Circle2D> noEnemySpawningZones { get; set; } = new();

    private bool shouldResetPlayerPhysics { get; set; }
    private Queue<PlayerController> playersToResetPhysicsFor { get; set; } = new();

    public Dictionary<int, GameObject> spawnedList { get; set; } = new();

    public ObservableDictionary<int, Enemy> enemies { get; private set; } = new(); 
    public Dictionary<int, GameObject> otherLevelContent { get; private set; } = new();


    protected override void Awake()
    {
        base.Awake();
        enemies.CollectionChanged += SpawnNextLevelOnCollectionAllRemoved;
        spawnZone = WrapBehaviour.cameraBounds;
        LoadLevel(0);
    }

    private void SpawnNextLevelOnCollectionAllRemoved(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (!dummy
            && e.OldItems != null
            && sender is ObservableDictionary<int, Enemy> dictionary
            && dictionary.Count == 0)
            LoadNextLevel();
    }

    private void FixedUpdate()
    {
        if (shouldResetPlayerPhysics)
        {
            var rb = GameManager.instance.player.GetComponent<Rigidbody2D>();
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0;
            shouldResetPlayerPhysics = false;
        }
    }

    public void LoadNextLevel()
    {
        currentLevelIndex++;
        LoadLevel(currentLevelIndex);
    }
    private void DestroyPreviousLevelContents()
    {
        foreach (var enemy in enemies.Values)
            Destroy(enemy.gameObject);
        enemies.Clear();

        foreach (var (_, content) in otherLevelContent)
            Destroy(content);
        otherLevelContent.Clear();
    }

    public void LoadLevel(uint index)
    {
        DestroyPreviousLevelContents();

        currentLevelIndex = index;
        if (currentLevelIndex >= levels.Length)
        {
            if (GameManager.instance != null)
                GameManager.instance.AddScene(GameManager.Scene.End);
            return;
        }

        currentLevel = levels[index];

        SetBackground();
        SpawnPlayer(currentLevel.includesPlayer);

        List<Circle2D> spawnedEnemies = new();
        foreach (var (enemy, amount) in currentLevel.enemiesToSpawn)
            for (int i = 0; i < amount; i++)
            {
                SpawnWithin(
                    enemy.GetComponent<CircleCollider2D>(),
                    spawnZone,
                    noEnemySpawningZones,
                    spawnedEnemies);
            }
    }

    private void SetBackground() => SetBackground(backgroundContainer, currentLevel, backgroundPrefab);
    private static void SetBackground(GameObject backgroundContainer, Level level, GameObject backgroundPrefab)
    {
        if (backgroundContainer == null)
            return;

        foreach (Transform child in backgroundContainer.transform)
            Destroy(child.gameObject);

        for (int i = 0; i < level.backgrounds.Length; i++)
        {
            var spriteRenderer = Instantiate(backgroundPrefab, backgroundContainer.transform, false)
                .GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = level.backgrounds[i];
            spriteRenderer.sortingOrder = i;
            spriteRenderer.transform.localScale = ScaleSpriteToCover(spriteRenderer, WrapBehaviour.cameraBounds);
        }

        static Vector2 ScaleSpriteToCover(SpriteRenderer spriteRenderer, Bounds cameraBounds)
        {
            var idealXScale = cameraBounds.size.x / spriteRenderer.sprite.bounds.size.x / spriteRenderer.transform.lossyScale.x;
            var idealYScale = cameraBounds.size.y / spriteRenderer.sprite.bounds.size.y / spriteRenderer.transform.lossyScale.y;
            var idealScale = Mathf.Max(idealXScale, idealYScale);
            return Vector2.one * idealScale;
        }
    }

    public void SpawnPlayer(bool shouldBeEnabled)
    {
        if (GameManager.instance == null)
            return;

        var player = GameManager.instance.player;
        if (player != null)
        {
            player.transform.SetPositionAndRotation(spawnZone.center, Quaternion.identity);
            playersToResetPhysicsFor.Enqueue(player);
            shouldResetPlayerPhysics = true;
        }
        else
        {
            var playerPrefab = GameManager.instance.playerPrefab != null
                ? GameManager.instance.playerPrefab
                : throw new MissingMemberException("No player prefab assigned to game manager!");

            player = Instantiate(playerPrefab, spawnZone.center, Quaternion.identity, transform);
            GameManager.instance.player = player;

            Bounds playerColliderBounds = GeometryUtility.CalculateBounds(
                player.GetComponent<PolygonCollider2D>().points.Select(x => (Vector3)x).ToArray(),
                player.transform.localToWorldMatrix);
            noEnemySpawningZones.Add(
                new(spawnZone.center + playerColliderBounds.center,
                currentLevel.safetyzoneSizeRelativeToPlayer * 0.5f * (playerColliderBounds.size.x + playerColliderBounds.size.y)));
        }

        player.gameObject.SetActive(shouldBeEnabled);
        if (shouldBeEnabled)
            player.BecomeTemporarilyInvisible(currentLevel.invincibilityDuration);
    }

    public void SpawnWithin(
        CircleCollider2D toSpawn,
        Bounds spawnArea,
        IEnumerable<Circle2D>? nospawnAreas = null,
        IList<Circle2D>? otherSpawnedThings = null,
        int allowedOverlaps = 0)
    {
        GameObject? spawned = SpawnWithin(transform, toSpawn, spawnArea, nospawnAreas, otherSpawnedThings, allowedOverlaps);
        if (spawned != null)
            spawnedList.Add(spawned.GetInstanceID(), spawned);
    }
    public static GameObject? SpawnWithin(
        Transform transform,
        CircleCollider2D toSpawn,
        Bounds spawnArea,
        IEnumerable<Circle2D>? nospawnAreas = null,
        IList<Circle2D>? otherSpawnedThings = null,
        int allowedOverlaps = 0)
    {
        Circle2D circle = new(Vector2.zero, toSpawn.radius * 0.5f * (transform.lossyScale.x + transform.lossyScale.y));
        int attemptsLeft = 10;
        do
        {
            circle.center = spawnArea.PickRandomPoint();
            attemptsLeft--;
        }
        while (attemptsLeft > 0 && CircleOverlapsSomething(nospawnAreas, otherSpawnedThings, allowedOverlaps, circle));
        if (attemptsLeft == 0)
            return null;

        GameObject spawned = Instantiate(toSpawn.gameObject, circle.center, Quaternion.identity, transform);
        otherSpawnedThings?.Add(circle);
        return spawned;
    }

    public void SpawnWithin(
        CircleCollider2D toSpawn,
        Circle2D spawnArea,
        IEnumerable<Circle2D> nospawnAreas,
        IList<Circle2D> otherSpawnedThings,
        int allowedOverlaps = 0)
    {
        Circle2D circle = new(Vector2.zero, toSpawn.radius * 0.5f * (transform.lossyScale.x + transform.lossyScale.y));
        int attemptsLeft = 10;
        do
        {
            circle.center = spawnArea.center + (spawnArea.radius * Random.insideUnitCircle);
            attemptsLeft--;
        }
        while (attemptsLeft > 0 && CircleOverlapsSomething(nospawnAreas, otherSpawnedThings, allowedOverlaps, circle));
        if (attemptsLeft == 0)
            return;

        GameObject spawned = Instantiate(toSpawn.gameObject, circle.center, Quaternion.identity, transform);

        otherSpawnedThings.Add(circle);
        spawnedList.Add(spawned.GetInstanceID(), spawned);
    }

    private static bool CircleOverlapsSomething(IEnumerable<Circle2D>? nospawnAreas, IList<Circle2D>? otherSpawnedThings, int allowedOverlaps, Circle2D circle)
    {
        int collisions = Physics2D.OverlapCircle(circle.center, circle.radius, new(), Array.Empty<Collider2D>());
        bool overlapsOtherSpawned = otherSpawnedThings?.Any(x => circle.Overlaps(x)) ?? false;
        bool isWithinNospawnArea = nospawnAreas?.Any(x => circle.Overlaps(x)) ?? false;

        return (collisions > allowedOverlaps) || overlapsOtherSpawned || isWithinNospawnArea;
    }

    public void Despawn(GameObject gameObject)
    {
        spawnedList.Remove(gameObject.GetInstanceID());
        Destroy(gameObject);
    }

    public void SpawnWithin(
        CircleCollider2D objectToSpawn,
        Bounds spawnArea,
        IList<Vector2> posList,
        int allowedOverlaps = 0)
    {
        Vector2 pos;
        int attemptsLeft = 10;
        do
        {
            pos = spawnArea.PickRandomPoint();
            attemptsLeft--;
        }
        while (attemptsLeft > 0 && PosOverlapsSomething(pos));
        if (attemptsLeft == 0)
            return;

        var spawned = Instantiate(objectToSpawn.gameObject, pos, Quaternion.identity, transform);
        spawned.AddComponent<WrapBehaviour>();
        posList.Add(pos);

        bool PosOverlapsSomething(Vector2 pos)
        {
            ContactFilter2D filter = new();
            Collider2D[] results = new Collider2D[5];
            int collisions = Physics2D.OverlapCircle(pos, objectToSpawn.radius, filter, results);

            bool overlapsOtherSpawned = posList.Any(x => Vector2.Distance(pos, x) < objectToSpawn.radius);

            return (collisions > allowedOverlaps) || overlapsOtherSpawned;
        }
    }
}
