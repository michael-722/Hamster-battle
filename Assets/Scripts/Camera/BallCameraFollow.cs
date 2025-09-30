using UnityEngine;

public class BallCameraFollow : MonoBehaviour
{
    public Transform target; // the ball we follow

    [Header("Offsets")]
    public Vector3 offset = new Vector3(0f, 6f, -8f); // camera position offset
    public Vector3 lookOffset = new Vector3(0f, 0.5f, 0f); // where camera looks

    [Header("Smoothing")]
    public float followLerp = 10f; // move speed
    public float lookLerp = 12f; // rotate speed

    private Vector3 vel; // unused, but kept for smoothing options

    private void LateUpdate()
    {
        if (target == null) return; // if no ball, do nothing

        Vector3 desiredPos = target.position + offset; // target camera position

        // check if wall blocks view
        Vector3 dir = (desiredPos - target.position).normalized;
        float dist = offset.magnitude;
        if (Physics.Raycast(target.position, dir, out RaycastHit hit, dist))
        {
            desiredPos = hit.point - dir * 0.3f; // move camera in front of wall
        }

        // smoothly move camera
        transform.position = Vector3.Lerp(transform.position, desiredPos, 1f - Mathf.Exp(-followLerp * Time.deltaTime));

        // smoothly rotate camera
        Vector3 desiredLook = target.position + lookOffset;
        Vector3 lookPos = Vector3.Lerp(transform.position + transform.forward, desiredLook, 1f - Mathf.Exp(-lookLerp * Time.deltaTime));
        transform.rotation = Quaternion.LookRotation((lookPos - transform.position).normalized, Vector3.up);
    }
}
