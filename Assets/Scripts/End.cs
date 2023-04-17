using UnityEngine;

public class End : MonoBehaviour
{
#pragma warning disable IDE0051 // Remove unused private members
    private void OnQuit()
#pragma warning restore IDE0051 // Remove unused private members
    {
        if (GameManager.instance != null)
            GameManager.instance.Reset();
    }
}
