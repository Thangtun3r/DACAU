using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CursorMovementController : MonoBehaviour
{
    [Header("Movement Settings")]
    // The allowed radius (max distance) for the mouse cursor.
    public float maxDistance = 5f;
    // The movement speed factor (units per second).
    public float moveSpeed = 10f;
    // Damping factor used for smoothing transitions.
    public float damping = 5f;

    // Cached reference to the Rigidbody2D component.
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        Vector2 mouseWorldPos = GetMouseWorldPosition();
        DrawDebugRay(mouseWorldPos);
        ProcessMovement(mouseWorldPos);
    }

    // Converts the mouse position from screen space to world space.
    private Vector2 GetMouseWorldPosition()
    {
        Vector3 screenPos = Input.mousePosition;
        return Camera.main.ScreenToWorldPoint(screenPos);
    }

    // Draws a debug ray from this object's position toward the mouse (up to maxDistance).
    private void DrawDebugRay(Vector2 mouseWorldPos)
    {
        Vector2 direction = (mouseWorldPos - (Vector2)transform.position).normalized;
        Debug.DrawRay(transform.position, direction * maxDistance, Color.green);
    }

    // Processes movement based on the distance between this object and the mouse.
    // When the distance exceeds maxDistance, the object moves toward the cursor.
    // When within maxDistance, the object's velocity is smoothly damped to zero.
    private void ProcessMovement(Vector2 mouseWorldPos)
    {
        Vector2 currentPos = transform.position;
        Vector2 toMouse = mouseWorldPos - currentPos;
        float distance = toMouse.magnitude;

        if (distance > maxDistance)
        {
            // Move the object toward the mouse position.
            Vector2 direction = toMouse.normalized;
            Vector2 targetVelocity = direction * moveSpeed;

            // Smoothly interpolate the current velocity toward the target velocity.
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, targetVelocity, damping * Time.deltaTime);
        }
        else
        {
            // When the cursor is within the allowed distance, gently damp the velocity to zero.
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, damping * Time.deltaTime);
        }
    }
}
