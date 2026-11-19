using UnityEngine;

[RequireComponent(typeof(Ship))]
public class EnemyVisibility : MonoBehaviour
{
    const float SpotHysteresis = 1.25f;

    [SerializeField] bool hideInFog;
    [SerializeField, Min(0f)] float visionRadius = 400f;
    [SerializeField, Min(0f)] float revealAfterShot = 3f;
    [SerializeField, Min(0.02f)] float checkInterval = 0.1f;

    Ship ship;
    Renderer[] renderers = new Renderer[0];
    bool[] wasEnabled = new bool[0];
    bool visible = true;
    bool spotted;
    float revealUntil;
    float timer;

    void Awake() => ship = GetComponent<Ship>();

    public void Init(float radius, float revealSeconds, bool hide)
    {
        visionRadius = radius;
        revealAfterShot = revealSeconds;
        hideInFog = hide;

        ship.Combat.RangeLimit = radius;
    }

    public void Reveal()
    {
        revealUntil = Time.time + revealAfterShot;
        Apply(true);
    }

    void Start() => Check();

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer > 0f) return;

        timer = checkInterval;
        Check();
    }

    void Check()
    {
        bool near = AnyPlayerShipWithin(visionRadius);

        if (near && !spotted)
        {
            spotted = true;
            string enemyName = ship.Data != null ? ship.Data.displayName : "невідомий";
            CombatMessage.Show($"Радар: помічено ворога — {enemyName}");
        }
        else if (!near && spotted && !AnyPlayerShipWithin(visionRadius * SpotHysteresis))
        {
            spotted = false;
        }

        if (hideInFog) Apply(near || Time.time < revealUntil);
    }

    bool AnyPlayerShipWithin(float radius)
    {
        float sqrRadius = radius * radius;
        var all = Ship.All;

        for (int i = 0; i < all.Count; i++)
        {
            Ship s = all[i];
            if (s == null || s.Team != Team.Player || s.Health.IsDead) continue;
            if (ship.FlatSqrDistanceTo(s.transform.position) <= sqrRadius) return true;
        }

        return false;
    }

    void Apply(bool shouldBeVisible)
    {
        if (shouldBeVisible == visible) return;

        visible = shouldBeVisible;
        ship.IsVisible = shouldBeVisible;

        if (shouldBeVisible) Show();
        else Hide();
    }

    void Hide()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
        wasEnabled = new bool[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            wasEnabled[i] = renderers[i].enabled;
            renderers[i].enabled = false;
        }
    }

    void Show()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i]) renderers[i].enabled = wasEnabled[i];
        }
    }
}
