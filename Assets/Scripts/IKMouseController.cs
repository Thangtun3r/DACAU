using UnityEngine;

public class IKTargetMouseFollow : MonoBehaviour
{
    public Rigidbody2D targetRB;        
    public Transform baseTransform;     
    public float followSpeed = 30f;     
    
    [Header("Distance Limits")]
    public float chainLength = 5f;      
    public float minRange = 1.5f;       

    [Header("Angle Constraints")]
    public bool useAngleLimits = true;
    [Range(-360, 360)] public float minAngle = -240f;
    [Range(-360, 360)] public float maxAngle = 68f;
    [Tooltip("Smoothing applied ONLY when snapping across the deadzone.")]
    public float flipSmoothing = 0.15f;

    [Header("Virtual Cursor")]
    [Tooltip("Max distance the virtual cursor can travel per FixedUpdate step.")]
    public float virtualCursorSpeed = 15f;

    private float _currentSmoothAngle;
    private float _angleVelocity;
    private Vector2 _virtualCursor; // world-space position
    private bool _virtualCursorInitialized = false;

    void FixedUpdate() 
    {
        if (targetRB == null || baseTransform == null) return;

        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 basePos = baseTransform.position;

        // --- Initialize virtual cursor on first frame ---
        if (!_virtualCursorInitialized)
        {
            _virtualCursor = mouseWorldPos;
            _virtualCursorInitialized = true;
        }

        // --- Move virtual cursor toward real mouse, but speed-limited ---
        // This prevents teleporting: it walks toward the real mouse each frame
        _virtualCursor = Vector2.MoveTowards(_virtualCursor, mouseWorldPos, virtualCursorSpeed * Time.fixedDeltaTime);

        // --- Now clamp virtual cursor to valid zone ---
        Vector2 dirToVirtual = _virtualCursor - basePos;
        float virtualDist = dirToVirtual.magnitude;

        // Clamp distance
        float clampedDist = Mathf.Clamp(virtualDist, minRange, chainLength);
        Vector2 clampedDir = virtualDist > 0.001f ? dirToVirtual.normalized : Vector2.right;

        // Clamp angle
        if (useAngleLimits)
        {
            float virtualAngle = Mathf.Atan2(clampedDir.y, clampedDir.x) * Mathf.Rad2Deg;
            float baseRot = baseTransform.eulerAngles.z;
            float relativeAngle = Mathf.DeltaAngle(baseRot, virtualAngle);

            float midPoint = (minAngle + maxAngle) / 2f;
            if (relativeAngle < midPoint - 180) relativeAngle += 360;
            else if (relativeAngle > midPoint + 180) relativeAngle -= 360;

            bool inDeadzone = (relativeAngle < minAngle || relativeAngle > maxAngle);
            float clampedRelAngle = Mathf.Clamp(relativeAngle, minAngle, maxAngle);

            if (inDeadzone)
                _currentSmoothAngle = Mathf.SmoothDampAngle(_currentSmoothAngle, clampedRelAngle, ref _angleVelocity, flipSmoothing);
            else
            {
                _currentSmoothAngle = clampedRelAngle;
                _angleVelocity = 0;
            }

            float finalAngle = (baseRot + _currentSmoothAngle) * Mathf.Deg2Rad;
            clampedDir = new Vector2(Mathf.Cos(finalAngle), Mathf.Sin(finalAngle));
        }

        // Snap virtual cursor back onto valid zone surface
        // so next frame it doesn't drift outside
        _virtualCursor = basePos + clampedDir * clampedDist;

        // --- Drive rigidbody toward virtual cursor ---
        Vector2 targetPosition = basePos + clampedDir * clampedDist;
        targetRB.linearVelocity = (targetPosition - targetRB.position) * followSpeed;
    }

    void OnDrawGizmos()
    {
        if (baseTransform == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(baseTransform.position, chainLength);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(baseTransform.position, minRange);

        float baseRot = baseTransform.eulerAngles.z;
        Gizmos.color = Color.cyan;
        Vector3 minV = Quaternion.Euler(0, 0, baseRot + minAngle) * Vector3.right;
        Vector3 maxV = Quaternion.Euler(0, 0, baseRot + maxAngle) * Vector3.right;
        Gizmos.DrawRay(baseTransform.position, minV * chainLength);
        Gizmos.DrawRay(baseTransform.position, maxV * chainLength);

        // Draw virtual cursor
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(_virtualCursor, 0.15f);
        }
    }
}