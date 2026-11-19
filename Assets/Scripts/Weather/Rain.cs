using UnityEngine;

public class FollowXZ : MonoBehaviour
{
    [SerializeField] private Transform target;

    private void LateUpdate()
    {
        if (target == null) return;
        Vector3 p = transform.position;
        p.x = target.position.x;
        p.z = target.position.z;
        transform.position = p;
    }
}