using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "Module2Asteroids/Level", order = 0)]
public class Level : ScriptableObject
{
    [field: SerializeField] public bool includesPlayer { get; private set; } = true; 
    [field: SerializeField] public GameObjectIntDictionary enemiesToSpawn { get; private set; } = new();
    [field: SerializeField] public float safetyzoneSizeRelativeToPlayer { get; private set; }
    [field: SerializeField] public float invincibilityDuration { get; private set; }
    [field: SerializeField] public Sprite[] backgrounds { get; private set; }

    public void Load() { }
    public void Unload() { }
    public void Reload() { }
}