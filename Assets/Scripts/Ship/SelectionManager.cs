using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[DefaultExecutionOrder(-100)]
public class SelectionManager : MonoBehaviour
{
    public static SelectionManager Instance { get; private set; }

    [Header("Input")]
    [SerializeField] LayerMask shipMask;
    [SerializeField] float maxRayDistance = 10000f;
    [SerializeField] float waterLevel = 0f;

    [Header("Ring")]
    [SerializeField] Material ringMaterial;
    [SerializeField, ColorUsage(true, true)] Color hoverColor = new Color(0f, 4f, 0.4f, 1f);
    [SerializeField, ColorUsage(true, true)] Color enemyHoverColor = new Color(4f, 0.3f, 0f, 1f);
    [SerializeField, ColorUsage(true, true)] Color selectedColor = new Color(0.2f, 1.5f, 5f, 1f);
    [SerializeField] float radiusPadding = 1.15f;
    [SerializeField] float ringHeightOffset = 0.3f;

    readonly List<Ship> selection = new List<Ship>();
    Ship hovered;

    public Ship Hovered => hovered ? hovered : null;

    public Ship Selected => selection.Count == 1 && selection[0] ? selection[0] : null;
    public IReadOnlyList<Ship> Selection => selection;

    public float WaterLevel => waterLevel;
    public Material RingMaterial => ringMaterial;
    public Color HoverColor => hoverColor;
    public Color EnemyHoverColor => enemyHoverColor;
    public Color SelectedColor => selectedColor;
    public float RadiusPadding => radiusPadding;
    public float RingHeightOffset => ringHeightOffset;

    public event Action<Ship> OnHoverChanged;
    public event Action<Ship> OnSelectionChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (shipMask.value == 0) shipMask = LayerMask.GetMask("Ship");
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Update()
    {

        if (selection.RemoveAll(s => !s) > 0) OnSelectionChanged?.Invoke(Selected);
        if ((object)hovered != null && !hovered) SetHovered(null);

        Ship under = null;
        Camera cam = Camera.main;
        bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        if (cam != null && !overUI)
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, shipMask, QueryTriggerInteraction.Collide))
            {
                under = hit.collider.GetComponentInParent<Ship>();
                if (under && !under.IsHoverable) under = null;
            }
        }

        SetHovered(under);
    }

    public bool IsSelected(Ship ship) => selection.Contains(ship);

    public void Select(Ship ship)
    {
        if (!ship) { ClearSelection(); return; }
        if (selection.Count == 1 && selection[0] == ship) return;

        selection.Clear();
        selection.Add(ship);
        OnSelectionChanged?.Invoke(ship);
    }

    public void SelectMany(IReadOnlyList<Ship> ships)
    {
        if (ships.Count == 0) { ClearSelection(); return; }

        selection.Clear();
        for (int i = 0; i < ships.Count; i++) selection.Add(ships[i]);
        OnSelectionChanged?.Invoke(Selected);
    }

    public void Deselect(Ship ship)
    {
        if (selection.Remove(ship)) OnSelectionChanged?.Invoke(Selected);
    }

    public void ClearSelection()
    {
        if (selection.Count == 0) return;

        selection.Clear();
        OnSelectionChanged?.Invoke(null);
    }

    void SetHovered(Ship ship)
    {
        if (hovered == ship) return;
        hovered = ship;
        OnHoverChanged?.Invoke(ship);
    }
}