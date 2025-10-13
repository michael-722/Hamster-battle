using TMPro;
using UnityEngine;

// Simple HUD: result, rank, and countdown
public class RaceHUD : MonoBehaviour
{
    public TextMeshProUGUI resultText;    // WIN / LOSE / DNF
    public TextMeshProUGUI placeText;     // 1st / 2nd / —
    public TextMeshProUGUI countdownText; // Time Left: X.Xs

    private PlayerRace local; // local player

    private void OnEnable()
    {
        resultText.text = "";
        placeText.text = "";
        countdownText.text = "";
    }

    private void Update()
    {
        // late-bind: find local player after spawn
        if (local == null)
        {
            foreach (var pr in FindObjectsOfType<PlayerRace>())
                if (pr.IsOwner) { local = pr; break; }

            if (local != null)
            {
                local.OnResultChanged += OnResultChanged; // subscribe
                // if result already decided, show it now
                if (local.Result != RaceResult.None)
                    OnResultChanged(local.Result, local.Rank);
            }
        }

        var rm = RaceManager.Instance;
        if (rm != null && rm.IsWindowOpen())
            countdownText.text = $"Time Left: {rm.TimeLeft():0.0}s";
        else
            countdownText.text = "";
    }

    private void OnResultChanged(RaceResult r, int rank)
    {
        resultText.text = r switch
        {
            RaceResult.Win => "WIN",
            RaceResult.LoseEscaped => "LOSE (Escaped)",
            RaceResult.LoseStuck => "DNF",
            _ => ""
        };

        placeText.text = rank > 0 ? Ordinal(rank) : "—";
    }

    private string Ordinal(int n)
    {
        int a = n % 100;
        if (a is >= 11 and <= 13) return n + "th";
        return (n % 10) switch { 1 => n + "st", 2 => n + "nd", 3 => n + "rd", _ => n + "th" };
    }
}
