using TMPro;
using Unity.Netcode;
using UnityEngine;

public class CarMovement : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private Vector3 targetOffset;
    [SerializeField] private float reachThreshold = 0.1f;

    private Vector3 originPosition;
    private Vector3 targetPosition;
    private bool movingToTarget = true;

    void Start()
    {
        originPosition = transform.position;
        targetPosition = originPosition + targetOffset;
    }

    void Update()
    {
        if (movingToTarget)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            if (Vector3.Distance(transform.position, targetPosition) <= reachThreshold)
            {
                movingToTarget = false;
            }
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, originPosition, speed * Time.deltaTime);
            if (Vector3.Distance(transform.position, originPosition) <= reachThreshold)
            {
                movingToTarget = true;
            }
        }
    }

    void OnDrawGizmos()
    {
        Vector3 origin = Application.isPlaying ? originPosition : transform.position;
        Vector3 target = origin + targetOffset;

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(origin, 0.15f);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(target, 0.15f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(origin, target);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(target, reachThreshold);
    }
}