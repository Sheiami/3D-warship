using UnityEngine;

public class ShipCombat : MonoBehaviour
{
    [Header("Ціль")]
    [SerializeField, Min(0.02f)] float targetRefreshInterval = 0.2f;

    [Header("Трасер")]
    [SerializeField] Material tracerMaterial;
    [SerializeField, Min(0.03f)] float tracerFlightTime = 0.2f;
    [SerializeField, Min(0.05f)] float tracerThickness = 1.5f;
    [SerializeField] GameObject hitEffectPrefab;
    [SerializeField, Min(0.1f)] float hitEffectLifetime = 2f;

    const float StreakFraction = 0.3f;

    static bool warnedNoTracer;
    static bool warnedNoHitEffect;

    Ship ship;
    BoxCollider box;
    LineRenderer tracer;
    Vector3 tracerFrom;
    Vector3 tracerTo;
    float tracerStartTime;
    float tracerLifetime = 0.1f;
    float nextFireTime;
    float refreshTimer;
    int weaponIndex;

    public Ship Target { get; private set; }
    public bool TargetInRange { get; private set; }
    public int WeaponIndex => weaponIndex;

    public WeaponData CurrentWeapon
    {
        get
        {
            WeaponData[] weapons = ship != null && ship.Data != null ? ship.Data.weapons : null;
            if (weapons == null || weapons.Length == 0) return null;
            return weapons[Mathf.Clamp(weaponIndex, 0, weapons.Length - 1)];
        }
    }

    public float RangeLimit { get; set; } = float.PositiveInfinity;

    public float CurrentRange => CurrentWeapon != null ? CurrentWeapon.range : 0f;

    public float EffectiveRange => CurrentWeapon != null ? Mathf.Min(CurrentWeapon.range, RangeLimit) : 0f;

    void Awake()
    {
        ship = GetComponent<Ship>();
        box = GetComponent<BoxCollider>();
    }

    void OnDestroy()
    {
        if (tracer) Destroy(tracer.gameObject);
    }

    public void SetTarget(Ship target)
    {
        Target = target;
        TargetInRange = false;
        refreshTimer = 0f;
    }

    public void ClearTarget()
    {
        Target = null;
        TargetInRange = false;
    }

    public bool SelectWeapon(int index)
    {
        WeaponData[] weapons = ship.Data != null ? ship.Data.weapons : null;
        if (weapons == null || index < 0 || index >= weapons.Length || weapons[index] == null) return false;

        weaponIndex = index;
        return true;
    }

    void Update()
    {
        UpdateTracer();

        if (Time.timeScale <= 0f) return;

        if ((object)Target == null)
        {
            TargetInRange = false;
            return;
        }

        if (Target == null || Target.Health.IsDead)
        {
            ClearTarget();
            ship.Mover.Stop();
            return;
        }

        WeaponData weapon = CurrentWeapon;
        if (weapon == null)
        {
            TargetInRange = false;
            return;
        }

        float range = Mathf.Min(weapon.range, RangeLimit);
        float sqrDistance = ship.FlatSqrDistanceTo(Target.transform.position);
        TargetInRange = sqrDistance <= range * range;

        if (TargetInRange)
        {
            if (ship.Mover.HasTarget) ship.Mover.Stop();
            TryFire(weapon);
            return;
        }

        refreshTimer -= Time.deltaTime;
        if (refreshTimer <= 0f)
        {
            refreshTimer = targetRefreshInterval;
            ship.Mover.SetTarget(Target.transform.position);
        }
    }

    void TryFire(WeaponData weapon)
    {
        if (Time.time < nextFireTime) return;
        if (weapon.ammo == null || weapon.fireRate <= 0f) return;

        nextFireTime = Time.time + 1f / weapon.fireRate;

        if (ship.Team == Team.Player && !PlayerStock.Instance.TryConsume(weapon.ammo))
        {
            CombatMessage.Show($"Немає набоїв: {weapon.ammo.displayName}");
            return;
        }

        RevealIfEnemy(ship);
        RevealIfEnemy(Target);

        float flightTime = Mathf.Min(tracerFlightTime, 0.9f / weapon.fireRate);
        ShowShotEffects(Target, flightTime);

        Target.Health.TakeDamage(weapon.ammo.damage);
    }

    static void RevealIfEnemy(Ship s)
    {
        EnemyVisibility visibility = s.GetComponent<EnemyVisibility>();
        if (visibility != null) visibility.Reveal();
    }

    void ShowShotEffects(Ship target, float flightTime)
    {
        BoxCollider targetBox = target.GetComponent<BoxCollider>();
        Vector3 targetCenter = targetBox != null ? targetBox.bounds.center : target.transform.position;

        Vector3 origin = box.ClosestPoint(targetCenter);
        Vector3 hitPoint = targetBox != null ? targetBox.ClosestPoint(origin) : targetCenter;

        DrawTracer(origin, hitPoint, flightTime);

        if (hitEffectPrefab)
        {
            GameObject fx = Instantiate(hitEffectPrefab, hitPoint, Quaternion.identity);
            Destroy(fx, hitEffectLifetime);
        }
        else if (!warnedNoHitEffect)
        {
            warnedNoHitEffect = true;
            Debug.LogWarning("ShipCombat: hitEffectPrefab не призначено — частинки влучання не показуються.", this);
        }
    }

    void DrawTracer(Vector3 from, Vector3 to, float flightTime)
    {
        if (tracer == null) tracer = CreateTracer();
        if (tracer == null) return;

        tracerFrom = from;
        tracerTo = to;
        tracerStartTime = Time.time;
        tracerLifetime = Mathf.Max(0.03f, flightTime);

        tracer.SetPosition(0, from);
        tracer.SetPosition(1, from);
        tracer.enabled = true;
    }

    void UpdateTracer()
    {
        if (tracer == null || !tracer.enabled) return;

        float t = (Time.time - tracerStartTime) / tracerLifetime;
        if (t >= 1f)
        {
            tracer.enabled = false;
            return;
        }

        float p = t * (1f + StreakFraction);
        float head = Mathf.Min(p, 1f);
        float tail = Mathf.Clamp01(p - StreakFraction);

        tracer.SetPosition(0, Vector3.Lerp(tracerFrom, tracerTo, tail));
        tracer.SetPosition(1, Vector3.Lerp(tracerFrom, tracerTo, head));
    }

    LineRenderer CreateTracer()
    {
        if (tracerMaterial == null)
        {
            if (!warnedNoTracer)
            {
                warnedNoTracer = true;
                Debug.LogWarning("ShipCombat: не призначено tracerMaterial на префабі ShipBase — трасер не показується.", this);
            }
            return null;
        }

        var go = new GameObject($"{name}_Tracer");
        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.useWorldSpace = true;
        lr.startWidth = tracerThickness;
        lr.endWidth = tracerThickness;
        lr.sharedMaterial = tracerMaterial;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;
        lr.enabled = false;
        return lr;
    }
}
