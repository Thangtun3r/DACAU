using UnityEngine;

public class TraiCauThrow : MonoBehaviour
{
    [Header("References")]
    public GameObject traiCau;

    [Header("Throw Settings")]
    public float throwForce = 10f;
    public Vector2 throwDirection = Vector2.up;

    [Header("Spawn")]
    public Transform spawnPoint;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            Throw();
    }

    public void Throw()
    {
        if (traiCau == null)
        {
            Debug.LogWarning("[TraiCauThrow] No traiCau assigned!", this);
            return;
        }

        Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : transform.position;

        traiCau.transform.position = spawnPos;
        traiCau.SetActive(true);

        Rigidbody2D rb = traiCau.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(throwDirection.normalized * throwForce, ForceMode2D.Impulse);
        }
        else
        {
            Debug.LogWarning("[TraiCauThrow] traiCau has no Rigidbody2D!", this);
        }
    }
}