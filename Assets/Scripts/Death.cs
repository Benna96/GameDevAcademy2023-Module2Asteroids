using UnityEngine;

public class Death : MonoBehaviour
{
#pragma warning disable IDE0051 // Remove unused private members
    private void OnStart() => GameManager.instance.Reset();
#pragma warning restore IDE0051 // Remove unused private members
}
