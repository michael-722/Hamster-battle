using UnityEngine;

// Grants a powerup when the ball touches it
[RequireComponent(typeof(Collider))]
public class PowerupPickup : MonoBehaviour
{
    public PowerupDef powerup; // which powerup

    private void Reset()
    {
        var col = GetComponent<Collider>(); // ensure trigger
        col.isTrigger = true; // trigger mode
    }

    private void OnTriggerEnter(Collider other)
    {
        var mgr = other.GetComponentInParent<PowerupManager>(); // find manager
        if (mgr == null) return; // not a player
        mgr.Give(powerup); // give powerup
        Destroy(gameObject); // consume
    }
}
