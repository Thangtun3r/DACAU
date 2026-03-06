using UnityEngine;
using System;

public class JianziCollider : MonoBehaviour
{
    public static event Action<float> OnCollisionEntered; // Sends force
    public string targetTag = "Player";   // The tag for a valid kick
    public string groundTag = "Ground";   // For a losing condition (if needed)
    public float impactThreshold = 10f;   // Minimum collision force required
    public float speedThreshold = 1f;     // Minimum Jianzi speed required
    public float cooldownTime = 1f;       // Cooldown duration in seconds
    public float minValidBounceDot = 0.5f;  
    // Percentage of the collider's height that is considered the "bottom" region (e.g., 0.25 = bottom 25%)
    public float bottomPercentage = 0.25f;  

    private ParticleSystem particleSystem;
    private Vector3 JianziOriginalTransform;
    private float lastCollisionTime = -Mathf.Infinity;
    private bool isConstraint = false;
    
    private void Start()
    {
        JianziOriginalTransform = transform.position;
        particleSystem = GetComponent<ParticleSystem>();
        if (particleSystem != null)
        {
            particleSystem.Stop();
        }
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (Time.time - lastCollisionTime < cooldownTime)
            return;
        
        lastCollisionTime = Time.time;
        float impactForce = collision.relativeVelocity.magnitude;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        float currentSpeed = rb.linearVelocity.magnitude;
        
        if (collision.collider.CompareTag(targetTag) &&
            impactForce > impactThreshold &&
            currentSpeed > speedThreshold &&
            isConstraint == false)
        {
            if (collision.contacts.Length > 0)
            {
                float dot = Vector2.Dot(collision.contacts[0].normal, Vector2.up);
                if (dot < minValidBounceDot)
                {
                    return;
                }
            }
            
            // New check: Only accept the collision if the contact point(s) are on the bottom of the collider.
            Collider2D myCollider = GetComponent<Collider2D>();
            if (myCollider != null)
            {
                float bottomEdge = myCollider.bounds.min.y;
                float colliderHeight = myCollider.bounds.size.y;
                float bottomThresholdY = bottomEdge + colliderHeight * bottomPercentage;
                
                bool validBottomContact = false;
                foreach (ContactPoint2D contact in collision.contacts)
                {
                    if (contact.point.y <= bottomThresholdY)
                    {
                        validBottomContact = true;
                        break;
                    }
                }
                if (!validBottomContact)
                {
                    return;
                }
            }
            
            if (particleSystem != null)
            {
                particleSystem.Play();
            }
            OnCollisionEntered?.Invoke(impactForce);
            Scoreboard.AddPoints(1);
        }
        else
        {
            if (collision.collider.CompareTag(groundTag))
            {
                transform.position = JianziOriginalTransform;
                rb.linearVelocity = Vector2.zero;
                Scoreboard.ResetScore();
            }
        }
    }
    
}
