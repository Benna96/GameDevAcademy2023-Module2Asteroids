using UnityEngine;

using Random = UnityEngine.Random;

public class Asteroid : MonoBehaviour
{
    [field: SerializeField] private float torqueStrength { get; set; }
    [field: SerializeField] private float forceStrength { get; set; }

    private void Awake() {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.AddTorque(Random.Range(-1f, 1f) * torqueStrength);
        rb.AddForce(Random.insideUnitCircle * forceStrength);
    }
}
