using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class BottomAnchoredZoomOut : MonoBehaviour
{
    [Header("References")]
    public CinemachineVirtualCamera virtualCamera;
    public Transform anchor;
    public Transform trackedObject;

    [Header("Zoom Settings")]
    public float minOrthoSize = 10f;
    public float maxOrthoSize = 60f;
    public float zoomSpeed = 5f;

    [Header("Distance Thresholds")]
    [Tooltip("Object must be farther than this before any zoom starts")]
    public float zoomStartDistance = 5f;
    [Tooltip("At this distance, zoom reaches maximum")]
    public float zoomFullDistance = 25f;

    void Awake()
    {
        if (virtualCamera == null)
            virtualCamera = GetComponent<CinemachineVirtualCamera>();

        virtualCamera.Follow = null;
        virtualCamera.LookAt = null;

        SetOrthoSize(minOrthoSize);
    }

    void Update()
    {
        if (anchor == null || trackedObject == null) return;

        float targetSize = minOrthoSize; // default: no zoom

        // Only zoom if object is active AND outside the start threshold
        bool objectActive  = trackedObject.gameObject.activeInHierarchy;
        float distance     = Vector3.Distance(anchor.position, trackedObject.position);
        bool outOfBounds   = distance > zoomStartDistance;

        if (objectActive && outOfBounds)
        {
            float t  = Mathf.InverseLerp(zoomStartDistance, zoomFullDistance, distance);
            targetSize = Mathf.Lerp(minOrthoSize, maxOrthoSize, t);
        }

        // Smooth zoom
        float newSize = Mathf.MoveTowards(
            virtualCamera.m_Lens.OrthographicSize,
            targetSize,
            zoomSpeed * Time.deltaTime);

        SetOrthoSize(newSize);

        // Pin anchor to bottom of frame
        Vector3 camPos = virtualCamera.transform.position;
        camPos.x       = anchor.position.x;
        camPos.y       = anchor.position.y + newSize;
        virtualCamera.transform.position = camPos;
    }

    private void SetOrthoSize(float size)
    {
        var lens = virtualCamera.m_Lens;
        lens.OrthographicSize = size;
        virtualCamera.m_Lens = lens;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (anchor == null || trackedObject == null) return;

        // Yellow = zoom start threshold
        Gizmos.color = Color.yellow;
        DrawCircle(anchor.position, zoomStartDistance);

        // Red = full zoom threshold
        Gizmos.color = Color.red;
        DrawCircle(anchor.position, zoomFullDistance);

        // Line between anchor and object
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(anchor.position, trackedObject.position);
    }

    void DrawCircle(Vector3 center, float radius)
    {
        int   segments  = 32;
        float angleStep = 360f / segments;
        Vector3 prev    = center + new Vector3(radius, 0, 0);
        for (int i = 1; i <= segments; i++)
        {
            float   angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 next  = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }
#endif
}