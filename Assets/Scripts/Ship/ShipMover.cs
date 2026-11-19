using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
public class ShipMover : MonoBehaviour
{
    [SerializeField] float stopDistance = 1f;
    [SerializeField, Range(1, 6)] int separationPasses = 3;

    static readonly List<ShipMover> all = new List<ShipMover>();

    struct Footprint
    {
        public Vector2 center, right, fwd; // XZ plane
        public float hx, hz;               // half extents along right / fwd
    }

    float speed = 5f;
    float turnSpeed = 60f;
    Vector3 target;
    bool hasTarget;
    Rigidbody rb;
    BoxCollider box;

    public bool HasTarget => hasTarget;
    public Vector3 Target => target;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        box = GetComponent<BoxCollider>();
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void OnEnable() => all.Add(this);
    void OnDisable() => all.Remove(this);

    public void Init(float speed, float turnSpeed)
    {
        this.speed = speed;
        this.turnSpeed = turnSpeed;
    }

    public void SetTarget(Vector3 point)
    {
        target = point;
        hasTarget = true;
    }

    public void Stop() => hasTarget = false;

    void FixedUpdate()
    {
        if (!hasTarget) return;

        Vector3 to = target - rb.position;
        to.y = 0f;
        float dist = to.magnitude;

        if (dist <= stopDistance)
        {
            hasTarget = false;
            return;
        }

        Vector3 dir = to / dist;
        Quaternion desired = Quaternion.LookRotation(dir, Vector3.up);
        Quaternion rot = Quaternion.RotateTowards(rb.rotation, desired, turnSpeed * Time.fixedDeltaTime);

        // Move forward only as fast as the hull is aligned with the target direction.
        float facing = Mathf.Clamp01(Vector3.Dot(rot * Vector3.forward, dir));
        float step = Mathf.Min(speed * facing * Time.fixedDeltaTime, dist);
        Vector3 pos = rb.position + rot * Vector3.forward * step;

        pos = SeparateFromShips(pos, rot);

        rb.MoveRotation(rot);
        rb.MovePosition(pos);
    }

    // Kinematic bodies don't collide with each other, so the candidate pose is pushed out
    // of every other ship's footprint (oriented rectangle test in XZ) before it is applied.
    Vector3 SeparateFromShips(Vector3 pos, Quaternion rot)
    {
        for (int pass = 0; pass < separationPasses; pass++)
        {
            bool moved = false;
            Footprint me = GetFootprint(pos, rot);

            foreach (ShipMover other in all)
            {
                if (other == this) continue;

                Footprint theirs = other.GetFootprint(other.rb.position, other.rb.rotation);
                if (TryGetPush(me, theirs, out Vector2 push))
                {
                    pos += new Vector3(push.x, 0f, push.y);
                    me = GetFootprint(pos, rot);
                    moved = true;
                }
            }

            if (!moved) break;
        }

        return pos;
    }

    Footprint GetFootprint(Vector3 pos, Quaternion rot)
    {
        Vector3 s = transform.lossyScale;
        Vector3 c = pos + rot * Vector3.Scale(box.center, s);
        Vector3 right = rot * Vector3.right;
        Vector3 fwd = rot * Vector3.forward;

        return new Footprint
        {
            center = new Vector2(c.x, c.z),
            right = new Vector2(right.x, right.z).normalized,
            fwd = new Vector2(fwd.x, fwd.z).normalized,
            hx = box.size.x * 0.5f * s.x,
            hz = box.size.z * 0.5f * s.z
        };
    }

    // Separating Axis Test for two oriented rectangles. 'push' moves A out of B.
    static bool TryGetPush(Footprint a, Footprint b, out Vector2 push)
    {
        push = Vector2.zero;
        float minOverlap = float.MaxValue;
        Vector2 d = a.center - b.center;

        for (int i = 0; i < 4; i++)
        {
            Vector2 axis = i == 0 ? a.right : i == 1 ? a.fwd : i == 2 ? b.right : b.fwd;

            float rA = a.hx * Mathf.Abs(Vector2.Dot(a.right, axis)) + a.hz * Mathf.Abs(Vector2.Dot(a.fwd, axis));
            float rB = b.hx * Mathf.Abs(Vector2.Dot(b.right, axis)) + b.hz * Mathf.Abs(Vector2.Dot(b.fwd, axis));
            float proj = Vector2.Dot(d, axis);
            float overlap = rA + rB - Mathf.Abs(proj);

            if (overlap <= 0f) return false; // separating axis found

            if (overlap < minOverlap)
            {
                minOverlap = overlap;
                push = axis * (proj >= 0f ? overlap : -overlap);
            }
        }

        return true;
    }
}
