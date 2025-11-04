using UnityEngine;

using Unity.Netcode;
using UnityEngine.UIElements;
using Unity.Services.Authentication;
using Unity.Netcode.Components;//networking

// Slingshot: pull = power, mouse wheel = arc; 0° arc draws a flat ground line
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(LineRenderer))]
public class HamsterBallController : NetworkBehaviour
{
    [Header("Shot Gating")]
    public float stopThreshold = 0.2f; // speed to count as stopped
    public float stopSettleTime = 0.2f; // time under threshold

    [Header("Slingshot")]
    public float power = 12f; // shot strength
    public float maxDragDistance = 4f; // max pull
    public float fineAimScale = 0.4f; // hold shift for fine aim

    [Header("Loft (arc)")]
    public float loftAngleDeg = 0f; // current arc angle
    public float loftMin = 0f; // min arc
    public float loftMax = 35f; // max arc
    public float loftAdjustPerWheelStep = 3f; // wheel step in degrees
    public float loftAdjustSpeed = 60f; // Q/E adjust speed

    [Header("Trajectory")]
    public int trajectoryPoints = 25; // number of points
    public float timeStep = 0.06f; // arc timestep
    public float flatPreviewMaxDistance = 8f; // max flat line length at 0°

    [Header("Audio (optional)")]
    public AudioSource shootSfx; // sound on shoot

    private Rigidbody rb; // ball rigidbody
    private LineRenderer lr; // line renderer
    private bool isDragging = false; // dragging state
    private Vector3 dragStartWorld; // drag start
    private Plane aimPlane; // plane at ball height
    private float stoppedTimer = 0f; // stopped timer
    private bool canShoot = false; // shoot gate
    private Camera cam; // main camera

    [Header("Powerup")]
    private float basePower; // original power

    private bool canMove = false;

    private bool inputEnabled = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        lr = GetComponent<LineRenderer>();
        cam = Camera.main;
        aimPlane = new Plane(Vector3.up, transform.position); // plane at ball
        lr.positionCount = 0; // clear line
        basePower = Mathf.Max(0.0001f, power); // capture inspector power
    }

    private void Start()
    {
        basePower = power; // remember original power
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            int id = (int)OwnerClientId;
            float radius = 10f;
            float angle = id * 90f;//90 appart - cahnge if more players added
            Vector3 offset = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0f, Mathf.Sin(angle * Mathf.Deg2Rad)) * radius;
            transform.position = new Vector3(offset.x, 5f, offset.z);
        }
    }

    private void Update()
    {
        if (GateToOwner()) return; // only gate when netcode is running

        if (!canMove)
            if (!CheckGameStatus()) return;//disable player until game starts
            else gameObject.GetComponent<PlayerRace>().startRaceTimer();//starts timer

                UpdateStopGate(); // update canShoot

        if (inputEnabled) // only when enabled
        {
            HandleLoftAdjust(); // wheel/Q/E adjusts loft
            HandleInput(); // drag + release
            DrawTrajectoryPreview(); // draw line
            if (Input.GetKeyDown(KeyCode.R)) StopBall(); // dev freeze
        }
    }
    
    private bool CheckGameStatus()
    {
        if (LobbySetUp.instance == null)
        {
            Debug.Log("no lobby set up instance");
            return canMove;
        }
        canMove = LobbySetUp.instance.canStart();
        return canMove;
    }

    // true only when Netcode is active and this isn't our owned ball
    private bool GateToOwner()
    {
        var nm = NetworkManager.Singleton; // netcode ref
        if (nm == null || !nm.IsListening) return false; // offline: don't gate
        if (!IsSpawned) return false; // not spawned: don't gate
        return !IsOwner; // online and not ours: gate
    }

    private void UpdateStopGate()
    {
        // ready when speeds are small for a moment
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

    private void HandleLoftAdjust()
    {
        float wheel = Input.mouseScrollDelta.y; // mouse wheel delta
        if (Mathf.Abs(wheel) > 0f) loftAngleDeg += wheel * loftAdjustPerWheelStep; // wheel adjust
        if (Input.GetKey(KeyCode.Q)) loftAngleDeg -= loftAdjustSpeed * Time.deltaTime; // Q down
        if (Input.GetKey(KeyCode.E)) loftAngleDeg += loftAdjustSpeed * Time.deltaTime; // E up
        loftAngleDeg = Mathf.Clamp(loftAngleDeg, loftMin, loftMax); // clamp
    }

    private void HandleInput()
    {
        // begin drag
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
            if (!isDragging) return;
            isDragging = false;
            if (!canShoot) { lr.positionCount = 0; return; }
            if (!TryGetMouseWorldOnAimPlane(out Vector3 aimWorld)) { lr.positionCount = 0; return; }

            Vector3 dragVec = dragStartWorld - aimWorld; // pull vector
            float sensitivity = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? fineAimScale : 1f; // fine aim
            dragVec *= sensitivity; // apply sensitivity
            if (dragVec.magnitude > maxDragDistance) dragVec = dragVec.normalized * maxDragDistance; // clamp pull

            Vector3 flatDir = new Vector3(dragVec.x, 0f, dragVec.z); // ground direction
            Vector3 lofted = ApplyLoft(flatDir, loftAngleDeg); // tilt by loft

            Vector3 impulse = lofted * power; // force to apply
            FireShot(impulse); // shoot
            lr.positionCount = 0; // clear
        }
    }

    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled; // set gate
        if (!enabled) lr.positionCount = 0; // hide line
    }


    private void DrawTrajectoryPreview()
    {
        if (!isDragging || !canShoot) { lr.positionCount = 0; return; }
        if (!TryGetMouseWorldOnAimPlane(out Vector3 aimWorld)) { lr.positionCount = 0; return; }

        Vector3 dragVec = dragStartWorld - aimWorld; // pull vector
        float sensitivity = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? fineAimScale : 1f; // fine aim
        dragVec *= sensitivity; // scale
        if (dragVec.magnitude > maxDragDistance) dragVec = dragVec.normalized * maxDragDistance; // clamp

        Vector3 flatDir = new Vector3(dragVec.x, 0f, dragVec.z); // ground direction
        float pullFrac = Mathf.Clamp01(dragVec.magnitude / maxDragDistance); // 0..1

        // branch: flat line when loft is 0°, arc otherwise
        if (loftAngleDeg <= 0.0001f)
        {
            DrawFlatPreview(flatDir, pullFrac); // straight ground line
        }
        else
        {
            DrawArcPreview(flatDir, pullFrac); // ballistic arc
        }
    }

    private void DrawFlatPreview(Vector3 flatDir, float pullFrac)
    {
        if (flatDir.sqrMagnitude < 1e-6f) { lr.positionCount = 0; return; } // no direction
        Vector3 dir = flatDir.normalized; // unit ground dir
        float dist = Mathf.Lerp(0.5f, flatPreviewMaxDistance, pullFrac); // line length
        Vector3 startPos = transform.position; // start
        lr.positionCount = trajectoryPoints; // points count

        for (int i = 0; i < trajectoryPoints; i++)
        {
            float t = (trajectoryPoints == 1) ? 1f : (i / (float)(trajectoryPoints - 1)); // 0..1
            Vector3 p = startPos + dir * (dist * t); // straight line
            p.y = startPos.y; // stay at ground height
            lr.SetPosition(i, p); // set
        }
    }

    private void DrawArcPreview(Vector3 flatDir, float pullFrac)
    {
        Vector3 lofted = ApplyLoft(flatDir, loftAngleDeg); // tilt up
        Vector3 startPos = transform.position; // start
        Vector3 startVel = rb.linearVelocity; // current vel
        Vector3 addedVel = lofted * power / rb.mass; // launch vel
        Vector3 v0 = startVel + addedVel; // total start vel

        lr.positionCount = trajectoryPoints; // points count
        Vector3 g = Physics.gravity; // gravity

        for (int i = 0; i < trajectoryPoints; i++)
        {
            float t = i * timeStep; // time
            Vector3 p = startPos + v0 * t + 0.5f * g * t * t; // s = ut + 1/2at^2
            lr.SetPosition(i, p); // set
        }
    }

    private void FireShot(Vector3 impulse)
    {
        rb.AddForce(impulse, ForceMode.Impulse); // apply force
        canShoot = false; // wait to stop
        stoppedTimer = 0f; // reset timer
        if (shootSfx != null) shootSfx.Play(); // sfx
    }

    private bool TryGetMouseWorldOnAimPlane(out Vector3 world)
    {
        aimPlane = new Plane(Vector3.up, new Vector3(0f, transform.position.y, 0f)); // refresh plane
        Ray ray = cam.ScreenPointToRay(Input.mousePosition); // mouse ray
        if (aimPlane.Raycast(ray, out float enter)) { world = ray.GetPoint(enter); return true; } // hit point
        world = Vector3.zero; return false; // miss
    }

    private Vector3 ApplyLoft(Vector3 flatVec, float angleDeg)
    {
        if (flatVec.sqrMagnitude < 1e-6f) return flatVec; // no dir
        Vector3 dir = flatVec.normalized; // unit dir
        float mag = flatVec.magnitude; // keep strength
        Vector3 right = Vector3.Cross(dir, Vector3.up); // tilt axis
        Vector3 tiltedDir = (angleDeg <= 0.0001f) ? dir : Quaternion.AngleAxis(angleDeg, right) * dir; // tilt or not
        return tiltedDir * mag; // result
    }

    public void StopBall()
    {
        rb.linearVelocity = Vector3.zero; // stop move
        rb.angularVelocity = Vector3.zero; // stop spin
        stoppedTimer = stopSettleTime; // mark stopped
        canShoot = true; // ready
    }

    public void ApplyExternalMultipliers(float speedMult, float sizeMult)
    {
        if (basePower <= 0.0001f) basePower = Mathf.Max(0.0001f, power); // safety
        power = basePower * Mathf.Max(0.01f, speedMult); // never zero
                                                         // size is handled by PowerupManager via transform.localScale
    }
}
