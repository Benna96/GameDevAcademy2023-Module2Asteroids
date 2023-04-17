using TMPro;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    [field: SerializeField] private TextMeshProUGUI score { get; set; }
    [field: SerializeField] private Transform livesContainer { get; set; }

    private void Start() {
        if (GameManager.instance == null)
            return;

        UpdateScore(GameManager.instance.score);
        UpdateLives(GameManager.instance.lives);
    }

    public void UpdateScore(int newScore)
    {
        if (score != null)
            score.text = $"Score: {newScore}";
    }

    public void UpdateLives(int newLives)
    {
        if (livesContainer != null)
            foreach (Transform item in livesContainer)
            {
                int i = item.GetSiblingIndex();
                item.gameObject.SetActive(i < newLives);
            }
    }
}
