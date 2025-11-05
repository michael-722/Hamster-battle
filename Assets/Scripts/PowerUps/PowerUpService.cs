using System.Collections.Generic;
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
            ApplyPushbackImpulse(def, picker, pr, center);
        }

        BeginPushbackRepeater(def, picker, center);
    }

    // ---------- server authoritative ----------
    private static void ApplyOnlineServer(PowerupDef def, PlayerRace picker, Vector3 center)
    {
        Debug.Log($"[PowerupService] SERVER applying '{def.id}' scope={def.scope} center={center}");

        var targets = new List<PlayerRace>();
        foreach (var pr in Object.FindObjectsOfType<PlayerRace>())
        {
            if (!pr.IsSpawned) continue;
            if (!IsTarget(def, picker, pr, center)) continue;
            targets.Add(pr);

            var to = new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = new[] { pr.OwnerClientId } }
            };
            pr.ApplyPowerUpByIdClientRpc(def.id, to); // client resolves & applies

            ApplyPushbackImpulse(def, picker, pr, center);
        }

        BeginPushbackRepeater(def, picker, center);
    }

    // shared targeting rules (Self / OthersInRadius / AllInRadius)
    internal static bool IsTarget(PowerupDef def, PlayerRace picker, PlayerRace candidate, Vector3 center)
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

    private static void BeginPushbackRepeater(PowerupDef def, PlayerRace picker, Vector3 center)
    {
        if (def.pushBack <= 0f || def.duration <= 0f) return;

        var repeaterObject = new GameObject($"{def.id}_PushbackRepeater");
        repeaterObject.hideFlags = HideFlags.DontSave;
        repeaterObject.transform.position = center;

        var repeater = repeaterObject.AddComponent<PowerupPushbackRepeater>();
        repeater.Initialise(def, picker, center);
    }

    private static void ApplyPushbackImpulse(PowerupDef def, PlayerRace picker, PlayerRace target, Vector3 center)
    {
        if (def == null || target == null) return;
        if (def.pushBack <= 0f) return;

        var rb = target.GetComponent<Rigidbody>();
        if (rb == null) return;

        var origin = def.centredOnPicker && picker != null ? picker.transform.position : center;
        var dir = target.transform.position - origin;
        if (dir.sqrMagnitude < 0.0001f) return;

        rb.AddForce(dir.normalized * def.pushBack, ForceMode.Impulse);
    }
}
