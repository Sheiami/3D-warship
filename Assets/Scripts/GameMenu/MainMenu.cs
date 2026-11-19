using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] Button playButton;
    [SerializeField] Button settingsButton;
    [SerializeField] Button quitButton;
    [SerializeField] GameObject mainPanel;
    [SerializeField] SettingsMenu settingsMenu;
    [SerializeField] string gameSceneName = "Game";

    void Awake()
    {
        playButton.onClick.AddListener(Play);
        settingsButton.onClick.AddListener(OpenSettings);
        quitButton.onClick.AddListener(Quit);
        settingsMenu.Closed += CloseSettings;
    }

    void OnDestroy() => settingsMenu.Closed -= CloseSettings;

    void Play() => SceneManager.LoadScene(gameSceneName);

    void OpenSettings()
    {
        mainPanel.SetActive(false);
        settingsMenu.gameObject.SetActive(true);
    }

    void CloseSettings()
    {
        settingsMenu.gameObject.SetActive(false);
        mainPanel.SetActive(true);
    }

    void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}