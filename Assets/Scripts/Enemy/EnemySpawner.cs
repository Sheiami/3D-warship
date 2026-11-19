using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] GameObject shipBasePrefab;
    [SerializeField] DefenseZone zone;
    [SerializeField] ShipData[] ships = new ShipData[5];
    [SerializeField, Min(0.01f)] float shipScale = 0.5f;

    [Header("Розміщення")]
    [SerializeField, Min(0f)] float minSpacing = 40f;
    [SerializeField, Min(0f)] float edgeMargin = 15f;
    [SerializeField, Min(1)] int maxAttempts = 30;

    [Header("AI")]
    [SerializeField, Min(0f)] float visionRadius = 400f;
    [SerializeField, Min(0f)] float detectRange = 600f;
    [SerializeField, Min(0f)] float revealAfterShot = 3f;
    [SerializeField] bool hideInFog = false;
    [SerializeField] MapBounds map;
    [SerializeField, Min(0f)] float centerSpread = 150f;

    public Ship[] Spawned { get; private set; }

    void Start()
    {
        if (!zone)
        {
            Debug.LogError("EnemySpawner: поле Zone не призначено.", this);
            return;
        }

        if (!shipBasePrefab)
        {
            Debug.LogError("EnemySpawner: поле Ship Base Prefab не призначено.", this);
            return;
        }

        Spawned = new Ship[ships.Length];
        var placed = new List<Vector3>();

        for (int i = 0; i < ships.Length; i++)
        {
            if (ships[i] == null || ships[i].visualPrefab == null)
            {
                Debug.LogError($"EnemySpawner: ships[{i}] або його visualPrefab не призначено.", this);
                continue;
            }

            Vector3 position = PickPoint(placed);
            placed.Add(position);

            Quaternion rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            Ship ship = ShipFactory.Spawn(shipBasePrefab, ships[i], position, rotation, shipScale);
            if (ship == null) continue;

            ship.Init(ships[i], Team.Enemy);
            ship.gameObject.AddComponent<EnemyVisibility>().Init(visionRadius, revealAfterShot, hideInFog);
            ship.gameObject.AddComponent<EnemyAI>().Init(zone, map, detectRange, centerSpread);

            Spawned[i] = ship;
        }
    }

    Vector3 PickPoint(List<Vector3> placed)
    {
        Vector3 candidate = zone.RandomPoint(edgeMargin);

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            candidate = zone.RandomPoint(edgeMargin);
            if (FarEnough(candidate, placed)) return candidate;
        }

        return candidate;
    }

    bool FarEnough(Vector3 point, List<Vector3> placed)
    {
        float sqrMin = minSpacing * minSpacing;

        for (int i = 0; i < placed.Count; i++)
        {
            Vector3 d = point - placed[i];
            d.y = 0f;
            if (d.sqrMagnitude < sqrMin) return false;
        }

        return true;
    }
}
