using UnityEngine;

/// <summary>
/// Runtime debug visualizer for cursor ground ray hit.
/// Shows:
///  - 3D world marker at the hit point,
///  - a line (ray) from camera,
///  - a small UI dot overlay in the Game view.
/// Toggle with F1.
/// </summary>
public class DebugAimVisualizer : MonoBehaviour
{
    [Header("Ray Settings")]
    [SerializeField] private Camera cam;                 // If null, will use Camera.main
    [SerializeField] private LayerMask groundMask;       // Layers to raycast against
    [SerializeField] private float rayDistance = 2000f;

    [Header("Toggles (can switch at runtime)")]
    [SerializeField] private bool enabledAtStart = true;
    [SerializeField] private bool drawWorldMarker = true;
    [SerializeField] private bool drawRayLine = true;
    [SerializeField] private bool drawScreenDot = true;

    [Header("World Marker")]
    [SerializeField] private float markerSize = 0.2f;
    [SerializeField] private Color markerColor = new Color(1f, 0.3f, 0.1f, 0.9f); // orange-red
    [SerializeField] private bool billboard = true;      // face camera
    [Tooltip("Optional custom mesh for marker; if null, a built-in quad or sphere is used.")]
    [SerializeField] private Mesh customMarkerMesh;
    [Tooltip("Optional custom material; if null, a simple unlit color material is created at runtime.")]
    [SerializeField] private Material customMarkerMaterial;
    [Tooltip("Use sphere instead of quad if no custom mesh provided.")]
    [SerializeField] private bool useSphereIfNoMesh = false;

    [Header("Ray Line")]
    [SerializeField] private Color rayColor = new Color(0.2f, 0.9f, 1f, 0.9f); // cyan
    [SerializeField] private float rayWidth = 0.02f;

    [Header("Screen Dot")]
    [SerializeField] private float screenDotSize = 6f;
    [SerializeField] private Color screenDotColor = new Color(1f, 0.95f, 0.2f, 0.95f); // yellow

    // runtime
    private bool _enabled;
    private Vector3 _hitPoint;
    private bool _hasHit;
    private LineRenderer _lr;
    private Material _markerMat;
    private Mesh _markerMesh;
    private Texture2D _dotTex; // tiny white circle

    private static readonly int _colorId = Shader.PropertyToID("_BaseColor");

    private void Awake()
    {
        _enabled = enabledAtStart;
        if (cam == null) cam = Camera.main;

        // Prepare line renderer if needed
        var lrGo = new GameObject("DebugAim_LineRenderer");
        lrGo.transform.SetParent(transform, false);
        _lr = lrGo.AddComponent<LineRenderer>();
        _lr.enabled = false;
        _lr.positionCount = 2;
        _lr.useWorldSpace = true;
        _lr.startWidth = _lr.endWidth = rayWidth;
        _lr.numCapVertices = 4;
        _lr.numCornerVertices = 2;
        _lr.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        _lr.material.SetColor(_colorId, rayColor);

        // Prepare marker material
        if (customMarkerMaterial != null)
        {
            _markerMat = customMarkerMaterial;
        }
        else
        {
            _markerMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            _markerMat.SetColor(_colorId, markerColor);
        }

        // Prepare marker mesh
        if (customMarkerMesh != null)
        {
            _markerMesh = customMarkerMesh;
        }
        else
        {
            _markerMesh = useSphereIfNoMesh ? CreateSphereMesh() : CreateQuadMesh();
        }

        // Prepare screen dot texture
        _dotTex = BuildDotTexture(16); // small round dot
    }

    private void Update()
    {
        // Toggle on/off
        if (Input.GetKeyDown(KeyCode.F1))
            _enabled = !_enabled;

        if (!_enabled || cam == null)
        {
            if (_lr != null) _lr.enabled = false;
            return;
        }

        // Raycast from mouse
        var ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit, rayDistance, groundMask))
        {
            _hasHit = true;
            _hitPoint = hit.point;

            // Line
            if (drawRayLine && _lr != null)
            {
                _lr.enabled = true;
                _lr.startColor = _lr.endColor = rayColor;
                _lr.SetPosition(0, ray.origin);
                _lr.SetPosition(1, hit.point);
            }
            else if (_lr != null) _lr.enabled = false;
        }
        else
        {
            _hasHit = false;
            if (_lr != null) _lr.enabled = false;
        }
    }

    private void LateUpdate()
    {
        if (!_enabled || !_hasHit) return;

        // Draw world marker using Graphics.DrawMesh for Game view
        if (drawWorldMarker && _markerMesh != null && _markerMat != null)
        {
            var camFwd = cam != null ? cam.transform.forward : Vector3.forward;
            Quaternion rot = billboard ? Quaternion.LookRotation(camFwd, Vector3.up)
                                       : Quaternion.identity;

            var matrix = Matrix4x4.TRS(_hitPoint, rot, Vector3.one * markerSize);

            // Set color each frame if using our runtime unlit
            if (customMarkerMaterial == null)
                _markerMat.SetColor(_colorId, markerColor);

            Graphics.DrawMesh(_markerMesh, matrix, _markerMat, 0);
        }
    }

    private void OnGUI()
    {
        if (!_enabled || !drawScreenDot || !_hasHit || cam == null || _dotTex == null) return;

        Vector3 sp = cam.WorldToScreenPoint(_hitPoint);
        if (sp.z < 0) return; // behind camera

        // GUI uses (0,0) at top-left
        float x = sp.x - screenDotSize * 0.5f;
        float y = Screen.height - sp.y - screenDotSize * 0.5f;

        var prevColor = GUI.color;
        GUI.color = screenDotColor;
        GUI.DrawTexture(new Rect(x, y, screenDotSize, screenDotSize), _dotTex, ScaleMode.StretchToFill, true);
        GUI.color = prevColor;
    }

    // ---------- Helpers ----------

    private static Mesh CreateQuadMesh()
    {
        var m = new Mesh();
        m.name = "DebugQuad";
        m.vertices = new[]
        {
            new Vector3(-0.5f, -0.5f, 0f),
            new Vector3( 0.5f, -0.5f, 0f),
            new Vector3(-0.5f,  0.5f, 0f),
            new Vector3( 0.5f,  0.5f, 0f)
        };
        m.uv = new[]
        {
            new Vector2(0,0), new Vector2(1,0),
            new Vector2(0,1), new Vector2(1,1)
        };
        m.triangles = new[] { 0, 2, 1, 2, 3, 1 };
        m.RecalculateNormals();
        return m;
    }

    // Lightweight sphere (ico-like). For real projects consider a shared mesh asset.
    private static Mesh CreateSphereMesh()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        var mesh = go.GetComponent<MeshFilter>().sharedMesh;
        GameObject.DestroyImmediate(go);
        return mesh;
    }

    private static Texture2D BuildDotTexture(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.ARGB32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        var pixels = new Color[size * size];
        float r = size * 0.5f;
        float cx = r - 0.5f, cy = r - 0.5f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = x - cx, dy = y - cy;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                float a = Mathf.Clamp01(1f - (d - (r - 1f)));
                a = Mathf.Pow(Mathf.Clamp01(a), 2f); // softer edge
                pixels[y * size + x] = new Color(1, 1, 1, a);
            }
        tex.SetPixels(pixels);
        tex.Apply(false, true);
        return tex;
    }

    // Public API to integrate with your existing systems
    public void SetCamera(Camera c) => cam = c;
    public void SetGroundMask(LayerMask m) => groundMask = m;
}
