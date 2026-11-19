using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CommandController : MonoBehaviour
{
    [SerializeField] GameObject moveMarkerPrefab;
    [SerializeField] float maxRayDistance = 10000f;

    [Header("Рамка вибору")]
    [SerializeField, Min(1f)] float dragThreshold = 6f;
    [SerializeField] Color boxFill = new Color(0.2f, 0.5f, 1f, 0.25f);
    [SerializeField] Color boxBorder = new Color(0.4f, 0.7f, 1f, 0.9f);

    readonly List<Ship> inBox = new List<Ship>();
    Vector2 dragStart;
    bool dragging;

    void Update()
    {
        if (Time.timeScale <= 0f)
        {
            dragging = false;
            return;
        }

        SelectionManager sm = SelectionManager.Instance;
        if (sm == null) return;

        if (dragging)
        {
            if (Input.GetMouseButtonUp(0)) FinishBoxSelect(sm);
            return;
        }

        if (!Input.GetMouseButtonDown(0)) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (ctrl)
        {
            dragStart = Input.mousePosition;
            dragging = true;
        }
        else if (shift) OnSelectClick(sm);
        else OnCommandClick(sm);
    }

    void OnGUI()
    {
        if (!dragging || Event.current.type != EventType.Repaint) return;

        Vector2 end = Input.mousePosition;
        if ((end - dragStart).sqrMagnitude < dragThreshold * dragThreshold) return;

        var rect = new Rect(
            Mathf.Min(dragStart.x, end.x),
            Screen.height - Mathf.Max(dragStart.y, end.y),
            Mathf.Abs(end.x - dragStart.x),
            Mathf.Abs(end.y - dragStart.y));

        const float border = 2f;
        Texture2D tex = Texture2D.whiteTexture;

        GUI.color = boxFill;
        GUI.DrawTexture(rect, tex);

        GUI.color = boxBorder;
        GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, border), tex);
        GUI.DrawTexture(new Rect(rect.x, rect.yMax - border, rect.width, border), tex);
        GUI.DrawTexture(new Rect(rect.x, rect.y, border, rect.height), tex);
        GUI.DrawTexture(new Rect(rect.xMax - border, rect.y, border, rect.height), tex);

        GUI.color = Color.white;
    }

    void FinishBoxSelect(SelectionManager sm)
    {
        dragging = false;

        Vector2 end = Input.mousePosition;
        if ((end - dragStart).sqrMagnitude < dragThreshold * dragThreshold)
        {
            OnSelectClick(sm);
            return;
        }

        Camera cam = Camera.main;
        if (cam == null) return;

        Rect rect = Rect.MinMaxRect(
            Mathf.Min(dragStart.x, end.x), Mathf.Min(dragStart.y, end.y),
            Mathf.Max(dragStart.x, end.x), Mathf.Max(dragStart.y, end.y));

        inBox.Clear();
        var all = Ship.All;

        for (int i = 0; i < all.Count; i++)
        {
            Ship s = all[i];
            if (s == null || s.Team != Team.Player || s.Health.IsDead) continue;

            BoxCollider box = s.GetComponent<BoxCollider>();
            Vector3 screen = cam.WorldToScreenPoint(s.transform.TransformPoint(box.center));
            if (screen.z > 0f && rect.Contains(screen)) inBox.Add(s);
        }

        sm.SelectMany(inBox);
    }

    void OnSelectClick(SelectionManager sm)
    {
        Ship hovered = sm.Hovered;
        if (hovered)
        {
            if (hovered.Team == Team.Player) sm.Select(hovered);
        }
        else
        {
            sm.ClearSelection();
        }
    }

    void OnCommandClick(SelectionManager sm)
    {
        IReadOnlyList<Ship> group = sm.Selection;
        if (group.Count == 0) return;

        Ship hovered = sm.Hovered;
        if (hovered)
        {
            for (int i = 0; i < group.Count; i++)
            {
                if (hovered.Team != group[i].Team) group[i].Combat.SetTarget(hovered);
            }
            return;
        }

        Camera cam = Camera.main;
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        var water = new Plane(Vector3.up, new Vector3(0f, sm.WaterLevel, 0f));
        if (!water.Raycast(ray, out float enter) || enter > maxRayDistance) return;

        Vector3 point = ray.GetPoint(enter);

        Vector3 center = Vector3.zero;
        for (int i = 0; i < group.Count; i++) center += group[i].transform.position;
        center /= group.Count;

        for (int i = 0; i < group.Count; i++)
        {
            Vector3 offset = group[i].transform.position - center;
            offset.y = 0f;
            group[i].OrderMove(point + offset, moveMarkerPrefab);
        }
    }
}