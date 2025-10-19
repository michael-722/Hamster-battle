using UnityEngine;
using Unity.Services.Core;
using System.Threading.Tasks;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using System.Collections;
using Unity.Services.Authentication;

public class LeaderboardManagerTest : MonoBehaviour
{
    [SerializeField] private GameObject leaderboardCanvas;
    [SerializeField] private Transform scrollviewContent;
    [SerializeField] private Transform entryPrefab;

    private string leaderboardID = "FastestTimes";

    private bool isSigningIn = false;

    private async void Start()
    {
        await UnityServices.InitializeAsync();
        await Task.Delay(1000);
        await TrySignIn();
        //await LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardID, 0);
        SubmitScore(75.345, "user");
    }

    private async Task TrySignIn()
    {
        if (AuthenticationService.Instance.IsSignedIn || isSigningIn) return;

        isSigningIn = true;
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        catch (AuthenticationException e)
        {
            Debug.LogError("sign in failed:" + e);
        }
        finally
        {
            isSigningIn = false;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            leaderboardCanvas.SetActive(!leaderboardCanvas.activeSelf);
            if (leaderboardCanvas.activeSelf == true) refreshLeaderboard();
        }
    }

    private async void refreshLeaderboard()
    {
        while (Application.isPlaying && leaderboardCanvas.activeSelf)
        {
            LeaderboardScoresPage leaderboardScorePage = await LeaderboardsService.Instance.GetScoresAsync(leaderboardID);

            foreach (Transform t in scrollviewContent) Destroy(t.gameObject);

            foreach (LeaderboardEntry e in leaderboardScorePage.Results)
            { 
                Transform entryTransform = Instantiate(entryPrefab, scrollviewContent);
                entryTransform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = e.Rank.ToString();
                entryTransform.GetChild(2).GetComponent<TMPro.TextMeshProUGUI>().text = e.PlayerName;
                entryTransform.GetChild(3).GetComponent<TMPro.TextMeshProUGUI>().text = e.Score.ToString();
            }

            await Task.Delay(500);
        }
    }

    public async void SubmitScore(double time, string userName)
    {
        Debug.Log("submitting score: " + time + " for user: " + userName);
        await AuthenticationService.Instance.UpdatePlayerNameAsync(userName);
        await LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardID, time);
    }
}
