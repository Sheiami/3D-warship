using UnityEngine;

public static class ShipFactory
{
    public static Ship Spawn(GameObject shipBase, ShipData data, Vector3 position, Quaternion rotation, float scale = 1f, Team team = Team.Player)
    {
        GameObject root = Object.Instantiate(shipBase, position, Quaternion.identity);
        root.name = data.displayName;
        root.transform.localScale = Vector3.one * scale;

        GameObject visual = Object.Instantiate(data.visualPrefab, root.transform);
        visual.name = "Visual";

        foreach (Rigidbody body in visual.GetComponentsInChildren<Rigidbody>(true))
            Object.Destroy(body);
        foreach (Collider col in visual.GetComponentsInChildren<Collider>(true))
            Object.Destroy(col);

        FitCollider(root.GetComponent<BoxCollider>(), root.transform, visual.GetComponentsInChildren<Renderer>());

        root.transform.rotation = rotation;
        Physics.SyncTransforms();

        Ship ship = root.GetComponent<Ship>();
        ship.Init(data, team);
        return ship;
    }

    static void FitCollider(BoxCollider box, Transform root, Renderer[] renderers)
    {
        bool found = false;
        Bounds local = default;

        foreach (Renderer r in renderers)
        {
            if (r is ParticleSystemRenderer || r is TrailRenderer || r is LineRenderer)
                continue;

            Bounds wb = r.bounds;
            for (int i = 0; i < 8; i++)
            {
                Vector3 corner = wb.center + Vector3.Scale(wb.extents, new Vector3(
                    (i & 1) == 0 ? -1f : 1f,
                    (i & 2) == 0 ? -1f : 1f,
                    (i & 4) == 0 ? -1f : 1f));
                Vector3 p = root.InverseTransformPoint(corner);

                if (!found) { local = new Bounds(p, Vector3.zero); found = true; }
                else local.Encapsulate(p);
            }
        }

        if (!found)
        {
            Debug.LogWarning("Ship visual has no renderers; collider left as in the base prefab.", box);
            return;
        }

        box.center = local.center;
        box.size = local.size;
    }
}