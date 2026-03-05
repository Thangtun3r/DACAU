using UnityEngine;

public class FootSolver : MonoBehaviour
{
    public Transform hip;
    public Transform leg;

    public float footOffset = -90f;

    [Range(0f,1f)]
    public float adjustmentStrength = 0.3f;

    public float adjustmentMin = -20f;
    public float adjustmentMax = 25f;

    void Update()
    {
        SolveFoot();
    }

    void SolveFoot()
    {
        Vector2 legDir = (leg.position - hip.position).normalized;
        Vector2 vertical = Vector2.down;

        float legAngle = Vector2.SignedAngle(vertical, legDir);

        legAngle = Mathf.Clamp(legAngle, adjustmentMin, adjustmentMax);

        float adjustment = legAngle * adjustmentStrength;

        float baseAngle = leg.eulerAngles.z + footOffset;

        float finalAngle = baseAngle + adjustment;

        transform.rotation = Quaternion.Euler(0, 0, finalAngle);
    }

}