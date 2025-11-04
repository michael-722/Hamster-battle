using Unity.Netcode;
using UnityEngine;

// Per-player race state + input freeze
public class PlayerRace : NetworkBehaviour
{
    public RaceResult Result { get; private set; } = RaceResult.None; // outcome
    public int Rank { get; private set; } = 0; // 1=first, 2=second, 0=unknown
    public System.Action<RaceResult, int> OnResultChanged; // HUD hook

    private HamsterBallController ctrl; // input/ball

    private double startTime;
    private double endTime;
    private double completeTime;

    private void Awake()  
    {
        ctrl = GetComponent<HamsterBallController>();
    }

    public void HitGoal() // called by GoalTrigger
    {
        endRaceTimer();//stops timer
        var nm = NetworkManager.Singleton;
        if (nm != null && nm.IsListening && IsSpawned)
            RaceManager.Instance.RegisterFinishServerRpc(OwnerClientId);
        else
            RaceManager.Instance.RegisterFinishOffline(this);
    }

    public void startRaceTimer()
    {
        startTime = Time.time;
    }

    public void endRaceTimer()
    {
        endTime = Time.time;
        completeTime = endTime - startTime;
        //Debug.Log("complete time was: " + completeTime);
        LeaderboardManager.instance.setTime(completeTime);
    }

    public void SetResultLocal(RaceResult r, int rank)
    {
        if (Result != RaceResult.None) return; // only once
        Result = r;
        Rank = rank;

        if (ctrl != null)
        {
            ctrl.StopBall();             // stop motion
            ctrl.SetInputEnabled(false); // disable input
        }

        OnResultChanged?.Invoke(Result, Rank); // notify HUD
    }
    [ClientRpc]
    public void ApplyPowerUpByIdClientRpc(string powerupId, ClientRpcParams rpcParams = default)
    {
        var mgr = GetComponent<PowerupManager>();
        if (mgr) mgr.GiveById(powerupId);
    }
}
