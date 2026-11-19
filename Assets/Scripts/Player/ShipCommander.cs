using System;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class ShipCommander : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] ShipSpawner spawner;
    [SerializeField] Transform marker;
    [SerializeField] float maxDistance = 5000f;

    public Ship Selected { get; private set; }
    public event Action<Ship> OnSelectionChanged;

    void Awake()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) Debug.LogError("ShipCommander: No camera", this);
    }

    void Update()
    {
        if (Selected == null) AutoSelect();

        if (cam != null && WasClicked(out Vector2 screenPos) && !PointerOverUI())
            HandleClick(screenPos);

        UpdateMarker();
    }

    void HandleClick(Vector2 screenPos)
    {
        Ray ray = cam.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            Ship clicked = hit.collider.GetComponentInParent<Ship>();
            if (clicked != null)
            {
                Select(clicked);
                return;
            }
        }

        if (Selected == null) return;

        Plane plane = new Plane(Vector3.up, Selected.transform.position);
        if (plane.Raycast(ray, out float enter))
            Selected.Mover.SetTarget(ray.GetPoint(enter));
    }

    void AutoSelect()
    {
        if (spawner == null || spawner.Spawned == null) return;

        foreach (Ship s in spawner.Spawned)
        {
            if (s != null)
            {
                Select(s);
                return;
            }
        }
    }

    void Select(Ship ship)
    {
        if (ship == Selected) return;
        Selected = ship;
        OnSelectionChanged?.Invoke(ship);
    }

    void UpdateMarker()
    {
        if (marker == null) return;

        bool show = Selected != null && Selected.Mover.HasTarget;
        if (marker.gameObject.activeSelf != show) marker.gameObject.SetActive(show);
        if (show) marker.position = Selected.Mover.Target;
    }

    static bool PointerOverUI() =>
        EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

    static bool WasClicked(out Vector2 position)
    {
#if ENABLE_INPUT_SYSTEM
        Mouse mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            position = mouse.position.ReadValue();
            return true;
        }
        position = default;
        return false;
#else
        position = Input.mousePosition;
        return Input.GetMouseButtonDown(0);
#endif
    }
}
