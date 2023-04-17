using System.Linq;

using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    [field: SerializeField] public PlayerController playerPrefab { get; private set; }
    public PlayerController player { get; set; }

    private const int initialLives = 3;
    public int lives { get; private set; }

    private const int initialScore = 0;
    public int score { get; private set; }

    public enum Scene
    {
        Master,
        Title,
        Level,
        Death,
        End
    }

    protected override void Awake()
    {
        base.Awake();
        Reset();
    }

    public void Reset()
    {
        UnloadAllOtherScenes();
        SceneManager.LoadSceneAsync((int)Scene.Title, LoadSceneMode.Additive);
        lives = initialLives;
        score = initialScore;
    }

    private void UnloadAllOtherScenes()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (scene.handle != gameObject.scene.handle)
                SceneManager.UnloadSceneAsync(scene);
        }
    }

    public void SwapScenes(UnityEngine.SceneManagement.Scene oldScene, Scene newScene)
    {
        SceneManager.UnloadSceneAsync(oldScene);
        AddScene(newScene);
    }
    public void AddScene(Scene newScene)
    {
        SceneManager.LoadScene((int)newScene, LoadSceneMode.Additive);

        if (newScene == Scene.Level)
            LevelManager.instance.LoadLevel(1);

        // If adding end scene onto level scene, mute level scene audio
        else if (newScene == Scene.End)
        {
            UnityEngine.SceneManagement.Scene? maybeLevelScene = null;
            for (int i = 0; i <= SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.buildIndex == (int)Scene.Level)
                {
                    maybeLevelScene = scene;
                    break;
                }
            }

            if (maybeLevelScene is UnityEngine.SceneManagement.Scene levelScene)
                foreach (var rootObject in levelScene.GetRootGameObjects())
                {
                    if (rootObject.name == "Audio")
                    {
                        var background = rootObject.transform.Find("Background");
                        if (background != null)
                        {
                            var backgroundAudios = background.GetComponents<AudioSource>();
                            foreach (var backgroundAudio in backgroundAudios)
                                backgroundAudio.volume = 0;
                        }
                    }
                }
        }
    }

    public void UpdateScore(int scoreChange)
    {
        score += scoreChange;
        if (UIManager.instance != null)
            UIManager.instance.UpdateScore(score);
    }
    public void UpdateLives(int livesChange)
    {
        lives += livesChange;

        if (UIManager.instance != null)
            UIManager.instance.UpdateLives(lives);

        if (livesChange < 0)
        {
            if (lives <= 0)
            {
                lives = 0;
                UnloadAllOtherScenes();
                if (LevelManager.instance != null)
                    LevelManager.instance.SpawnPlayer(false);
                AddScene(Scene.Death);
            }
            else if (LevelManager.instance != null)
                LevelManager.instance.SpawnPlayer(player.isActiveAndEnabled);
        }
    }

#pragma warning disable IDE0051 // Remove unused private members
    private void OnSoftReset() => Reset();
#pragma warning restore IDE0051 // Remove unused private members
}
