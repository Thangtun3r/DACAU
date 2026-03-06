using UnityEngine;

public class DebugVelocity2D : MonoBehaviour
{
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogWarning($"[DebugVelocity2D] No Rigidbody2D found on {gameObject.name}.");
        }
    }

    void FixedUpdate()
    {
        if (rb != null)
        {
            Debug.Log($"[Velocity Debug] {gameObject.name} | Velocity: {rb.linearVelocity} | Speed: {rb.linearVelocity.magnitude:F2} | Angular Velocity: {rb.angularVelocity:F2}");
        }
    }
}