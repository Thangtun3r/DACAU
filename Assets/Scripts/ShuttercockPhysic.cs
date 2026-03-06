using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class JianziPhysics : MonoBehaviour
{
    private Rigidbody2D rb;

    [Header("Aerodynamics Settings")]
    [Tooltip("A coefficient to adjust the magnitude of aerodynamic drag.")]
    public float dragCoefficient = 1.0f;
    [Tooltip("Offset to add to the calculated angle so the sprite faces correctly.")]
    public float offset = 90f;
    
    [Tooltip("Estimate of the frontal area of the Jianzi (in square meters).")]
    public float frontalArea = 0.05f;
    
    [Tooltip("Density of air (kg/m^3). Standard sea-level value is 1.225.")]
    public float airDensity = 1.225f;
    
    [Header("Rotation Settings")]
    [Tooltip("How quickly the Jianzi rotates to align with its velocity.")]
    public float rotationLerpSpeed = 5f;

    [Header("External Forces")]
    [Tooltip("Optional wind force that can be applied (set to zero if not used).")]
    public Vector2 windForce = Vector2.zero;

    [Header("Downforce Settings")]
    [Tooltip("A coefficient to adjust the magnitude of aerodynamic downforce.")]
    public float downforceCoefficient = 1.0f;
    [Tooltip("The maximum allowed downward speed.")]
    public float maxDownwardSpeed = 30f;

    [Header("Velocity Limits")]
    [Tooltip("The maximum allowed overall speed for the Jianzi to prevent tunneling.")]
    public float maxSpeed = 30f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        ApplyAerodynamicDrag();
        ApplyWindForce();
        ApplyDownforce();
        AlignRotationToVelocity();

        // Clamp overall velocity to prevent tunneling issues.
        rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, maxSpeed);

        // Clamp the downward speed (y-axis) to the maximum allowed.
        if (rb.linearVelocity.y < -maxDownwardSpeed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -maxDownwardSpeed);
        }
    }

    /// <summary>
    /// Applies aerodynamic drag based on the Jianzi’s current velocity.
    /// Formula: F_drag = 0.5 * airDensity * speed^2 * dragCoefficient * frontalArea
    /// </summary>
    void ApplyAerodynamicDrag()
    {
        Vector2 velocity = rb.linearVelocity;
        float speed = velocity.magnitude;
        
        if (speed > 0.01f)
        {
            float dragMagnitude = 0.5f * airDensity * speed * speed * dragCoefficient * frontalArea;
            Vector2 dragForce = -dragMagnitude * (velocity / speed);
            rb.AddForce(dragForce);
        }
    }

    /// <summary>
    /// Applies an optional wind force to the Jianzi.
    /// </summary>
    void ApplyWindForce()
    {
        if (windForce != Vector2.zero)
        {
            rb.AddForce(windForce);
        }
    }

    /// <summary>
    /// Applies aerodynamic downforce based on the Jianzi’s current velocity.
    /// Formula: F_downforce = 0.5 * airDensity * speed^2 * downforceCoefficient * frontalArea
    /// This force is always applied downward.
    /// </summary>
    void ApplyDownforce()
    {
        float speed = rb.linearVelocity.magnitude;
        if (speed > 0.01f)
        {
            float downforceMagnitude = 0.5f * airDensity * speed * speed * downforceCoefficient * frontalArea;
            rb.AddForce(Vector2.down * downforceMagnitude);
        }
    }

    /// <summary>
    /// Aligns the Jianzi’s rotation with its velocity vector.
    /// </summary>
    void AlignRotationToVelocity()
    {
        Vector2 velocity = rb.linearVelocity;
        float speed = velocity.magnitude;
    
        // Only adjust rotation if there is enough movement.
        if (speed > 0.1f)
        {
            float targetAngle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg + offset;
        
            // Calculate a lerp factor that scales with the speed.
            // When speed is at maxSpeed, the factor equals rotationLerpSpeed * Time.fixedDeltaTime,
            // and when speed is low, the factor becomes small.
            float lerpFactor = rotationLerpSpeed * (speed / maxSpeed) * Time.fixedDeltaTime;
            lerpFactor = Mathf.Clamp01(lerpFactor);  // Ensure the factor stays between 0 and 1.
        
            float newRotation = Mathf.LerpAngle(rb.rotation, targetAngle, lerpFactor);
            rb.MoveRotation(newRotation);
        }
    }

}
