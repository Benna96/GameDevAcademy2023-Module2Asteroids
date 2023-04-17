using UnityEngine;
using UnityEngine.InputSystem;

public class TitleScreen : MonoBehaviour
{
    private void Start()
    {
        if (LevelManager.instance != null)
            LevelManager.instance.LoadLevel(0);
    }

#pragma warning disable IDE0051 // Remove unused private members
    private void OnStart()
#pragma warning restore IDE0051 // Remove unused private members
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.SwapScenes(gameObject.scene, GameManager.Scene.Level);
        }
    }

#pragma warning disable IDE0051 // Remove unused private members
    private void OnQuit()
#pragma warning restore IDE0051 // Remove unused private members
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
