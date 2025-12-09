using Unity.Netcode;
using UnityEngine;

// Per-player race state + input freeze
public class PlayerRace : NetworkBehaviour
{
    public static PlayerRace instance;

    public RaceResult Result { get; private set; } = RaceResult.None; // outcome
    public int Rank { get; private set; } = 0; // 1=first, 2=second, 0=unknown
    public System.Action<RaceResult, int> OnResultChanged; // HUD hook

    private HamsterBallController ctrl; // input/ball

    private double startTime;
    private double endTime;
    private double completeTime;

    private void Awake()  
    {
        instance = this;

        ctrl = GetComponent<HamsterBallController>();
    }

    public double GetCompleteTime()
    {
        return completeTime;
    }

    public void HitGoal() // called by GoalTrigger
    {
        endRaceTimerRpc();//stops timer
        var nm = NetworkManager.Singleton;
        if (nm != null && nm.IsListening && IsSpawned)
        {
            RaceManager.Instance.RegisterFinishServerRpc(OwnerClientId);
            gameObject.transform.localScale = new Vector3(0, 0, 0);
        }
        else
        {
            LeaderboardManager.instance.setTime(completeTime);
            //RaceManager.Instance.RegisterFinishOffline(this);
            Destroy(this.gameObject);
        }
    }

    [Rpc(SendTo.Owner)]
    public void startRaceTimerRpc()
    {
        startTime = Time.time;
        //Debug.Log("timer started " + startTime);
    }

    [Rpc(SendTo.Owner)]
    public void endRaceTimerRpc()
    {
        endTime = Time.time;
        completeTime = endTime - startTime;
        //Debug.Log("complete time was: " + completeTime);
        LeaderboardManager.instance.setTime(completeTime);

        ctrl.SetCanMove(false);
        //Debug.Log("timer ended " + endTime);
    }

    public void SetResultLocal(RaceResult r, int rank)
    {
        if (Result != RaceResult.None) return; // only once
        if (r == RaceResult.LoseStuck) LeaderboardManager.instance.OnShowDNFUI();//call dnf ui
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

    [ClientRpc]
    public void ShowPowerupActivatedClientRpc(string powerupId, float duration, ulong pickerId)
    {
        string who;

        if (ctrl != null && !string.IsNullOrEmpty(ctrl.PlayerColorName))
            who = $"{ctrl.PlayerColorName} player";
        else
            who = $"Player {pickerId}";

        string msg = $"{who} has activated {powerupId} for {duration:0.#} seconds";
        NotificationUI.Instance?.Show(msg, duration);
    }

    public void ResetPlayer()
    {
        Result = RaceResult.None;
        Rank = 0;
        startTime = 0;
        endTime = 0;
        completeTime = 0;

        if (ctrl != null)
        {
            ctrl.StopBall();
            ctrl.SetInputEnabled(true);
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner) StartCoroutine(BindHUDNextFrame());
    }

    private System.Collections.IEnumerator BindHUDNextFrame()
    {
        yield return null;

        if (RaceHUD.Instance != null) RaceHUD.Instance.BindLocalPlayer(this);
    }
}
