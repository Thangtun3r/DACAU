using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ShutterCock : MonoBehaviour
{
    // ── Events ───────────────────────────────────────────────────────────────
    /// <summary>Fired when the shuttlecock hits a tagged Player collider. Passes impact force.</summary>
    public static event Action<float> OnPlayerHit;

    /// <summary>Fired when the shuttlecock hits a tagged Ground collider.</summary>
    public static event Action OnGroundHit;

    // ── Inspector ────────────────────────────────────────────────────────────
    [Header("Collision Tags")]
    public string playerTag = "Player";
    public string groundTag = "Ground";

    [Header("Aerodynamics")]
    [Tooltip("Scales the magnitude of aerodynamic drag.")]
    public float dragCoefficient   = 1.0f;
    [Tooltip("Estimated frontal area (m²).")]
    public float frontalArea       = 0.05f;
    [Tooltip("Air density (kg/m³). Sea-level standard = 1.225.")]
    public float airDensity        = 1.225f;

    [Header("Rotation")]
    [Tooltip("Sprite orientation offset so the asset faces the velocity direction.")]
    public float orientationOffset = 90f;
    [Tooltip("How quickly the shuttlecock rotates to align with velocity.")]
    public float rotationLerpSpeed = 5f;

    [Header("Downforce")]
    [Tooltip("Scales the aerodynamic downforce.")]
    public float downforceCoefficient = 1.0f;
    [Tooltip("Maximum downward speed (m/s).")]
    public float maxDownwardSpeed     = 30f;

    [Header("Speed Limit")]
    [Tooltip("Maximum overall speed – prevents tunnelling.")]
    public float maxSpeed = 30f;

    [Header("Wind")]
    [Tooltip("Optional constant wind force (zero = disabled).")]
    public Vector2 windForce = Vector2.zero;

    // ── Private ──────────────────────────────────────────────────────────────
    private Rigidbody2D _rb;

    // ── Unity ────────────────────────────────────────────────────────────────
    void Awake() => _rb = GetComponent<Rigidbody2D>();

    void FixedUpdate()
    {
        ApplyAerodynamicDrag();
        ApplyWindForce();
        ApplyDownforce();
        AlignRotationToVelocity();
        ClampVelocity();
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.collider.CompareTag(playerTag))
        {
            OnPlayerHit?.Invoke(col.relativeVelocity.magnitude);
        }
        else if (col.collider.CompareTag(groundTag))
        {
            OnGroundHit?.Invoke();
        }
    }

    // ── Physics helpers ──────────────────────────────────────────────────────

    /// F_drag = ½ · ρ · v² · Cd · A  (opposed to velocity)
    void ApplyAerodynamicDrag()
    {
        Vector2 vel   = _rb.linearVelocity;
        float   speed = vel.magnitude;
        if (speed < 0.01f) return;

        float   mag  = 0.5f * airDensity * speed * speed * dragCoefficient * frontalArea;
        Vector2 drag = -mag * vel.normalized;
        _rb.AddForce(drag);
    }

    void ApplyWindForce()
    {
        if (windForce != Vector2.zero)
            _rb.AddForce(windForce);
    }

    /// Same formula as drag but always acts downward.
    void ApplyDownforce()
    {
        float speed = _rb.linearVelocity.magnitude;
        if (speed < 0.01f) return;

        float mag = 0.5f * airDensity * speed * speed * downforceCoefficient * frontalArea;
        _rb.AddForce(Vector2.down * mag);
    }

    void AlignRotationToVelocity()
    {
        Vector2 vel   = _rb.linearVelocity;
        float   speed = vel.magnitude;
        if (speed < 0.1f) return;

        float targetAngle = Mathf.Atan2(vel.y, vel.x) * Mathf.Rad2Deg + orientationOffset;

        // Scale lerp factor with speed so slow motion doesn't snap.
        float t = Mathf.Clamp01(rotationLerpSpeed * (speed / maxSpeed) * Time.fixedDeltaTime);
        _rb.MoveRotation(Mathf.LerpAngle(_rb.rotation, targetAngle, t));
    }

    void ClampVelocity()
    {
        Vector2 vel = Vector2.ClampMagnitude(_rb.linearVelocity, maxSpeed);
        if (vel.y < -maxDownwardSpeed) vel.y = -maxDownwardSpeed;
        _rb.linearVelocity = vel;
    }
}
