using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(SpriteRenderer))]
public class ParallaxLayer : MonoBehaviour
{
    public enum FitMode { None, FitWidth, FitHeight, Cover, Contain }

    [SerializeField] private Camera targetCamera;

    [Header("Parallax (0=fijo a cámara, 1=mueve igual que el mundo)")]
    [Range(0f, 1f)] public float parallaxX = 0.2f;
    [Range(0f, 1f)] public float parallaxY = 0f;

    [Header("Ajuste a cámara")]
    [SerializeField] private FitMode fitMode = FitMode.Cover;
    public bool keepAspect = true;

    [Header("Alineación vertical")]
    public bool alignToCameraBottom = false;
    public float bottomOffset = 0f;

    [Header("Escala adicional")]
    [Min(0f)] public float scaleMultiplier = 1f;

    [Header("Loop horizontal")]
    public bool loopX = false;
    [Tooltip("Usar 3 segmentos (centro/izq/der).")]
    public bool threeSegments = true;

    [Header("Mantenimiento")]
    [Tooltip("Eliminar segmentos generados al deshabilitar o al salir de Play Mode.")]
    public bool cleanupOnDisable = true;

    private float anchorOffsetX;
    private float anchorOffsetY;

    private SpriteRenderer sr;
    private Transform leftSeg;
    private Transform rightSeg;
    private float segmentWidth;

    private const string LeftSuffix = " (L)";
    private const string RightSuffix = " (R)";

    private void OnEnable()
    {
        if (targetCamera == null) targetCamera = Camera.main;
        sr = GetComponent<SpriteRenderer>();
        
        FitAndAlign();
        // Solo crear segmentos en Play Mode o si no existen en Editor
        if (Application.isPlaying || (leftSeg == null && rightSeg == null))
        {
            AdoptOrCreateSegments();
        }
    }

    private void OnValidate()
    {
        // OnValidate solo debe ajustar, no crear objetos.
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        FitAndAlign();
    }

    private void LateUpdate()
    {
        if (targetCamera == null) return;

        var camPos = targetCamera.transform.position;
        var pos = transform.position;
        pos.x = camPos.x * parallaxX + anchorOffsetX;
        pos.y = camPos.y * parallaxY + anchorOffsetY;
        transform.position = pos;

        UpdateSegments();
        
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            if (transform.hasChanged)
            {
                FitAndAlign();
                transform.hasChanged = false;
            }
        }
#endif
    }

    private void OnDisable()
    {
        if (cleanupOnDisable || !Application.isPlaying)
            CleanupSegments(immediate: !Application.isPlaying);
    }

    private void OnDestroy()
    {
        CleanupSegments(immediate: !Application.isPlaying);
    }

    [ContextMenu("Ajustar y Anclar Fondo")]
    public void FitAndAlign()
    {
        FitToCamera();
        CacheAnchor();
    }

    private void FitToCamera()
    {
        if (sr == null || sr.sprite == null || targetCamera == null) return;

        float worldH = targetCamera.orthographicSize * 2f;
        float worldW = worldH * targetCamera.aspect;

        Vector2 spriteSize = sr.sprite.bounds.size;
        if (spriteSize.x == 0 || spriteSize.y == 0) return;

        float sx = worldW / spriteSize.x;
        float sy = worldH / spriteSize.y;

        Vector3 scale = transform.localScale;
        switch (fitMode)
        {
            case FitMode.None: break;
            case FitMode.FitWidth:  scale = keepAspect ? new Vector3(sx, sx, 1f) : new Vector3(sx, sy, 1f); break;
            case FitMode.FitHeight: scale = keepAspect ? new Vector3(sy, sy, 1f) : new Vector3(sx, sy, 1f); break;
            case FitMode.Cover:     { float s = Mathf.Max(sx, sy); scale = keepAspect ? new Vector3(s, s, 1f) : new Vector3(sx, sy, 1f); } break;
            case FitMode.Contain:   { float s = Mathf.Min(sx, sy); scale = keepAspect ? new Vector3(s, s, 1f) : new Vector3(sx, sy, 1f); } break;
        }
        
        transform.localScale = scale * Mathf.Max(0.0001f, scaleMultiplier);
        
        if (alignToCameraBottom) AlignBottomToCamera();

        segmentWidth = sr.sprite.bounds.size.x * transform.localScale.x;
    }

    private void AlignBottomToCamera()
    {
        if (sr == null || sr.sprite == null || targetCamera == null) return;

        float spriteWorldHeight = sr.sprite.bounds.size.y * transform.localScale.y;
        float halfH = spriteWorldHeight * 0.5f;
        float camBottom = targetCamera.transform.position.y - targetCamera.orthographicSize;

        var pos = transform.position;
        pos.y = camBottom + halfH + bottomOffset;
        transform.position = pos;
    }

    private void CacheAnchor()
    {
        if (targetCamera == null) return;
        anchorOffsetX = transform.position.x - targetCamera.transform.position.x * parallaxX;
        anchorOffsetY = transform.position.y - targetCamera.transform.position.y * parallaxY;
    }

    // --------- Segmentos (loop) ----------
    private void AdoptOrCreateSegments()
    {
        if (!loopX || !threeSegments)
        {
            CleanupSegments(immediate: !Application.isPlaying);
            return;
        }

        leftSeg = FindChildByName(name + LeftSuffix);
        rightSeg = FindChildByName(name + RightSuffix);

        if (leftSeg == null)  leftSeg  = FindSiblingByName(name + LeftSuffix);
        if (rightSeg == null) rightSeg = FindSiblingByName(name + RightSuffix);
        if (leftSeg != null && leftSeg.parent != transform)   leftSeg.SetParent(transform, worldPositionStays: true);
        if (rightSeg != null && rightSeg.parent != transform) rightSeg.SetParent(transform, worldPositionStays: true);

        if (leftSeg == null)  leftSeg  = CreateSegment(LeftSuffix);
        if (rightSeg == null) rightSeg = CreateSegment(RightSuffix);

        RemoveDuplicateChildren(name + LeftSuffix, keep: leftSeg);
        RemoveDuplicateChildren(name + RightSuffix, keep: rightSeg);

        PositionSegmentsLocal();
        SyncChildRenderers();
    }

    private Transform CreateSegment(string suffix)
    {
        var go = new GameObject(name + suffix);
        var segSr = go.AddComponent<SpriteRenderer>();
        CopyRenderer(sr, segSr);
        go.transform.SetParent(transform, worldPositionStays: true);
        go.transform.localScale = Vector3.one;
        return go.transform;
    }

    private void CopyRenderer(SpriteRenderer src, SpriteRenderer dst)
    {
        dst.sprite = src.sprite;
        dst.color = src.color;
        dst.flipX = src.flipX; dst.flipY = src.flipY;
        dst.sharedMaterial = src.sharedMaterial;
        dst.sortingLayerID = src.sortingLayerID;
        dst.sortingOrder = src.sortingOrder;
        dst.drawMode = src.drawMode;
    }

    private void PositionSegmentsLocal()
    {
        if (segmentWidth <= 0f) return;
        if (leftSeg != null)  leftSeg.localPosition  = Vector3.left  * segmentWidth;
        if (rightSeg != null) rightSeg.localPosition = Vector3.right * segmentWidth;
    }

    private void UpdateSegments()
    {
        if (!loopX || !threeSegments || segmentWidth <= 0f) return;

        SyncChildRenderers();
        PositionSegmentsLocal();

        float camX = targetCamera.transform.position.x;
        float centerX = transform.position.x;

        if (camX - centerX > segmentWidth)
        {
            transform.position += Vector3.right * segmentWidth;
            CacheAnchor();
        }
        else if (centerX - camX > segmentWidth)
        {
            transform.position += Vector3.left * segmentWidth;
            CacheAnchor();
        }
    }

    private void SyncChildRenderers()
    {
        var lsr = leftSeg ? leftSeg.GetComponent<SpriteRenderer>() : null;
        var rsr = rightSeg ? rightSeg.GetComponent<SpriteRenderer>() : null;
        if (lsr != null) CopyRenderer(sr, lsr);
        if (rsr != null) CopyRenderer(sr, rsr);
    }

    private void CleanupSegments(bool immediate)
    {
        DestroyChildByName(name + LeftSuffix, immediate);
        DestroyChildByName(name + RightSuffix, immediate);
        leftSeg = rightSeg = null;

        var parent = transform.parent;
        if (parent != null)
        {
            DestroySiblingByName(parent, name + LeftSuffix, immediate);
            DestroySiblingByName(parent, name + RightSuffix, immediate);
        }
    }

    private Transform FindChildByName(string childName)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var t = transform.GetChild(i);
            if (t.name == childName) return t;
        }
        return null;
    }

    private Transform FindSiblingByName(string siblingName)
    {
        var parent = transform.parent;
        if (parent == null) return null;
        for (int i = 0; i < parent.childCount; i++)
        {
            var t = parent.GetChild(i);
            if (t == transform) continue;
            if (t.name == siblingName) return t;
        }
        return null;
    }

    private void RemoveDuplicateChildren(string childName, Transform keep)
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var t = transform.GetChild(i);
            if (t == keep) continue;
            if (t.name == childName)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying) DestroyImmediate(t.gameObject);
                else Destroy(t.gameObject);
#else
                Destroy(t.gameObject);
#endif
            }
        }
    }

    private void DestroyChildByName(string childName, bool immediate)
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var t = transform.GetChild(i);
            if (t.name == childName)
            {
                if (immediate) DestroyImmediate(t.gameObject);
                else Destroy(t.gameObject);
            }
        }
    }

    private void DestroySiblingByName(Transform parent, string objName, bool immediate)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            var t = parent.GetChild(i);
            if (t == transform) continue;
            if (t.name == objName)
            {
                if (immediate) DestroyImmediate(t.gameObject);
                else Destroy(t.gameObject);
            }
        }
    }
}