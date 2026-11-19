using UnityEngine;

public class MapBounds : MonoBehaviour
{
    [SerializeField] private Vector2 size = new Vector2(1000f, 1000f);

    public Vector3 Clamp(Vector3 p)
    {
        Vector3 c = transform.position;
        float hx = size.x * 0.5f;
        float hz = size.y * 0.5f;
        p.x = Mathf.Clamp(p.x, c.x - hx, c.x + hx);
        p.z = Mathf.Clamp(p.z, c.z - hz, c.z + hz);
        return p;
    }

    public Vector3 RandomPoint(float margin)
    {
        Vector3 c = transform.position;
        float hx = Mathf.Max(0f, size.x * 0.5f - margin);
        float hz = Mathf.Max(0f, size.y * 0.5f - margin);
        return new Vector3(c.x + Random.Range(-hx, hx), c.y, c.z + Random.Range(-hz, hz));
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(size.x, 1f, size.y));
    }
}
