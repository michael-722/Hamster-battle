using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FanZone : MonoBehaviour
{
    [Header("Fan Settings")]
    public Vector3 direction = Vector3.forward; // local wind direction
    public float force = 20f;                   // strength
    public float range = 5f;                    // distance for full effect
    public bool useFalloff = true;              // weaker with distance
    public bool visualize = true;               // draw gizmos

    private void Reset()
    {
        var col = GetComponent<Collider>(); // ensure trigger
        col.isTrigger = true;
    }

    private void OnTriggerStay(Collider other)
    {
        var rb = other.attachedRigidbody; // target rigidbody
        if (rb == null) return;

        // wind direction in world space
        Vector3 worldDir = transform.TransformDirection(direction).normalized;

        // simple distance-based falloff 
        float t = 1f;
        if (useFalloff)
        {
            float dist = Vector3.Distance(transform.position, rb.position);
            t = Mathf.Clamp01(1f - dist / Mathf.Max(0.0001f, range));
        }

        // continuous push (Acceleration ignores mass)
        rb.AddForce(worldDir * force * t, ForceMode.Acceleration);
    }

    private void OnDrawGizmos()
    {
        if (!visualize) return;

        // draw a simple arrow (line + spheres)
        Vector3 worldDir = transform.TransformDirection(direction).normalized;
        Vector3 start = transform.position;
        Vector3 end = start + worldDir * range;

        Gizmos.color = new Color(0f, 0.6f, 1f, 0.35f);
        Gizmos.DrawLine(start, end);
        Gizmos.DrawWireSphere(start, 0.2f);
        Gizmos.DrawWireSphere(end, 0.3f);

        // little arrow head
        Vector3 right = Vector3.Cross(worldDir, Vector3.up).normalized;
        float head = 0.4f;
        Gizmos.DrawLine(end, end - worldDir * head + right * (head * 0.5f));
        Gizmos.DrawLine(end, end - worldDir * head - right * (head * 0.5f));
    }
}
