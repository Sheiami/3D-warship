using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    public event Action Closed;

    [SerializeField] TMP_Dropdown qualityDropdown;
    [SerializeField] TMP_Dropdown resolutionDropdown;
    [SerializeField] TMP_Dropdown antiAliasingDropdown;
    [SerializeField] TMP_Dropdown fpsDropdown;
    [SerializeField] Toggle fullscreenToggle;
    [SerializeField] Toggle vsyncToggle;
    [SerializeField] Button applyButton;
    [SerializeField] Button backButton;

    readonly List<Vector2Int> resolutions = new List<Vector2Int>();

    void Awake()
    {
        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(new List<string>(QualitySettings.names));

        foreach (var r in Screen.resolutions)
        {
            var size = new Vector2Int(r.width, r.height);
            if (!resolutions.Contains(size)) resolutions.Add(size);
        }
        var resLabels = new List<string>();
        foreach (var s in resolutions) resLabels.Add($"{s.x} x {s.y}");
        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(resLabels);

        antiAliasingDropdown.ClearOptions();
        antiAliasingDropdown.AddOptions(new List<string>(GameSettings.AANames));

        var fpsLabels = new List<string>();
        foreach (int f in GameSettings.FpsOptions) fpsLabels.Add(f == 0 ? "Unlimited" : f.ToString());
        fpsDropdown.ClearOptions();
        fpsDropdown.AddOptions(fpsLabels);

        vsyncToggle.onValueChanged.AddListener(on => fpsDropdown.interactable = !on);
        applyButton.onClick.AddListener(OnApply);
        backButton.onClick.AddListener(OnBack);
    }

    void OnEnable()
    {
        qualityDropdown.SetValueWithoutNotify(GameSettings.QualityLevel);
        int resIndex = resolutions.IndexOf(GameSettings.ScreenSize);
        resolutionDropdown.SetValueWithoutNotify(resIndex >= 0 ? resIndex : resolutions.Count - 1);
        antiAliasingDropdown.SetValueWithoutNotify(GameSettings.AAIndex);
        fpsDropdown.SetValueWithoutNotify(Mathf.Max(0, Array.IndexOf(GameSettings.FpsOptions, GameSettings.FpsCap)));
        fullscreenToggle.SetIsOnWithoutNotify(GameSettings.Fullscreen);
        vsyncToggle.SetIsOnWithoutNotify(GameSettings.VSync);
        fpsDropdown.interactable = !GameSettings.VSync;
    }

    void OnApply()
    {
        GameSettings.QualityLevel = qualityDropdown.value;
        GameSettings.ScreenSize = resolutions[resolutionDropdown.value];
        GameSettings.AAIndex = antiAliasingDropdown.value;
        GameSettings.FpsCap = GameSettings.FpsOptions[fpsDropdown.value];
        GameSettings.Fullscreen = fullscreenToggle.isOn;
        GameSettings.VSync = vsyncToggle.isOn;
        GameSettings.Apply();
        GameSettings.Save();
    }

    void OnBack() => Closed?.Invoke();
}