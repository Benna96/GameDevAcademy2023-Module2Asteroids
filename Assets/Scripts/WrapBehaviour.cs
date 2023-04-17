using System;

using UnityEngine;

public class WrapBehaviour : MonoBehaviour
{
    private static Bounds? _cameraBounds;
    public static Bounds cameraBounds { get
        {
            if (_cameraBounds != null)
                return (Bounds)_cameraBounds;

            Vector2 bottomLeft = Camera.main.ViewportToWorldPoint(new(0, 0));
            Vector2 topRight = Camera.main.ViewportToWorldPoint(new(1, 1));
            _cameraBounds = new Bounds(
                Vector2.Lerp(bottomLeft, topRight, 0.5f),
                new Vector2(topRight.x - bottomLeft.x, topRight.y - bottomLeft.y));
            return (Bounds)_cameraBounds;
        }
    }

    [field: SerializeField] private GameObject ghostObject { get; set; }
    protected bool isGhost { get; set; }
    private WrapBehaviour main { get; set; }
    private bool isDestroyingGhosts { get; set; }

    protected Transform[] ghosts = new Transform[8];

    protected virtual void Start() {
        if (!isGhost)
        {
            if (ghostObject == null)
                ghostObject = gameObject;
            CreateGhosts();
            AddToLevelManager();
        }
    }

    protected virtual void AddToLevelManager() { }

    private void CreateGhosts()
    {
        // Create "ghosts" to have nicer screen wrapping
        // https://gamedevelopment.tutsplus.com/articles/create-an-asteroids-like-screen-wrapping-effect-with-unity--gamedev-15055
        for (int i = 0; i < ghosts.Length; i++)
        {
            var ghost = Instantiate(ghostObject, CalcGhostPos(i), transform.rotation, transform.parent);

            if (!ghost.TryGetComponent<WrapBehaviour>(out var wrap))
                wrap = ghost.AddComponent<WrapBehaviour>();
            wrap.isGhost = true;
            wrap.main = this;

            ghosts[i] = ghost.transform;
        }
    }

    private Vector2 CalcGhostPos(int i)
    {
        // Ghost positions
        // ^ 7--0--1
        // | 6--•--2
        // | 5--4--3
        // | ------>
        Vector2 ghostPos = transform.position;
        if (i % 4 != 0)
            ghostPos.x += cameraBounds.size.x * (i < 4 ? 1 : -1);
        if (i % 4 != 2)
            ghostPos.y += cameraBounds.size.y * (i < 2 || i > 6 ? 1 : -1);
        return ghostPos;
    }

    protected virtual void Update()
    {
        if (isGhost)
            return;
        
        Vector2 currentPos = transform.position;

        bool xChanged = true;
        if (currentPos.x > cameraBounds.max.x)
            currentPos.x = cameraBounds.min.x;
        else if (currentPos.x < cameraBounds.min.x)
            currentPos.x = cameraBounds.max.x;
        else
            xChanged = false;

        bool yChanged = true;
        if (currentPos.y > cameraBounds.max.y)
            currentPos.y = cameraBounds.min.y;
        else if (currentPos.y < cameraBounds.min.y)
            currentPos.y = cameraBounds.max.y;
        else
            yChanged = false;
        
        if (xChanged || yChanged)
            transform.position = currentPos;

        for (int i = 0; i < ghosts.Length; i++)
        {
            var ghost = ghosts[i];
            if (ghost == null)
                continue;
            ghost.SetPositionAndRotation(CalcGhostPos(i), transform.rotation);
            ghost.localScale = transform.localScale;
        }
    }

    public virtual void Destroy() => Destroy(gameObject);

    protected virtual void OnEnable() => SetActiveGhosts(true);
    protected virtual void OnDisable() => SetActiveGhosts(false);
    private void SetActiveGhosts(bool value)
    {
        if (!isGhost && CompareTag("Player"))
            foreach (var ghost in ghosts)
                if (ghost != null)
                    ghost.gameObject.SetActive(value);
    }

    protected virtual void OnDestroy() {
        if (!isGhost)
        {
            isDestroyingGhosts = true;
            foreach (var ghost in ghosts)
                if (ghost != null)
                    Destroy(ghost.gameObject);
            isDestroyingGhosts = false;
            RemoveFromLevelManager();
        }
        else if (main != null && !main.isDestroyingGhosts)
            Destroy(main.gameObject);
    }

    protected virtual void RemoveFromLevelManager() { }
}
