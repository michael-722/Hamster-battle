using UnityEngine;
using TMPro;
using Unity.Netcode;
using UnityEngine.UI;

public class LobbySetUp : NetworkBehaviour
{
    public static LobbySetUp instance;

    private NetworkVariable<bool> canStartGame = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private bool gameStarted = false;

    [SerializeField] private TMP_InputField lobbyCode;
    [SerializeField] private TMP_Text joinCode;

    [SerializeField] private GameObject hostStartedGameUI;
    [SerializeField] private GameObject LobbyUI;

    [SerializeField] private GameObject startGameBtn;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void Start()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += ConnectionApprovalCheck;
    }

    private void ConnectionApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        if (gameStarted)
        {
            response.Approved = false;
            response.CreatePlayerObject = false;
        }
        else
        {
            response.Approved = true;
            response.CreatePlayerObject = true;
        }
    }

    public void onHostGamePressed()
    {
        Relay.instance.StartHost();
    }

    public void setJoinCode(string code)
    {
        joinCode.text = ("Join code: " + code);
    }

    public void onStartGamePressed()
    {
        if (IsServer)
        {
            gameStarted = true;

            syncedCountdown.instance.StartCountdown();
            onStartPressedRpc();
        }
    }

    [Rpc(SendTo.NotServer)]
    private void onStartPressedRpc()
    {
        syncedCountdown.instance.StartCountdown();

        startGameBtn.SetActive(true);
        startGameBtn.GetComponent<Button>().onClick.Invoke();
        startGameBtn.SetActive(false);
    }

    public void OnSingleplayerPressed()
    {
        syncedCountdown.instance.StartCountdown();
        canStartGame.Value = true;
    }

    public void onJoinGamePressed()
    {
        Relay.instance.JoinGame(lobbyCode.text);
    }

    public bool canStart()
    {
        return canStartGame.Value;
    }

    [Rpc(SendTo.Server)]
    public void allowStartRpc()
    {
        canStartGame.Value = true;
    }

    public void gameAlreadyStated()
    { 
        LobbyUI.SetActive(false);
        hostStartedGameUI.SetActive(true);
    }
}