using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class DefenseZone : MonoBehaviour
{
    [SerializeField] private Vector2 size = new Vector2(160f, 160f);
    [SerializeField, Min(1f)] private float triggerHeight = 200f;
    [SerializeField, Min(0f)] private float damagePerSecond = 20f;

    private BoxCollider box;

    private void Awake() => SyncCollider();

    private void OnValidate() => SyncCollider();

    private void SyncCollider()
    {
        if (!box) box = GetComponent<BoxCollider>();
        if (!box) return;

        box.isTrigger = true;
        box.center = Vector3.zero;
        box.size = new Vector3(size.x, triggerHeight, size.y);
    }

    private void OnTriggerStay(Collider other)
    {
        Ship ship = other.GetComponentInParent<Ship>();
        if (ship == null || ship.Team != Team.Player) return;

        ship.Health.TakeDamage(damagePerSecond * Time.fixedDeltaTime);
    }

    public Vector3 RandomPoint(float margin)
    {
        Vector3 c = transform.position;
        float hx = Mathf.Max(0f, size.x * 0.5f - margin);
        float hz = Mathf.Max(0f, size.y * 0.5f - margin);
        return new Vector3(c.x + Random.Range(-hx, hx), c.y, c.z + Random.Range(-hz, hz));
    }

    public Vector3 PushOut(Vector3 p)
    {
        Vector3 c = transform.position;
        float hx = size.x * 0.5f;
        float hz = size.y * 0.5f;
        float dx = p.x - c.x;
        float dz = p.z - c.z;

        if (Mathf.Abs(dx) >= hx || Mathf.Abs(dz) >= hz) return p;

        float toRight = hx - dx;
        float toLeft = hx + dx;
        float toFront = hz - dz;
        float toBack = hz + dz;
        float min = Mathf.Min(Mathf.Min(toRight, toLeft), Mathf.Min(toFront, toBack));

        if (min == toRight) p.x = c.x + hx;
        else if (min == toLeft) p.x = c.x - hx;
        else if (min == toFront) p.z = c.z + hz;
        else p.z = c.z - hz;

        return p;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(size.x, 1f, size.y));
    }
}
