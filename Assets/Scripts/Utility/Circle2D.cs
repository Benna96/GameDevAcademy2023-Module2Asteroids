using UnityEngine;

public class Circle2D
{
    public Vector2 center { get; set; }
    public float radius { get; set; }

    public Circle2D() { }
    public Circle2D(Vector2 center, float radius)
    {
        this.center = center;
        this.radius = radius;
    }
    public Circle2D(CircleCollider2D circleCollider)
    {
        center = (Vector2)circleCollider.gameObject.transform.position + circleCollider.offset;
        radius = circleCollider.radius;
    }

    public bool Overlaps(Circle2D other)
    {
        float distance = Vector2.Distance(center, other.center);
        return distance <= (radius + other.radius);
    }
}