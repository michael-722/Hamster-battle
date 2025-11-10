using System;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using UnityEditor;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager instance;

    [SerializeField] private GameObject leaderboardUI;
    [SerializeField] private GameObject submitUI;
    [SerializeField] private Transform scrollviewContent;
    [SerializeField] private Transform entryPrefab;

    [SerializeField] private TMP_Text completeTimeTxt;
    [SerializeField] private TMP_InputField userEnteredName;

    private string leaderboardID = "FastestTimes";

    private bool isSigningIn = false;
    private bool hasSubmittedTime = false;
    private bool refresh = true;

    private double completedTime;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private async void Start()
    {
        leaderboardUI.SetActive(false);
        submitUI.SetActive(false);

        await UnityServices.InitializeAsync();
        await Task.Delay(1000);
        await TrySignIn();

        //await LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardID, 105);
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
        /* //testing input for the leaderboard
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
        */

        if (Input.GetKeyDown(KeyCode.Return) && submitUI.activeSelf) OnSubmitBtn();

        if (leaderboardUI.activeSelf && refresh)
        {
            if (!AuthenticationService.Instance.IsSignedIn) return;
            refresh = false;
            refreshLeaderboard();
        }
    }

    private async void refreshLeaderboard()
    {
        LeaderboardScoresPage leaderboardScorePage = await LeaderboardsService.Instance.GetScoresAsync(leaderboardID, new GetScoresOptions() { Limit = 100 });

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
        //Debug.Log("refreshing leaderboard");
        await Task.Delay(1000);

        refresh = true;
    }

    public void OnSubmitBtn()
    {
        //add check if username has spaces - not valid
        SubmitScore();
        hasSubmittedTime = true;

        submitUI.SetActive(false);
        leaderboardUI.SetActive(true);
    }

    public async void SubmitScore()
    {
        int timeMS = (int)(completedTime * 1000);
        //Debug.Log("submitting time: " + timeMS + " for user: " + userEnteredName.text);
        await AuthenticationService.Instance.UpdatePlayerNameAsync(userEnteredName.text);
        await LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardID, timeMS);

        await Task.Delay(250);

        AuthenticationService.Instance.SignOut();
        AuthenticationService.Instance.ClearSessionToken();
        await TrySignIn();
    }

    public void setTime(double time)
    {
        completedTime = time;
        int timeMS = (int)(time * 1000);
        TimeSpan timeConverted = TimeSpan.FromMilliseconds((int)(timeMS));
        submitUI.SetActive(true);
        completeTimeTxt.GetComponent<TMPro.TextMeshProUGUI>().text = "Your time was: " + timeConverted.ToString(@"mm\:ss\.ff");

        //Debug.Log("Your time was: " + timeConverted.ToString(@"mm\:ss\.ff"));
    }

    public void OnLeaveGameBtnPressed()
    {
        LobbySetUp.instance.dissconnect();
        LobbySetUp.instance.showStartUI();
    }

    public void OnPlayAgainPressed()
    {
        if (NetworkManager.Singleton.LocalClientId == 0)
        {
            //Debug.Log("host pressed play again");
            LobbySetUp.instance.showHostPlayAgainUI();
        }
        else
        {   
            //Debug.Log("client pressed play again");
            LobbySetUp.instance.showClientPlayAgainUI();
        }
    }
}