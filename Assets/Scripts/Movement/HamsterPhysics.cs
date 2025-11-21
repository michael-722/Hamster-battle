using UnityEngine;

public class HamsterPhysics : MonoBehaviour
{
    [SerializeField] private Transform hamsterCenter;
    [SerializeField] private Rigidbody ballRigidbody;

    [SerializeField] private float runTiltStrength = 5f;//climb height inside ball
    [SerializeField] private float gravityAlignSpeed = 10f;
    [SerializeField] private float turnSpeed = 10f;

    [SerializeField] private float pinThreshold = 15f;
    [SerializeField] private float unpinSpeed = 5f;

    private Vector3 lastMoveDir = Vector3.forward;
    private float currentPinFactor = 0f;//0 = gravity, 1 = pinned

    private void FixedUpdate()
    {
        if (ballRigidbody == null) return;

        float angularSpeed = ballRigidbody.angularVelocity.magnitude;
        float linearSpeed = ballRigidbody.linearVelocity.magnitude;

        // look dir
        Vector3 flatVelocity = ballRigidbody.linearVelocity;
        flatVelocity.y = 0;

        if (flatVelocity.sqrMagnitude > 0.1f)
        {
            lastMoveDir = flatVelocity.normalized;
        }

        //face move dir
        Quaternion targetLookRot = Quaternion.LookRotation(lastMoveDir, Vector3.up);

        //calc run tilt
        float climbAngle = -Mathf.Clamp(linearSpeed * runTiltStrength, 0f, 45f);
        Quaternion climbRot = Quaternion.Euler(climbAngle, 0, 0);

        //look + tilt
        Quaternion desiredWorldRotation = targetLookRot * climbRot;


        //calc pin factor
        //map to 0-1 range - (a val, b val, current val) returns where current val is between a and b
        float targetPin = Mathf.InverseLerp(pinThreshold * 0.5f, pinThreshold, angularSpeed);

        currentPinFactor = Mathf.Lerp(currentPinFactor, targetPin, Time.fixedDeltaTime * unpinSpeed);

        //0 = force upright
        //1 = let parent handles rot
        if (currentPinFactor < 0.99f)
        {
            Quaternion pinnedRotation = hamsterCenter.rotation;
            Quaternion finalRot = Quaternion.Slerp(desiredWorldRotation, pinnedRotation, currentPinFactor);

            hamsterCenter.rotation = Quaternion.Slerp(hamsterCenter.rotation, finalRot, Time.fixedDeltaTime * gravityAlignSpeed);
        }
    }
}