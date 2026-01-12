using TreeEditor;
using UnityEngine;
using UnityEngine.Splines;
using System.Collections;

// Orbit follow camera: RMB drag = rotate, wheel = zoom
public class BallCameraFollow : MonoBehaviour
{
    public Transform target; // ball to follow

    [Header("Start Offset")]
    public Vector3 offset = new Vector3(0f, 6f, -8f); // initial camera offset

    [Header("Orbit")]
    public float yaw;   // horizontal angle (deg)
    public float pitch = 20f; // vertical angle (deg)
    public float minPitch = -10f; // clamp down
    public float maxPitch = 75f;  // clamp up
    public float xSensitivity = 3f; // mouse X speed
    public float ySensitivity = 3f; // mouse Y speed
    public float distance = 8f;     // camera distance
    public float minDistance = 3f;  // min zoom
    public float maxDistance = 15f; // max zoom
    public float zoomSensitivity = 2f; // wheel speed

    [Header("Look")]
    public Vector3 lookOffset = new Vector3(0f, 0.5f, 0f); // look-at offset

    [Header("Smoothing")]
    public float followLerp = 10f; // move smoothing
    public float lookLerp = 12f;   // turn smoothing

    [Header("Collision")]
    public float collisionPadding = 0.3f; // keep camera off walls

    private SplineAnimate splineAnim;
    private bool panning = false;

    void Start()
    {
        // derive orbit from starting offset
        distance = offset.magnitude; // starting zoom
        yaw = Mathf.Atan2(offset.x, -offset.z) * Mathf.Rad2Deg; // around Y
        float horiz = new Vector2(offset.x, offset.z).magnitude; // flat length
        pitch = Mathf.Atan2(offset.y, horiz) * Mathf.Rad2Deg; // up/down

        splineAnim = gameObject.GetComponent<SplineAnimate>();
    }

    void LateUpdate()
    {
        if (target == null)
        {
            try { target = FindAnyObjectByType<HamsterBallController>().transform; }
            catch { return; }// no target
        }

        // orbit input (hold right mouse)
        if (Input.GetMouseButton(1))
        {
            if (SettingsMenu.instance.isPaused) return;
            float senMult = Mathf.Lerp(0.5f, 2f, Mathf.Clamp01(SettingsMenu.instance.GetSenVal()));

            yaw += Input.GetAxis("Mouse X") * 100f * xSensitivity * senMult * Time.deltaTime;   // left/right
            pitch -= Input.GetAxis("Mouse Y") * 100f * ySensitivity * senMult * Time.deltaTime; // up/down
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch); // clamp pitch
        }

        // zoom input (wheel)
        distance -= Input.mouseScrollDelta.y * zoomSensitivity;
        distance = Mathf.Clamp(distance, minDistance, maxDistance); // clamp zoom

        // desired orbital offset
        Vector3 orbitOffset = Quaternion.Euler(pitch, yaw, 0f) * new Vector3(0f, 0f, -distance);

        // desired camera position
        Vector3 desiredPos = target.position + orbitOffset;

        // collision push-in
        Vector3 dir = (desiredPos - target.position).normalized; // ray dir
        float dist = orbitOffset.magnitude; // ray length
        if (Physics.Raycast(target.position, dir, out RaycastHit hit, dist))
        {
            desiredPos = hit.point - dir * collisionPadding; // pull off wall
        }

        // smooth move
        if (!panning)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                desiredPos,
                1f - Mathf.Exp(-followLerp * Time.deltaTime));
        }

        // smooth look
        Vector3 desiredLook = target.position + lookOffset; // look target
        Vector3 lookPos = Vector3.Lerp(
            transform.position + transform.forward,
            desiredLook,
            1f - Mathf.Exp(-lookLerp * Time.deltaTime)
        );
        transform.rotation = Quaternion.LookRotation((lookPos - transform.position).normalized, Vector3.up); // face ball
    }

    public void panLevel()
    {
        if (splineAnim == null)
        {
            Debug.Log("sp null");
            return;
        }

        StartCoroutine(PanLevelCoroutine());
    }

    private IEnumerator PanLevelCoroutine()
    {
        splineAnim.Play();

        panning = true;
        while (splineAnim.IsPlaying) yield return null;
        panning = false;
    }
}
