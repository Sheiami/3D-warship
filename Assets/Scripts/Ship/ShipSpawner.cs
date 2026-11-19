using UnityEngine;

public class ShipSpawner : MonoBehaviour
{
    [SerializeField] GameObject shipBasePrefab;
    [SerializeField] ShipData[] ships = new ShipData[5];
    [SerializeField] float spacing = 15f;
    [SerializeField, Min(0.01f)] float shipScale = 0.5f;
    [SerializeField, Min(0)] int starterPacks = 1;

    static readonly Vector2[] Wedge =
    {
        new Vector2( 0,  0),
        new Vector2(-1, -1), new Vector2(1, -1),
        new Vector2(-2, -2), new Vector2(2, -2),
    };

    public Ship[] Spawned { get; private set; }

    void Start()
    {
        int count = Mathf.Min(ships.Length, Wedge.Length);
        Spawned = new Ship[count];

        for (int i = 0; i < count; i++)
        {
            if (ships[i] == null || ships[i].visualPrefab == null)
            {
                Debug.LogError($"ShipSpawner: ships[{i}] or its visualPrefab is not assigned.", this);
                continue;
            }

            GrantStarterAmmo(ships[i]);

            Vector3 offset = new Vector3(Wedge[i].x, 0f, Wedge[i].y) * spacing;
            Spawned[i] = ShipFactory.Spawn(shipBasePrefab, ships[i], transform.TransformPoint(offset), transform.rotation, shipScale);
        }
    }

    void GrantStarterAmmo(ShipData data)
    {
        if (starterPacks <= 0 || data.weapons == null) return;

        foreach (WeaponData weapon in data.weapons)
        {
            if (weapon == null || weapon.ammo == null) continue;
            PlayerStock.Instance.GrantStarter(weapon.ammo, starterPacks * weapon.ammo.packSize);
        }
    }
}
