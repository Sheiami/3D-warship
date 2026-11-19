using TMPro;
using UnityEngine;

public class CombatMessage : MonoBehaviour
{
    public static CombatMessage Instance { get; private set; }

    [SerializeField] TMP_Text label;
    [SerializeField, Min(0.1f)] float duration = 2f;

    float hideTime;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        if (label) label.enabled = false;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public static void Show(string text)
    {
        if (Instance == null) return;
        Instance.Display(text);
    }

    void Display(string text)
    {
        if (!label) return;

        label.text = text;
        label.enabled = true;
        hideTime = Time.unscaledTime + duration;
    }

    void Update()
    {
        if (label && label.enabled && Time.unscaledTime >= hideTime)
            label.enabled = false;
    }
}
