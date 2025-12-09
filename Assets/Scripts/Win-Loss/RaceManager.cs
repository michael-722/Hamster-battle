using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// Server authority for finish order + 15s window (works offline too (ish))
public class RaceManager : NetworkBehaviour
{
    public static RaceManager Instance; // singleton

    [Header("Settings")]
    public float finishWindow = 15f; // seconds after first finish

    // network state for countdown
    public NetworkVariable<bool> windowOpen = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> windowEndServerTime = new(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private readonly List<ulong> finishOrder = new(); // online order by clientId
    private bool closed = false; // window closed

    // offline state
    private float offlineWindowEnd = 0f; // local end time
    private readonly List<PlayerRace> offlineFinishOrder = new(); // offline rank

    private void Awake()
    {
        Instance = this; // set singleton
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }

    public bool HasGameBeenFinished() => windowOpen.Value; // for HUD

    public float TimeLeft() // seconds remaining
    {
        if (!windowOpen.Value) return 0f;
        double nowD = IsServer
            ? NetworkManager.ServerTime.Time
            : (NetworkManager.Singleton != null ? NetworkManager.Singleton.ServerTime.Time : 0.0);
        float now = (float)nowD; // cast
        return Mathf.Max(0f, windowEndServerTime.Value - now);
    }

    // -------- ONLINE (server authoritative) --------

    [ServerRpc(RequireOwnership = false)]
    public void RegisterFinishServerRpc(ulong playerId)
    {
        if (closed) return;

        bool first = finishOrder.Count == 0;
        if (!finishOrder.Contains(playerId)) finishOrder.Add(playerId);
        int rank = finishOrder.IndexOf(playerId) + 1; // 1-based

        if (first)
        {
            windowOpen.Value = true;
            windowEndServerTime.Value = (float)NetworkManager.ServerTime.Time + finishWindow;
            StartCoroutine(CloseWindowWhenDue());
            SendResultClientRpc(playerId, RaceResult.Win, rank);
        }
        else
        {
            if (NetworkManager.ServerTime.Time <= windowEndServerTime.Value) SendResultClientRpc(playerId, RaceResult.LoseEscaped, rank);
            else SendResultClientRpc(playerId, RaceResult.LoseStuck, 0);
        }
    }

    private IEnumerator CloseWindowWhenDue()
    {
        while ((float)NetworkManager.ServerTime.Time < windowEndServerTime.Value)
        {
            //Debug.Log("time left: " + TimeLeft());
            yield return null;
        }
        CloseAndDNF();
    }

    private void CloseAndDNF()
    {
        if (closed) return;
        closed = true;
        windowOpen.Value = false;

        foreach (var kv in NetworkManager.ConnectedClientsList)
        {
            ulong id = kv.ClientId;
            if (!finishOrder.Contains(id))
                SendResultClientRpc(id, RaceResult.LoseStuck, 0); // DNF, rank unknown
        }
    }

    // send result to a specific client
    [ClientRpc]
    private void SendResultClientRpc(ulong targetClientId, RaceResult result, int rank)
    {
        if (NetworkManager.Singleton == null) return;
        if (NetworkManager.Singleton.LocalClientId != targetClientId) return;

        var pr = FindLocalPlayerRace();
        if (pr != null) pr.SetResultLocal(result, rank);
        RaceHUD.Instance.SetFinished(true);
    }

    private PlayerRace FindLocalPlayerRace()
    {
        foreach (var pr in FindObjectsOfType<PlayerRace>())
            if (pr.IsOwner) return pr;
        return null;
    }

    // -------- OFFLINE SUPPORT (with ranks) --------

    public void RegisterFinishOffline(PlayerRace pr)
    {
        //LeaderboardManager.instance.OnShowSinglePlayerLeave();

        // first finisher
        if (!windowOpen.Value)
        {
            windowOpen.Value = true;
            offlineWindowEnd = Time.time + finishWindow;
            offlineFinishOrder.Clear();
            offlineFinishOrder.Add(pr);
            pr.SetResultLocal(RaceResult.Win, 1);
        }
        else
        {
            if (Time.time <= offlineWindowEnd)
            {
                if (!offlineFinishOrder.Contains(pr))
                    offlineFinishOrder.Add(pr);
                int rank = offlineFinishOrder.IndexOf(pr) + 1; // 1-based
                pr.SetResultLocal(RaceResult.LoseEscaped, rank);
            }
            else
            {
                pr.SetResultLocal(RaceResult.LoseStuck, 0);
            }
        }
    }

    private void Update()
    {
        // offline countdown close
        if (NetworkManager.Singleton == null && windowOpen.Value && Time.time >= offlineWindowEnd)
        {
            windowOpen.Value = false;
            closed = true;

            foreach (var pr in FindObjectsOfType<PlayerRace>())
            {
                if (pr.Result == RaceResult.None)
                    pr.SetResultLocal(RaceResult.LoseStuck, 0); // DNF
            }
        }
    }

    public void ResetRace()
    {
        StopAllCoroutines();

        closed = false;
        finishOrder.Clear();
        offlineFinishOrder.Clear();

        if (IsServer)
        {
            windowOpen.Value = false;
            windowEndServerTime.Value = 0f;
            offlineWindowEnd = 0f;
        }

        foreach (var pr in FindObjectsByType<PlayerRace>(FindObjectsSortMode.None))
        {
            pr.ResetPlayer();
        }
    }
}
