using UnityEngine;

public class IKTargetMouseFollow : MonoBehaviour
{
    // ── Inspector ────────────────────────────────────────────────────────────
    [Header("References")]
    public Rigidbody2D targetRB;
    public Transform   baseTransform;

    [Header("Follow")]
    public float followSpeed = 30f;

    [Header("Distance Limits")]
    public float chainLength = 5f;
    public float minRange    = 1.5f;

    [Header("Angle Constraints")]
    public bool  useAngleLimits = true;
    [Range(-360, 360)] public float minAngle = -240f;
    [Range(-360, 360)] public float maxAngle =   68f;
    [Tooltip("Smoothing applied ONLY when snapping back from the deadzone.")]
    public float flipSmoothing = 0.15f;

    [Header("Virtual Cursor")]
    [Tooltip("Max world-units the virtual cursor travels per FixedUpdate step.")]
    public float virtualCursorSpeed = 15f;

    // ── Private ──────────────────────────────────────────────────────────────
    private float   _currentSmoothAngle;
    private float   _angleVelocity;
    private Vector2 _virtualCursor;
    private bool    _cursorInitialized = false;
    private bool    _active            = false;

    // ── Unity ────────────────────────────────────────────────────────────────
    void OnEnable()
    {
        GameManager.OnGameStart  += Activate;
        GameManager.OnGameResume += Activate;
        GameManager.OnGamePause  += Deactivate;
        GameManager.OnGameStop   += Deactivate;
    }

    void OnDisable()
    {
        GameManager.OnGameStart  -= Activate;
        GameManager.OnGameResume -= Activate;
        GameManager.OnGamePause  -= Deactivate;
        GameManager.OnGameStop   -= Deactivate;
    }

    void FixedUpdate()
    {
        if (!_active || targetRB == null || baseTransform == null) return;

        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 basePos    = baseTransform.position;

        // Initialise virtual cursor on first active frame.
        if (!_cursorInitialized)
        {
            _virtualCursor     = mouseWorld;
            _cursorInitialized = true;
        }

        // Walk virtual cursor toward real mouse (speed-limited to avoid teleporting).
        _virtualCursor = Vector2.MoveTowards(_virtualCursor, mouseWorld, virtualCursorSpeed * Time.fixedDeltaTime);

        // Clamp distance.
        Vector2 dir  = _virtualCursor - basePos;
        float   dist = dir.magnitude;
        float   clampedDist = Mathf.Clamp(dist, minRange, chainLength);
        Vector2 clampedDir  = dist > 0.001f ? dir.normalized : Vector2.right;

        // Clamp angle.
        if (useAngleLimits)
        {
            float virtualAngle  = Mathf.Atan2(clampedDir.y, clampedDir.x) * Mathf.Rad2Deg;
            float baseRot       = baseTransform.eulerAngles.z;
            float relAngle      = Mathf.DeltaAngle(baseRot, virtualAngle);

            float mid = (minAngle + maxAngle) / 2f;
            if      (relAngle < mid - 180) relAngle += 360;
            else if (relAngle > mid + 180) relAngle -= 360;

            bool  inDeadzone      = relAngle < minAngle || relAngle > maxAngle;
            float clampedRelAngle = Mathf.Clamp(relAngle, minAngle, maxAngle);

            if (inDeadzone)
                _currentSmoothAngle = Mathf.SmoothDampAngle(_currentSmoothAngle, clampedRelAngle, ref _angleVelocity, flipSmoothing);
            else
            {
                _currentSmoothAngle = clampedRelAngle;
                _angleVelocity      = 0f;
            }

            float finalRad = (baseRot + _currentSmoothAngle) * Mathf.Deg2Rad;
            clampedDir = new Vector2(Mathf.Cos(finalRad), Mathf.Sin(finalRad));
        }

        // Snap virtual cursor back onto valid surface so it doesn't drift.
        _virtualCursor = basePos + clampedDir * clampedDist;

        // Drive rigidbody.
        Vector2 target = basePos + clampedDir * clampedDist;
        targetRB.linearVelocity = (target - targetRB.position) * followSpeed;
    }

    // ── Helpers ──────────────────────────────────────────────────────────────
    void Activate()
    {
        _active            = true;
        _cursorInitialized = false; // re-sync cursor on next active frame
    }

    void Deactivate()
    {
        _active = false;
        if (targetRB != null) targetRB.linearVelocity = Vector2.zero;
    }

    // ── Gizmos ───────────────────────────────────────────────────────────────
    void OnDrawGizmos()
    {
        if (baseTransform == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(baseTransform.position, chainLength);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(baseTransform.position, minRange);

        float    baseRot = baseTransform.eulerAngles.z;
        Vector3  minVec  = Quaternion.Euler(0, 0, baseRot + minAngle) * Vector3.right;
        Vector3  maxVec  = Quaternion.Euler(0, 0, baseRot + maxAngle) * Vector3.right;
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(baseTransform.position, minVec * chainLength);
        Gizmos.DrawRay(baseTransform.position, maxVec * chainLength);

        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(_virtualCursor, 0.15f);
        }
    }
}
