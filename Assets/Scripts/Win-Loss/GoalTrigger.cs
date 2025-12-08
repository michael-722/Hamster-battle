using UnityEngine;

// Detects player reaching the exit
[RequireComponent(typeof(Collider))]
public class GoalTrigger : MonoBehaviour
{
    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true; // ensure trigger
    }

    private void OnTriggerEnter(Collider other)
    {
        var pr = other.GetComponentInParent<PlayerRace>(); // find player
        if (pr == null) return; // not a player
        pr.HitGoal(); // submit finish
    }
}
