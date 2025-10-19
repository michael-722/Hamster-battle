using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using UnityEngine;
using TMPro;

public class LeaderboardManagerTest : MonoBehaviour
{
    [SerializeField] private GameObject leaderboardUI;
    [SerializeField] private GameObject submitUI;
    [SerializeField] private Transform scrollviewContent;
    [SerializeField] private Transform entryPrefab;

    [SerializeField] private TMP_InputField userEnteredName;

    private string leaderboardID = "FastestTimes";

    private bool isSigningIn = false;
    private bool hasSubmittedTime = false;
    private bool refresh = true;

    private async void Start()
    {
        leaderboardUI.SetActive(false);
        submitUI.SetActive(false);

        await UnityServices.InitializeAsync();
        await Task.Delay(1000);
        await TrySignIn();

        //await LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardID, 105);
        //SubmitScore(75.53, "user");
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
            Debug.Log("sign in failed:" + e);
        }
        finally
        {
            isSigningIn = false;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L) && !hasSubmittedTime)
        {
            submitUI.SetActive(true);
        }
        else if (Input.GetKeyDown(KeyCode.L))
        {
            hasSubmittedTime = false;
            submitUI.SetActive(false);
            leaderboardUI.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.Return) && submitUI.activeSelf) OnSubmitBtn();

        if (leaderboardUI.activeSelf && refresh)
        {
            refresh = false;
            refreshLeaderboard();
        }
    }

    private async void refreshLeaderboard()
    {
        //Debug.Log("refreshing leaderboard");
        await Task.Delay(1000);

        LeaderboardScoresPage leaderboardScorePage = await LeaderboardsService.Instance.GetScoresAsync(leaderboardID);

        foreach (Transform t in scrollviewContent) Destroy(t.gameObject);

        foreach (LeaderboardEntry e in leaderboardScorePage.Results)
        {
            Transform entryTransform = Instantiate(entryPrefab, scrollviewContent);
            entryTransform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = (e.Rank + 1).ToString();
            entryTransform.GetChild(2).GetComponent<TMPro.TextMeshProUGUI>().text = e.PlayerName;

            TimeSpan timeConverted = TimeSpan.FromMilliseconds((int)(e.Score));
            string formattedTime = timeConverted.ToString(@"mm\:ss\.ff");
            entryTransform.GetChild(3).GetComponent<TMPro.TextMeshProUGUI>().text = formattedTime;

            //Debug.Log("rank obj:" + entryTransform.GetChild(1).name);
            //Debug.Log("name obj:" + entryTransform.GetChild(2).name);
            //Debug.Log("score obj:" + entryTransform.GetChild(3).name);
        }
        refresh = true;
    }

    public void OnSubmitBtn()
    {
        //add check if username has spaces - not valid

        SubmitScore(75.53, userEnteredName.text);
        hasSubmittedTime = true;

        submitUI.SetActive(false);
        leaderboardUI.SetActive(true);
    }

    public async void SubmitScore(double time, string userName)
    {
        int timeMS = (int)(time * 1000);
        //Debug.Log("submitting time: " + time + " for user: " + userName);
        await AuthenticationService.Instance.UpdatePlayerNameAsync(userName);
        await LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardID, timeMS, new AddPlayerScoreOptions());
    }
}
