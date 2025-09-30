using UnityEngine;

// Controls ball shooting and aiming
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(LineRenderer))]
public class HamsterBallController : MonoBehaviour
{
    [Header("Shot Gating")]
    public float stopThreshold = 0.2f; // how slow ball must be to shoot
    public float stopSettleTime = 0.2f; // how long ball must stay stopped

    [Header("Slingshot")]
    public float power = 12f; // shot strength
    public float maxDragDistance = 4f; // max pull distance
    public float maxUpAngleDegrees = 35f; // max aim up angle
    public float fineAimScale = 0.4f; // slow aim when holding shift

    [Header("Trajectory")]
    public int trajectoryPoints = 25; // how many dots in arc
    public float timeStep = 0.06f; // gap between dots

    [Header("Audio")]
    public AudioSource shootSfx; // sound on shoot

    private Rigidbody rb; // ball physics
    private LineRenderer lr; // draw aim line

    private bool isDragging = false; // are we aiming
    private Vector3 dragStartWorld; // where drag began
    private Plane aimPlane; // flat plane for aiming

    private float stoppedTimer = 0f; // time ball stayed still
    private bool canShoot = false; // if we can shoot

    private Camera cam; // main camera

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        lr = GetComponent<LineRenderer>();
        cam = Camera.main;

        aimPlane = new Plane(Vector3.up, transform.position); // aim plane at ball height
        lr.positionCount = 0; // clear line
    }

    private void Update()
    {
        UpdateStopGate(); // check if ball is still
        HandleInput(); // check mouse

        if (Input.GetKeyDown(KeyCode.R))
        {
            StopBall();       // Dev hotkey: instantly stop the ball (REMOVE LATER... or turn into power-up)

        }

        DrawTrajectoryPreview(); // show arc
    }

    private void UpdateStopGate()
    {
        // if ball is slow enough, start timer
        if (rb.linearVelocity.magnitude <= stopThreshold && rb.angularVelocity.magnitude <= stopThreshold * 2f)
        {
            stoppedTimer += Time.deltaTime;
            if (stoppedTimer >= stopSettleTime) canShoot = true;
        }
        else
        {
            stoppedTimer = 0f;
            canShoot = false;
        }
    }

    private void HandleInput()
    {
        // start drag
        if (Input.GetMouseButtonDown(0))
        {
            if (!canShoot) return;
            if (TryGetMouseWorldOnAimPlane(out Vector3 hit))
            {
                isDragging = true;
                dragStartWorld = hit;
            }
        }

        // release drag
        if (Input.GetMouseButtonUp(0))
        {
            if (isDragging)
            {
                isDragging = false;
                if (!canShoot) { lr.positionCount = 0; return; }

                // find end drag point
                Vector3 aimWorld;
                if (!TryGetMouseWorldOnAimPlane(out aimWorld)) { lr.positionCount = 0; return; }

                // drag vector
                Vector3 dragVec = dragStartWorld - aimWorld;
                float sensitivity = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? fineAimScale : 1f;
                dragVec *= sensitivity;

                // clamp distance
                if (dragVec.magnitude > maxDragDistance)
                    dragVec = dragVec.normalized * maxDragDistance;

                // clamp vertical aim
                Vector3 flat = new Vector3(dragVec.x, 0f, dragVec.z);
                float upRatio = Mathf.Clamp(Mathf.Tan(maxUpAngleDegrees * Mathf.Deg2Rad), 0f, 3f);
                float maxY = flat.magnitude * upRatio;
                dragVec.y = Mathf.Clamp(dragVec.y, -maxY, maxY);

                // make impulse
                Vector3 impulse = dragVec * power;

                FireShot(impulse); // shoot

                lr.positionCount = 0; // clear line
            }
        }
    }

    private void DrawTrajectoryPreview()
    {
        // no preview unless dragging
        if (!isDragging || !canShoot) { lr.positionCount = 0; return; }

        if (!TryGetMouseWorldOnAimPlane(out Vector3 aimWorld)) { lr.positionCount = 0; return; }

        // same drag logic as before
        Vector3 dragVec = dragStartWorld - aimWorld;
        float sensitivity = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? fineAimScale : 1f;
        dragVec *= sensitivity;

        if (dragVec.magnitude > maxDragDistance)
            dragVec = dragVec.normalized * maxDragDistance;

        Vector3 flat = new Vector3(dragVec.x, 0f, dragVec.z);
        float upRatio = Mathf.Clamp(Mathf.Tan(maxUpAngleDegrees * Mathf.Deg2Rad), 0f, 3f);
        float maxY = flat.magnitude * upRatio;
        dragVec.y = Mathf.Clamp(dragVec.y, -maxY, maxY);

        // predict arc
        Vector3 startPos = transform.position;
        Vector3 startVel = rb.linearVelocity;
        Vector3 addedVel = dragVec * power / rb.mass;
        Vector3 v0 = startVel + addedVel;

        lr.positionCount = trajectoryPoints;

        Vector3 g = Physics.gravity;
        for (int i = 0; i < trajectoryPoints; i++)
        {
            float t = i * timeStep;
            Vector3 p = startPos + v0 * t + 0.5f * g * t * t;
            lr.SetPosition(i, p);
        }
    }

    private void FireShot(Vector3 impulse)
    {
        rb.AddForce(impulse, ForceMode.Impulse); // push ball
        canShoot = false;
        stoppedTimer = 0f;
        if (shootSfx != null) shootSfx.Play();
    }

    private bool TryGetMouseWorldOnAimPlane(out Vector3 world)
    {
        // aim plane at ball height
        aimPlane = new Plane(Vector3.up, new Vector3(0f, transform.position.y, 0f));

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (aimPlane.Raycast(ray, out float enter))
        {
            world = ray.GetPoint(enter); // mouse point in world
            return true;
        }

        world = Vector3.zero;
        return false;
    }

    private void StopBall()
    {
        rb.linearVelocity = Vector3.zero; // stop movement
        rb.angularVelocity = Vector3.zero; // stop spinning
        stoppedTimer = stopSettleTime; // trick gate into thinking it's stopped
        canShoot = true; // allow next shot right away
    }

}
