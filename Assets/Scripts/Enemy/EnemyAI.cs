using UnityEngine;

public enum EnemyState { Guard, Chase, Attack, Search }

[RequireComponent(typeof(Ship))]
public class EnemyAI : MonoBehaviour
{
    [SerializeField, Min(0f)] float detectRange = 600f;
    [SerializeField, Min(1f)] float loseRangeFactor = 1.2f;
    [SerializeField, Min(0.05f)] float thinkInterval = 0.2f;
    [SerializeField, Min(0f)] float searchMargin = 50f;

    public EnemyState State { get; private set; } = EnemyState.Search;

    Ship ship;
    DefenseZone zone;
    MapBounds map;
    float centerSpread;
    bool headToCenter;
    bool wasLast;
    Vector3 lastKnownPos;
    bool searchLastKnown;
    float timer;

    void Awake() => ship = GetComponent<Ship>();

    public void Init(DefenseZone fallbackZone, MapBounds searchArea, float detect, float spread)
    {
        zone = fallbackZone;
        map = searchArea;
        detectRange = detect;
        centerSpread = spread;
        headToCenter = searchArea != null;
    }

    void Update()
    {
        if (Time.timeScale <= 0f) return;

        timer -= Time.deltaTime;
        if (timer > 0f) return;

        timer = thinkInterval;
        Think();
    }

    void Think()
    {
        if (ship.Health.IsDead) return;

        Ship target = ChooseTarget();

        if (target != null)
        {
            lastKnownPos = target.transform.position;
            if (ship.Combat.Target != target) ship.Combat.SetTarget(target);
            State = ship.Combat.TargetInRange ? EnemyState.Attack : EnemyState.Chase;
            return;
        }

        if (State == EnemyState.Chase || State == EnemyState.Attack)
        {
            ship.Combat.ClearTarget();
            ship.Mover.Stop();
            State = EnemyState.Search;
            searchLastKnown = true;
        }

        if (State == EnemyState.Search)
        {
            bool last = map != null && IsLastEnemy();

            if (last && !wasLast) ship.Mover.Stop();
            wasLast = last;

            if (!ship.Mover.HasTarget) GoToNextSearchPoint(last);
        }
    }

    Ship ChooseTarget()
    {
        float detect = Mathf.Max(detectRange, ship.Combat.EffectiveRange);

        Ship current = ship.Combat.Target;
        if (current != null && !current.Health.IsDead)
        {
            float keep = detect * loseRangeFactor;
            if (ship.FlatSqrDistanceTo(current.transform.position) <= keep * keep) return current;
        }

        Ship best = null;
        float bestSqr = detect * detect;
        var all = Ship.All;

        for (int i = 0; i < all.Count; i++)
        {
            Ship s = all[i];
            if (s == null || s.Team != Team.Player || s.Health.IsDead) continue;

            float sqr = ship.FlatSqrDistanceTo(s.transform.position);
            if (sqr <= bestSqr)
            {
                bestSqr = sqr;
                best = s;
            }
        }

        return best;
    }

    bool IsLastEnemy()
    {
        var all = Ship.All;

        for (int i = 0; i < all.Count; i++)
        {
            Ship s = all[i];
            if (s == null || s == ship || s.Team != Team.Enemy || s.Health.IsDead) continue;
            return false;
        }

        return true;
    }

    void GoToNextSearchPoint(bool lastEnemy)
    {
        Vector3 point;

        if (lastEnemy) point = CenterPoint();
        else if (searchLastKnown)
        {
            point = lastKnownPos;
            searchLastKnown = false;
        }
        else if (headToCenter)
        {
            point = CenterPoint();
            headToCenter = false;
        }
        else if (map != null) point = map.RandomPoint(searchMargin);
        else if (zone != null) point = zone.RandomPoint(searchMargin);
        else return;

        ship.Mover.SetTarget(point);
    }

    Vector3 CenterPoint()
    {
        Vector2 offset = Random.insideUnitCircle * centerSpread;
        Vector3 center = map.transform.position;
        return new Vector3(center.x + offset.x, center.y, center.z + offset.y);
    }
}