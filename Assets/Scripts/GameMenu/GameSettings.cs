using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.HighDefinition;

public static class GameSettings
{
    public static readonly HDAdditionalCameraData.AntialiasingMode[] AAModes =
    {
        HDAdditionalCameraData.AntialiasingMode.None,
        HDAdditionalCameraData.AntialiasingMode.FastApproximateAntialiasing,
        HDAdditionalCameraData.AntialiasingMode.TemporalAntialiasing,
        HDAdditionalCameraData.AntialiasingMode.SubpixelMorphologicalAntiAliasing
    };
    public static readonly string[] AANames = { "Off", "FXAA", "TAA", "SMAA" };
    public static readonly int[] FpsOptions = { 0, 30, 60, 120, 144, 240 };

    public static int QualityLevel;
    public static Vector2Int ScreenSize;
    public static bool Fullscreen;
    public static bool VSync;
    public static int FpsCap;
    public static int AAIndex;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        Load();
        Apply();
        SceneManager.sceneLoaded += (scene, mode) => ApplyToCameras();
    }

    public static void Load()
    {
        QualityLevel = Mathf.Clamp(
            PlayerPrefs.GetInt("gfx.quality", QualitySettings.GetQualityLevel()),
            0, QualitySettings.names.Length - 1);
        ScreenSize = new Vector2Int(
            PlayerPrefs.GetInt("gfx.resW", Screen.currentResolution.width),
            PlayerPrefs.GetInt("gfx.resH", Screen.currentResolution.height));
        Fullscreen = PlayerPrefs.GetInt("gfx.full", 1) == 1;
        VSync = PlayerPrefs.GetInt("gfx.vsync", 1) == 1;
        FpsCap = PlayerPrefs.GetInt("gfx.fps", 0);
        AAIndex = Mathf.Clamp(PlayerPrefs.GetInt("gfx.aa", 1), 0, AAModes.Length - 1);
    }

    public static void Save()
    {
        PlayerPrefs.SetInt("gfx.quality", QualityLevel);
        PlayerPrefs.SetInt("gfx.resW", ScreenSize.x);
        PlayerPrefs.SetInt("gfx.resH", ScreenSize.y);
        PlayerPrefs.SetInt("gfx.full", Fullscreen ? 1 : 0);
        PlayerPrefs.SetInt("gfx.vsync", VSync ? 1 : 0);
        PlayerPrefs.SetInt("gfx.fps", FpsCap);
        PlayerPrefs.SetInt("gfx.aa", AAIndex);
        PlayerPrefs.Save();
    }

    public static void Apply()
    {
        QualitySettings.SetQualityLevel(QualityLevel, true);
        QualitySettings.vSyncCount = VSync ? 1 : 0;
        Application.targetFrameRate = FpsCap > 0 ? FpsCap : -1;
        Screen.SetResolution(ScreenSize.x, ScreenSize.y,
            Fullscreen ? FullScreenMode.ExclusiveFullScreen : FullScreenMode.Windowed);
        ApplyToCameras();
    }

    static void ApplyToCameras()
    {
        var mode = AAModes[AAIndex];
        foreach (var cam in Object.FindObjectsByType<HDAdditionalCameraData>(FindObjectsSortMode.None))
            cam.antialiasing = mode;
    }
}