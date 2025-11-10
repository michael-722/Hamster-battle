using TMPro;
using Unity.Netcode;
using UnityEngine;

// Simple HUD: result, rank, and countdown
public class RaceHUD : MonoBehaviour
{
    public TextMeshProUGUI resultText;    // WIN / LOSE / DNF
    public TextMeshProUGUI placeText;     // 1st / 2nd / —
    public TextMeshProUGUI countdownText; // Time Left: X.Xs

    public static RaceHUD Instance;

    private PlayerRace local; // local player
    private bool finished = false;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        resultText.text = "";
        placeText.text = "";
        countdownText.text = "";
    }

    private void OnDestroy()
    {
        if (local != null)
            local.OnResultChanged -= OnResultChanged;
    }

    public void ResetHUD()
    {
        resultText.text = "";
        placeText.text = "";
        countdownText.text = "";

        if (local != null)
        {
            local.OnResultChanged -= OnResultChanged;
            local = null;
        }

        finished = false;
    }

    public void SetFinished(bool val) => finished = val;

    private void Update()
    {
        // fallback check: sometimes HUD is enabled before player binds
        if (local == null)
        {
            foreach (var pr in FindObjectsOfType<PlayerRace>())
            {
                if (pr.IsOwner)
                {
                    BindLocalPlayer(pr);
                    break;
                }
            }
        }

        var rm = RaceManager.Instance;
        if (rm != null && rm.IsWindowOpen() && !finished)
            countdownText.text = $"Time Left: {rm.TimeLeft():0.0}s";
        else
            countdownText.text = "";
    }

    private void OnResultChanged(RaceResult r, int rank)
    {
        resultText.text = r switch
        {
            RaceResult.Win => "WIN",
            RaceResult.LoseEscaped => "Escaped",
            RaceResult.LoseStuck => "DNF",
            _ => ""
        };

        placeText.text = rank > 0 ? Ordinal(rank) : "—";
    }

    private string Ordinal(int n)
    {
        int a = n % 100;
        if (a is >= 11 and <= 13) return n + "th";
        return (n % 10) switch
        {
            1 => n + "st",
            2 => n + "nd",
            3 => n + "rd",
            _ => n + "th"
        };
    }

    public void BindLocalPlayer(PlayerRace pr)
    {
        if (local == pr) return;
        if (local != null) local.OnResultChanged -= OnResultChanged;

        local = pr;

        if (local != null)
        {
            local.OnResultChanged += OnResultChanged;

            if (local.Result != RaceResult.None) OnResultChanged(local.Result, local.Rank);
        }

        finished = false;
    }
}
