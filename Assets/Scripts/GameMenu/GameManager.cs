using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static bool IsOver { get; private set; }

    [Header("HUD")]
    [SerializeField] TMP_Text enemiesLabel;

    [Header("Екран результату")]
    [SerializeField] GameObject resultPanel;
    [SerializeField] TMP_Text resultTitle;
    [SerializeField] Button restartButton;
    [SerializeField] Button menuButton;
    [SerializeField, Min(0f)] float resultDelay = 3.5f;

    [Header("Сцени")]
    [SerializeField] string menuScene = "MainMenu";

    bool battleStarted;
    bool ended;
    int shownEnemies = -1;

    void Awake()
    {
        IsOver = false;

        PlayerStock.Instance.ResetState();

        restartButton.onClick.AddListener(Restart);
        menuButton.onClick.AddListener(ToMenu);
        resultPanel.SetActive(false);
    }

    void Update()
    {
        if (ended) return;

        int own = 0, enemies = 0;
        var all = Ship.All;

        for (int i = 0; i < all.Count; i++)
        {
            Ship s = all[i];
            if (s == null || s.Health.IsDead) continue;

            if (s.Team == Team.Player) own++;
            else enemies++;
        }

        if (enemies != shownEnemies)
        {
            shownEnemies = enemies;
            enemiesLabel.text = $"Ворогів: {enemies}";
        }

        if (own > 0 && enemies > 0)
        {
            battleStarted = true;
            return;
        }

        if (!battleStarted) return;

        ended = true;
        IsOver = true;
        StartCoroutine(ShowResult(own > 0));
    }

    IEnumerator ShowResult(bool win)
    {
        yield return new WaitForSeconds(resultDelay);

        resultTitle.text = win ? "Перемога" : "Поразка";
        resultPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void ToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuScene);
    }
}