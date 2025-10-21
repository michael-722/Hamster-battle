using Unity.Netcode;
using UnityEngine;
using static PowerupDef;

public static class PowerupService
{
   
    public static void Apply(PowerupDef def, PlayerRace picker, Vector3 center)
    {
        if (def == null || picker == null) return; // safety

        var nm = NetworkManager.Singleton;
        bool onlineServer = (nm != null && nm.IsListening && nm.IsServer);

        if (onlineServer) ApplyOnlineServer(def, picker, center);
        else ApplyOffline(def, picker, center);
    }

    // ---------- offline ----------
    private static void ApplyOffline(PowerupDef def, PlayerRace picker, Vector3 center)
    {
        foreach (var pr in Object.FindObjectsOfType<PlayerRace>())
        {
            if (!IsTarget(def, picker, pr, center)) continue;
            pr.GetComponent<PowerupManager>()?.Give(def); // local apply
        }
    }

    // ---------- server authoritative ----------
    private static void ApplyOnlineServer(PowerupDef def, PlayerRace picker, Vector3 center)
    {
        Debug.Log($"[PowerupService] SERVER applying '{def.id}' scope={def.scope} center={center}");

        foreach (var pr in Object.FindObjectsOfType<PlayerRace>())
        {
            if (!pr.IsSpawned) continue;
            if (!IsTarget(def, picker, pr, center)) continue;

            var to = new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = new[] { pr.OwnerClientId } }
            };
            pr.ApplyPowerUpByIdClientRpc(def.id, to); // client resolves & applies
        }
    }

    // shared targeting rules (Self / OthersInRadius / AllInRadius)
    private static bool IsTarget(PowerupDef def, PlayerRace picker, PlayerRace candidate, Vector3 center)
    {
        switch (def.scope)
        {
            case PowerupScope.Self:
                return candidate == picker;

            case PowerupScope.OthersInRadius:
                if (candidate == picker) return false;
                return Vector3.Distance(candidate.transform.position, center) <= def.aoeRadius;

            case PowerupScope.AllInRadius:
                return Vector3.Distance(candidate.transform.position, center) <= def.aoeRadius;
        }
        return false;
    }
}