using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Ship))]
public class ShipHighlight : MonoBehaviour
{
    enum State { None, Hover, Selected }

    static readonly int[] EmissiveProps = { Shader.PropertyToID("_EmissiveColor"), Shader.PropertyToID("_EmissionColor") };
    static readonly int[] BaseProps = { Shader.PropertyToID("_BaseColor"), Shader.PropertyToID("_UnlitColor"), Shader.PropertyToID("_Color") };
    static Mesh ringMesh;

    Ship ship;
    BoxCollider box;
    SelectionManager sm;
    MeshRenderer ringRenderer;
    Transform ring;
    Material material;
    bool warned;

    void Awake()
    {
        ship = GetComponent<Ship>();
        box = GetComponent<BoxCollider>();
    }

    void OnEnable()
    {
        sm = SelectionManager.Instance;
        if (sm == null) return;
        sm.OnHoverChanged += OnChanged;
        sm.OnSelectionChanged += OnChanged;
        Refresh();
    }

    void OnDisable()
    {
        if (sm != null)
        {
            sm.OnHoverChanged -= OnChanged;
            sm.OnSelectionChanged -= OnChanged;
        }
        if (ringRenderer != null) ringRenderer.enabled = false;
    }

    void OnDestroy()
    {
        if (ring) Destroy(ring.gameObject);
        if (material) Destroy(material);
    }

    void OnChanged(Ship _) => Refresh();

    void LateUpdate()
    {
        if (ringRenderer == null || !ringRenderer.enabled || sm == null) return;


        Vector3 c = transform.position + transform.rotation * Vector3.Scale(box.center, transform.lossyScale);
        c.y = sm.WaterLevel + sm.RingHeightOffset;
        ring.position = c;
    }

    void Refresh()
    {
        State s = State.None;
        if (sm.IsSelected(ship)) s = State.Selected;
        else if (sm.Hovered == ship) s = State.Hover;

        if (s == State.None)
        {
            if (ringRenderer != null) ringRenderer.enabled = false;
            return;
        }

        if (!EnsureRing()) return;

        Vector3 sc = transform.lossyScale;
        float radius = Mathf.Max(box.size.x * sc.x, box.size.z * sc.z) * 0.5f * sm.RadiusPadding;
        ring.localScale = new Vector3(radius, 1f, radius);

        Color c = s == State.Selected ? sm.SelectedColor
                : ship.Team == Team.Enemy ? sm.EnemyHoverColor : sm.HoverColor;
        ApplyColor(c);
        ringRenderer.enabled = true;
    }

    bool EnsureRing()
    {
        if (ring) return true;
        if (!sm.RingMaterial)
        {
            Debug.LogError("SelectionManager: ringMaterial не призначено", sm);
            return false;
        }

        var go = new GameObject(name + "_SelectionRing");
        go.AddComponent<MeshFilter>().sharedMesh = GetRingMesh();

        ringRenderer = go.AddComponent<MeshRenderer>();
        ringRenderer.shadowCastingMode = ShadowCastingMode.Off;
        ringRenderer.receiveShadows = false;
        material = new Material(sm.RingMaterial);
        ringRenderer.sharedMaterial = material;
        ringRenderer.enabled = false;
        ring = go.transform;
        return true;
    }

    void ApplyColor(Color c)
    {
        float m = Mathf.Max(c.r, c.g, c.b, 0.0001f);
        Color baseC = new Color(c.r / m, c.g / m, c.b / m, 1f);
        bool any = false;

        foreach (int id in EmissiveProps)
            if (material.HasProperty(id)) { material.SetColor(id, c); any = true; }
        foreach (int id in BaseProps)
            if (material.HasProperty(id)) material.SetColor(id, baseC);

        if (!any && !warned)
        {
            Debug.LogWarning("ShipHighlight: у матеріалі немає _EmissiveColor/_EmissionColor, колір не змінюється", this);
            warned = true;
        }
    }


    static Mesh GetRingMesh()
    {
        if (ringMesh) return ringMesh;

        const int seg = 96;
        const float inner = 0.93f;
        var v = new Vector3[seg * 2];
        var n = new Vector3[seg * 2];
        var uv = new Vector2[seg * 2];
        var t = new int[seg * 6];

        for (int i = 0; i < seg; i++)
        {
            float a = i * Mathf.PI * 2f / seg;
            float c = Mathf.Cos(a), s = Mathf.Sin(a);

            v[i * 2] = new Vector3(c, 0f, s);
            v[i * 2 + 1] = new Vector3(c * inner, 0f, s * inner);
            n[i * 2] = n[i * 2 + 1] = Vector3.up;
            uv[i * 2] = new Vector2(0.5f + 0.5f * c, 0.5f + 0.5f * s);
            uv[i * 2 + 1] = new Vector2(0.5f + 0.5f * c * inner, 0.5f + 0.5f * s * inner);

            int next = (i + 1) % seg;
            int o0 = i * 2, i0 = i * 2 + 1, o1 = next * 2, i1 = next * 2 + 1;
            int k = i * 6;
            t[k] = o0; t[k + 1] = i0; t[k + 2] = o1;
            t[k + 3] = o1; t[k + 4] = i0; t[k + 5] = i1;
        }

        ringMesh = new Mesh { name = "SelectionRing" };
        ringMesh.vertices = v;
        ringMesh.normals = n;
        ringMesh.uv = uv;
        ringMesh.triangles = t;
        ringMesh.RecalculateBounds();
        return ringMesh;
    }
}