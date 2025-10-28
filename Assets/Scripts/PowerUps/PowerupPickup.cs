using Unity.Netcode;
using UnityEngine;
using static PowerupDef;

public class PowerupPickup : NetworkBehaviour
{
    public PowerupDef powerup;                
    private bool consumed = false;

    private void OnTriggerEnter(Collider other)
    {
        if (consumed) return;

        var picker = other.GetComponentInParent<PlayerRace>();
        if (!picker) return;

        // Choose AoE center (player vs pickup)
        var center = powerup.centredOnPicker ? picker.transform.position : transform.position;

        var nm = NetworkManager.Singleton;
        bool online = nm != null && nm.IsListening;

        if (!online)
        {
            // OFFLINE: apply locally and destroy
            PowerupService.Apply(powerup, picker, center);
            ConsumeLocal();
            return;
        }

        // ONLINE:
        if (IsServer)
        {
            // Host picks up -> server path already... apply immediately
            PowerupService.Apply(powerup, picker, center);
            ConsumeNetwork();
        }
        else
        {
            // Client picks up -> ask the server to validate & apply
            RequestConsumeServerRpc(picker.OwnerClientId, center);
        }
    }

    public override void OnNetworkSpawn()
    {
        // When this pickup appears on a client, make sure its SO is known by id
        if (powerup != null) PowerupManager.RegisterDef(powerup);
    }

    // Client -> Server: consume this pickup for this picker at this center
    [ServerRpc(RequireOwnership = false)]
    private void RequestConsumeServerRpc(ulong pickerClientId, Vector3 center)
    {
        if (consumed) return;

        // find the server side PlayerRace for this client
        PlayerRace picker = null;
        foreach (var pr in FindObjectsOfType<PlayerRace>())
            if (pr.IsSpawned && pr.OwnerClientId == pickerClientId) { picker = pr; break; }
        if (picker == null) return;

        // validate they are close enough to the pickup
        if (Vector3.Distance(picker.transform.position, transform.position) > powerup.aoeRadius + 2f
            && powerup.scope != PowerupScope.Self)
            return;

        PowerupService.Apply(powerup, picker, center);
        ConsumeNetwork();
    }

    private void ConsumeLocal()
    {
        if (consumed) return;
        consumed = true;
        Destroy(gameObject);
    }

    private void ConsumeNetwork()
    {
        if (consumed) return;
        consumed = true;

        var no = GetComponent<NetworkObject>();
        if (no && no.IsSpawned)
        {
            
            no.Despawn(true); // <-- always destroy spawned prefab
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
