using UnityEngine;

public class SlidingPower : MonoBehaviour
{
    // ── Inspector ────────────────────────────────────────────────────────────
    [Header("References")]
    public Transform baseTransform;
    public Transform slidingObject;
    public Transform virtualCursorMarker; // visual only

    [Header("Virtual Cursor (Visual)")]
    public float cursorDeadzoneRadius      = 2f;
    public float virtualCursorSmoothSpeed  = 20f;

    [Header("Mechanical Constraints")]
    public float chainLength        = 5f;
    public float maxExtendDistance  = 3f;
    public float slidingMultiplier  = 1f;
    public float slideSpeed         = 15f;
    public bool  useAngleLimits     = true;
    public float minAngle           = -240f;
    public float maxAngle           =   68f;

    [Header("Micro-Adjustment (Momentum)")]
    public float microAdjustSpeed     = 2f;
    public float microAdjustMaxOffset = 1.5f;
    public float microAdjustDecaySpeed = 4f;

    // ── Private ──────────────────────────────────────────────────────────────
    private Vector2 _virtualCursorPos;
    private float   _microAdjustOffset = 0f;
    private bool    _active            = false;

    // ── Unity ────────────────────────────────────────────────────────────────
    void Start()
    {
        if (baseTransform != null) _virtualCursorPos = baseTransform.position;
    }

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
        if (!_active || baseTransform == null || slidingObject == null) return;

        UpdateVirtualCursor();
        HandleMicroAdjustment();
        ApplySlidingObjectMovement();
    }

    // ── Private helpers ──────────────────────────────────────────────────────
    void UpdateVirtualCursor()
    {
        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float   mouseDist  = Vector2.Distance(mouseWorld, baseTransform.position);

        if (mouseDist > cursorDeadzoneRadius)
            _virtualCursorPos = Vector2.Lerp(_virtualCursorPos, mouseWorld, Time.fixedDeltaTime * virtualCursorSmoothSpeed);

        if (virtualCursorMarker != null)
            virtualCursorMarker.position = _virtualCursorPos;
    }

    void HandleMicroAdjustment()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");

        if (Mathf.Abs(horizontal) > 0.01f)
            _microAdjustOffset += horizontal * microAdjustSpeed * Time.fixedDeltaTime;
        else
            _microAdjustOffset = Mathf.MoveTowards(_microAdjustOffset, 0f, microAdjustDecaySpeed * Time.fixedDeltaTime);

        _microAdjustOffset = Mathf.Clamp(_microAdjustOffset, -microAdjustMaxOffset, microAdjustMaxOffset);
    }

    void ApplySlidingObjectMovement()
    {
        Vector2 dirToVirtual  = _virtualCursorPos - (Vector2)baseTransform.position;
        float   extensionFactor = 0f;

        if (IsWithinMechanicalLimits(dirToVirtual))
            extensionFactor = CalculateExtensionFactor(dirToVirtual.magnitude);

        float   baseSlideX = Mathf.Sign(dirToVirtual.x) * extensionFactor * slidingMultiplier;
        Vector3 targetPos  = baseTransform.position + new Vector3(baseSlideX + _microAdjustOffset, 0f, 0f);

        slidingObject.position = Vector3.Lerp(slidingObject.position, targetPos, Time.fixedDeltaTime * slideSpeed);
    }

    bool IsWithinMechanicalLimits(Vector2 dir)
    {
        if (!useAngleLimits) return true;

        float currentAngle  = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        float relativeAngle = Mathf.DeltaAngle(baseTransform.eulerAngles.z, currentAngle);

        float mid = (minAngle + maxAngle) / 2f;
        if      (relativeAngle < mid - 180) relativeAngle += 360;
        else if (relativeAngle > mid + 180) relativeAngle -= 360;

        return relativeAngle >= minAngle && relativeAngle <= maxAngle;
    }

    float CalculateExtensionFactor(float dist)
    {
        float extension = Mathf.Max(0f, dist - chainLength);
        return Mathf.Clamp01(extension / maxExtendDistance);
    }

    void Activate()
    {
        _active = true;
        // Re-seed virtual cursor to current base position to avoid a stale jump.
        if (baseTransform != null) _virtualCursorPos = baseTransform.position;
    }

    void Deactivate()
    {
        _active            = false;
        _microAdjustOffset = 0f;

        // Snap sliding object back to base so it doesn't hang mid-air.
        if (slidingObject != null && baseTransform != null)
            slidingObject.position = baseTransform.position;
    }

    // ── Gizmos ───────────────────────────────────────────────────────────────
    void OnDrawGizmos()
    {
        if (baseTransform == null) return;

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(baseTransform.position, cursorDeadzoneRadius);

        if (useAngleLimits)
        {
            float   baseRot = baseTransform.eulerAngles.z;
            Vector3 minVec  = Quaternion.Euler(0, 0, baseRot + minAngle) * Vector3.right;
            Vector3 maxVec  = Quaternion.Euler(0, 0, baseRot + maxAngle) * Vector3.right;
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(baseTransform.position, minVec * chainLength);
            Gizmos.DrawRay(baseTransform.position, maxVec * chainLength);
        }
    }
}
