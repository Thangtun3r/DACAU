using UnityEngine;

public class RotateToMouse2D : MonoBehaviour
{
    public float minAngle = -90f;
    public float maxAngle = 90f;

    void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = mousePos - transform.position;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Clamp the leg rotation
        angle = Mathf.Clamp(angle, minAngle, maxAngle);

        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}