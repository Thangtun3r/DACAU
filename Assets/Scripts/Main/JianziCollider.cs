using System;
using UnityEngine;

public class JianziCollider : MonoBehaviour
{
    // ── Events ───────────────────────────────────────────────────────────────
    /// <summary>Fired on a valid kick. Passes impact force.</summary>
    public static event Action<float> OnCollisionEntered;

    // ── Inspector ────────────────────────────────────────────────────────────
    [Header("Tags")]
    public string targetTag = "Player";
    public string groundTag = "Ground";

    [Header("Kick Validation")]
    public float impactThreshold    = 10f;   // minimum collision force
    public float speedThreshold     = 1f;    // minimum Jianzi speed
    public float cooldownTime       = 1f;
    public float minValidBounceDot  = 0.5f;  // contact normal must face upward by this much
    [Tooltip("Bottom fraction of the collider that counts as a valid contact zone (0–1).")]
    public float bottomPercentage   = 0.25f;

    // ── Private ──────────────────────────────────────────────────────────────
    private ParticleSystem _hitParticles;
    private Vector3        _spawnPosition;
    private float          _lastCollisionTime = float.NegativeInfinity;
    private bool           _isConstrained     = false;

    // ── Unity ────────────────────────────────────────────────────────────────
    void Start()
    {
        _spawnPosition = transform.position;
        _hitParticles  = GetComponent<ParticleSystem>();
        _hitParticles?.Stop();
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (Time.time - _lastCollisionTime < cooldownTime) return;
        _lastCollisionTime = Time.time;

        float        impactForce = col.relativeVelocity.magnitude;
        Rigidbody2D  rb          = GetComponent<Rigidbody2D>();
        float        currentSpeed = rb != null ? rb.linearVelocity.magnitude : 0f;

        if (col.collider.CompareTag(targetTag)
            && impactForce  > impactThreshold
            && currentSpeed > speedThreshold
            && !_isConstrained)
        {
            if (!HasValidNormal(col))  return;
            if (!IsBottomContact(col)) return;

            _hitParticles?.Play();
            OnCollisionEntered?.Invoke(impactForce);
            Scoreboard.AddPoints(1);
        }
        else if (col.collider.CompareTag(groundTag))
        {
            ResetToSpawn(rb);
            Scoreboard.ResetScore();
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────
    bool HasValidNormal(Collision2D col)
    {
        if (col.contacts.Length == 0) return false;
        return Vector2.Dot(col.contacts[0].normal, Vector2.up) >= minValidBounceDot;
    }

    bool IsBottomContact(Collision2D col)
    {
        Collider2D myCol = GetComponent<Collider2D>();
        if (myCol == null) return true; // can't check – allow by default

        float bottomEdge       = myCol.bounds.min.y;
        float bottomThresholdY = bottomEdge + myCol.bounds.size.y * bottomPercentage;

        foreach (ContactPoint2D contact in col.contacts)
            if (contact.point.y <= bottomThresholdY)
                return true;

        return false;
    }

    void ResetToSpawn(Rigidbody2D rb)
    {
        transform.position = _spawnPosition;
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }
}
