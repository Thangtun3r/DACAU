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

    private float _currentSmoothAngle;
    private float _angleVelocity;

    void FixedUpdate() 
    {
        if (targetRB == null || baseTransform == null) return;

        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 basePos = baseTransform.position;
        Vector2 dirToMouse = mouseWorldPos - basePos;
        float mouseDistance = dirToMouse.magnitude;

        float targetAngle;

        if (useAngleLimits)
        {
            float currentMouseAngle = Mathf.Atan2(dirToMouse.y, dirToMouse.x) * Mathf.Rad2Deg;
            float baseRot = baseTransform.eulerAngles.z;
            float relativeAngle = Mathf.DeltaAngle(baseRot, currentMouseAngle);

            // Unwrap for wide ranges
            float midPoint = (minAngle + maxAngle) / 2f;
            if (relativeAngle < midPoint - 180) relativeAngle += 360;
            else if (relativeAngle > midPoint + 180) relativeAngle -= 360;

            bool inDeadzone = (relativeAngle < minAngle || relativeAngle > maxAngle);
            float clampedTarget = Mathf.Clamp(relativeAngle, minAngle, maxAngle);

            if (inDeadzone)
            {
                _currentSmoothAngle = Mathf.SmoothDampAngle(_currentSmoothAngle, clampedTarget, ref _angleVelocity, flipSmoothing);
            }
            else
            {
                _currentSmoothAngle = clampedTarget;
                _angleVelocity = 0; 
            }
            targetAngle = baseRot + _currentSmoothAngle;
        }
        else
        {
            targetAngle = Mathf.Atan2(dirToMouse.y, dirToMouse.x) * Mathf.Rad2Deg;
        }

        float resultRad = targetAngle * Mathf.Deg2Rad;
        Vector2 finalDir = new Vector2(Mathf.Cos(resultRad), Mathf.Sin(resultRad));
        float finalDistance = Mathf.Clamp(mouseDistance, minRange, chainLength);
        Vector2 targetPosition = basePos + (finalDir * finalDistance);

        targetRB.velocity = (targetPosition - targetRB.position) * followSpeed;
    }

    void OnDrawGizmos()
    {
        if (baseTransform == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(baseTransform.position, chainLength);
        Gizmos.color = Color.cyan;
        float baseRot = baseTransform.eulerAngles.z;
        Vector3 minV = Quaternion.Euler(0, 0, baseRot + minAngle) * Vector3.right;
        Vector3 maxV = Quaternion.Euler(0, 0, baseRot + maxAngle) * Vector3.right;
        Gizmos.DrawRay(baseTransform.position, minV * chainLength);
        Gizmos.DrawRay(baseTransform.position, maxV * chainLength);
    }
}