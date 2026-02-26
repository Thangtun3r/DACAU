using UnityEngine;

public class SlidingPower : MonoBehaviour
{
    [Header("References")]
    public Transform baseTransform;
    public Transform slidingObject;
    public Transform virtualCursor;

    [Header("Virtual Cursor (Visual Only)")]
    public float cursorDeadzoneRadius = 2f;
    public float virtualCursorSmoothSpeed = 20f;
    private Vector2 _virtualCursorPos;

    [Header("Mechanical Constraints (Applied to Object)")]
    public float chainLength = 5f;
    public float maxExtendDistance = 3f;
    public float slidingMultiplier = 1f;
    public float slideSpeed = 15f;
    public bool useAngleLimits = true;
    public float minAngle = -240f;
    public float maxAngle = 68f;

    [Header("Micro Adjustment (Momentum)")]
    public float microAdjustSpeed = 2f;
    public float microAdjustMaxOffset = 1.5f;
    public float microAdjustDecaySpeed = 4f;
    private float _microAdjustOffset = 0f;

    void Start()
    {
        if (baseTransform != null) _virtualCursorPos = baseTransform.position;
    }

    void FixedUpdate()
    {
        if (baseTransform == null || slidingObject == null) return;

        UpdateVirtualCursor();
        HandleMicroAdjustment();
        ApplySlidingObjectMovement();
    }

    private void UpdateVirtualCursor()
    {
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float mouseDistance = Vector2.Distance(mouseWorldPos, baseTransform.position);

        // Visual cursor follows mouse freely once outside deadzone radius
        if (mouseDistance > cursorDeadzoneRadius)
        {
            _virtualCursorPos = Vector2.Lerp(_virtualCursorPos, mouseWorldPos, Time.fixedDeltaTime * virtualCursorSmoothSpeed);
        }

        if (virtualCursor != null) virtualCursor.position = _virtualCursorPos;
    }

    private void HandleMicroAdjustment()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");

        if (Mathf.Abs(horizontalInput) > 0.01f)
        {
            _microAdjustOffset += horizontalInput * microAdjustSpeed * Time.fixedDeltaTime;
        }
        else
        {
            _microAdjustOffset = Mathf.MoveTowards(_microAdjustOffset, 0f, microAdjustDecaySpeed * Time.fixedDeltaTime);
        }

        _microAdjustOffset = Mathf.Clamp(_microAdjustOffset, -microAdjustMaxOffset, microAdjustMaxOffset);
    }

    private void ApplySlidingObjectMovement()
    {
        Vector2 dirToVirtual = _virtualCursorPos - (Vector2)baseTransform.position;
        float extensionFactor = 0f;

        // Constraint check: only allow extension if within the mechanical angle slice
        if (IsWithinMechanicalLimits(dirToVirtual))
        {
            extensionFactor = CalculateExtensionFactor(dirToVirtual.magnitude);
        }

        // Apply physical movement
        float baseSlideX = Mathf.Sign(dirToVirtual.x) * extensionFactor * slidingMultiplier;
        Vector3 targetPos = baseTransform.position + new Vector3(baseSlideX + _microAdjustOffset, 0f, 0f);
        
        slidingObject.position = Vector3.Lerp(slidingObject.position, targetPos, Time.fixedDeltaTime * slideSpeed);
    }

    private bool IsWithinMechanicalLimits(Vector2 dir)
    {
        if (!useAngleLimits) return true;

        float currentAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        float relativeAngle = Mathf.DeltaAngle(baseTransform.eulerAngles.z, currentAngle);

        float midPoint = (minAngle + maxAngle) / 2f;
        if (relativeAngle < midPoint - 180) relativeAngle += 360;
        else if (relativeAngle > midPoint + 180) relativeAngle -= 360;

        return (relativeAngle >= minAngle && relativeAngle <= maxAngle);
    }

    private float CalculateExtensionFactor(float dist)
    {
        float extension = Mathf.Max(0f, dist - chainLength);
        return Mathf.Clamp01(extension / maxExtendDistance);
    }

    void OnDrawGizmos()
    {
        if (baseTransform == null) return;
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(baseTransform.position, cursorDeadzoneRadius);
        
        // Visualizing the mechanical "Allowed" zone
        if (useAngleLimits)
        {
            Gizmos.color = Color.cyan;
            float baseRot = baseTransform.eulerAngles.z;
            Vector3 minV = Quaternion.Euler(0, 0, baseRot + minAngle) * Vector3.right;
            Vector3 maxV = Quaternion.Euler(0, 0, baseRot + maxAngle) * Vector3.right;
            Gizmos.DrawRay(baseTransform.position, minV * chainLength);
            Gizmos.DrawRay(baseTransform.position, maxV * chainLength);
        }
    }
}