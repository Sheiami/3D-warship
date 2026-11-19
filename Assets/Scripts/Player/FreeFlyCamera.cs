using UnityEngine;

public class FreeFlyCamera : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 25f;
    [SerializeField] private float boostMultiplier = 3f;
    [SerializeField] private float lookSensitivity = 2f;
    [SerializeField] private float minHeight = 3f;
    [SerializeField] private float maxHeight = 50f;
    [SerializeField] private DefenseZone zone;
    [SerializeField] private MapBounds bounds;

    private float yaw;
    private float pitch;

    private void Start()
    {
        Vector3 e = transform.eulerAngles;
        yaw = e.y;
        pitch = Mathf.DeltaAngle(0f, e.x);
    }

    private void Update()
    {
        if (PauseMenu.IsPaused) return;

        if (Input.GetMouseButtonDown(1)) Cursor.lockState = CursorLockMode.Locked;
        if (Input.GetMouseButtonUp(1)) Cursor.lockState = CursorLockMode.None;

        if (Input.GetMouseButton(1))
        {
            yaw += Input.GetAxis("Mouse X") * lookSensitivity;
            pitch -= Input.GetAxis("Mouse Y") * lookSensitivity;
            pitch = Mathf.Clamp(pitch, -89f, 89f);
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        moveSpeed = Mathf.Clamp(moveSpeed * (1f + Input.mouseScrollDelta.y * 0.1f), 2f, 200f);

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        float y = (Input.GetKey(KeyCode.E) ? 1f : 0f) - (Input.GetKey(KeyCode.Q) ? 1f : 0f);

        Vector3 dir = transform.right * x + transform.forward * z + Vector3.up * y;
        if (dir.sqrMagnitude > 1f) dir.Normalize();

        float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? boostMultiplier : 1f);
        transform.position += dir * (speed * Time.deltaTime);

        Vector3 p = transform.position;
        p.y = Mathf.Clamp(p.y, minHeight, maxHeight);
        transform.position = p;

        if (zone != null) transform.position = zone.PushOut(transform.position);
        if (bounds != null) transform.position = bounds.Clamp(transform.position);
    }
}