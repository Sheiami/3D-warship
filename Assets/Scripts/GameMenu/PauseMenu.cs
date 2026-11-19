using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public static bool IsPaused { get; private set; }

    [SerializeField] GameObject panel;
    [SerializeField] Button resumeButton;
    [SerializeField] Button escapeButton;

    [SerializeField] Button menuButton;
    [SerializeField] string menuSceneName = "MainMenu";

    CursorLockMode savedLock;
    bool savedVisible;

    void Awake()
    {
        IsPaused = false;
        escapeButton.onClick.AddListener(Pause);
        resumeButton.onClick.AddListener(Resume);
        menuButton.onClick.AddListener(GoToMenu);
        panel.SetActive(false);
    }

    void Update()
    {
        if (!EscapePressed()) return;
        if (IsPaused) Resume(); else Pause();
    }

    void Pause()
    {
        if (GameManager.IsOver) return;

        savedLock = Cursor.lockState;
        savedVisible = Cursor.visible;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;
        IsPaused = true;
        panel.SetActive(true);
    }

    void Resume()
    {
        panel.SetActive(false);
        Time.timeScale = 1f;
        IsPaused = false;

        Cursor.lockState = savedLock;
        Cursor.visible = savedVisible;
        EventSystem.current.SetSelectedGameObject(null);
    }

    void GoToMenu()
    {
        Time.timeScale = 1f;
        IsPaused = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(menuSceneName);
    }

    void OnDestroy()
    {
        Time.timeScale = 1f;
        IsPaused = false;
    }

    static bool EscapePressed()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = UnityEngine.InputSystem.Keyboard.current;
        return kb != null && kb.escapeKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Escape);
#endif
    }
}