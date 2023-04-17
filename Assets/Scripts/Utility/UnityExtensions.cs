using UnityEngine;

public static class TransformExtensions
{
    public static Transform GetRelative(this Transform transform, string name)
    {
        Transform parent = transform;
        string[] nameParts = name.Split('/');
        foreach (var namePart in nameParts)
            parent = namePart switch
            {
                "." => parent,
                ".." => parent != null ? parent.parent : null,
                "_" => null,
                _ => GetChild(parent, namePart)
            };

        return parent;

        static Transform GetChild(Transform parent, string namePart)
        {
            Transform child;
            if (parent == null)
            {
                GameObject foundGameObject = GameObject.Find(namePart);
                child = foundGameObject != null ? foundGameObject.transform : null;
            }
            else
                child = parent.Find(namePart);

            if (child == null)
            {
                child = new GameObject(namePart).transform;
                child.SetParent(parent, false);
            }

            parent = child;
            return parent;
        }
    }
}

public static class BoundsExtensions
{
    public static Vector2 PickRandomPoint(this Bounds bounds)
    {
        return new(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y)
        );
    }
}
