using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Team { Player, Enemy }

[RequireComponent(typeof(Rigidbody), typeof(BoxCollider), typeof(Health))]
[RequireComponent(typeof(ShipMover))]
public class Ship : MonoBehaviour
{
    static readonly List<Ship> all = new List<Ship>();
    public static IReadOnlyList<Ship> All => all;

    static bool warnedNoExplosion;

    [Header("Смерть")]
    [SerializeField] GameObject explosionPrefab;
    [SerializeField, Min(0.1f)] float explosionLifetime = 8f;
    [SerializeField, Min(0.1f)] float sinkTime = 3f;
    [SerializeField, Min(0f)] float sinkDepthFactor = 1.2f;
    [SerializeField, Range(0f, 45f)] float sinkTilt = 15f;

    public ShipData Data { get; private set; }
    public Health Health { get; private set; }
    public ShipMover Mover { get; private set; }
    public ShipCombat Combat { get; private set; }
    public Team Team { get; private set; } = Team.Player;

    public bool IsVisible { get; set; } = true;
    public bool IsHoverable => IsVisible;

    GameObject moveMarker;

    void Awake()
    {
        Health = GetComponent<Health>();
        Mover = GetComponent<ShipMover>();
        Combat = GetOrAdd<ShipCombat>();
        GetOrAdd<ShipHighlight>();
        Health.OnDeath += HandleDeath;
    }

    void OnEnable() => all.Add(this);
    void OnDisable() => all.Remove(this);

    public void Init(ShipData data, Team team = Team.Player)
    {
        Data = data;
        Team = team;
        Health.Init(data.maxHP);
        Mover.Init(data.speed, data.turnSpeed);
    }

    public void OrderMove(Vector3 point, GameObject markerPrefab)
    {

        Combat.ClearTarget();
        Mover.SetTarget(point);

        if (!markerPrefab) return;
        if (!moveMarker) moveMarker = Instantiate(markerPrefab);
        moveMarker.transform.position = point;
        moveMarker.SetActive(false);
        moveMarker.SetActive(true);
    }


    public float FlatSqrDistanceTo(Vector3 point)
    {
        Vector3 d = point - transform.position;
        d.y = 0f;
        return d.sqrMagnitude;
    }

    void Update()
    {
        if (moveMarker && moveMarker.activeSelf && !Mover.HasTarget)
            moveMarker.SetActive(false);
    }

    void OnDestroy()
    {
        if (moveMarker) Destroy(moveMarker);
    }

    void HandleDeath()
    {
        SelectionManager sm = SelectionManager.Instance;
        if (sm != null) sm.Deselect(this);

        Combat.ClearTarget();
        Mover.Stop();
        Mover.enabled = false;

        BoxCollider box = GetComponent<BoxCollider>();
        box.enabled = false;

        EnemyAI ai = GetComponent<EnemyAI>();
        if (ai != null) ai.enabled = false;

        Explode(transform.TransformPoint(box.center));
        StartCoroutine(Sink(Vector3.Scale(box.size, transform.lossyScale).y * sinkDepthFactor));
    }

    void Explode(Vector3 position)
    {
        if (!explosionPrefab)
        {
            if (!warnedNoExplosion)
            {
                warnedNoExplosion = true;
                Debug.LogWarning("Ship: не призначено explosionPrefab на префабі ShipBase — вибух не показується.", this);
            }
            return;
        }

        GameObject fx = Instantiate(explosionPrefab, position, Quaternion.identity);
        Destroy(fx, explosionLifetime);
    }

    IEnumerator Sink(float depth)
    {
        GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.None;

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        Vector3 endPos = startPos + Vector3.down * depth;
        Quaternion endRot = startRot * Quaternion.Euler(sinkTilt, 0f, Random.value < 0.5f ? -sinkTilt : sinkTilt);

        for (float t = 0f; t < sinkTime; t += Time.deltaTime)
        {
            float k = t / sinkTime;
            transform.SetPositionAndRotation(Vector3.Lerp(startPos, endPos, k), Quaternion.Slerp(startRot, endRot, k));
            yield return null;
        }

        Destroy(gameObject);
    }

    T GetOrAdd<T>() where T : Component
    {
        T c = GetComponent<T>();
        return c ? c : gameObject.AddComponent<T>();
    }
}