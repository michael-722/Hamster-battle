using UnityEngine;
using TMPro;
using Unity.Netcode;
using UnityEngine.UI;

public class LobbySetUp : NetworkBehaviour
{
    public static LobbySetUp instance;

    private NetworkVariable<bool> canStartGame = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<bool> gameStarted = new(false);

    [SerializeField] private TMP_InputField lobbyCode;
    [SerializeField] private TMP_Text joinCode;

    [SerializeField] private GameObject hostStartedGameUI;
    [SerializeField] private GameObject LobbyUI;

    [SerializeField] private GameObject startGameBtn;

    [SerializeField] private TMP_Text connectedPlayersTxt;

    private int connectedPlayers = -1;

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
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void ConnectionApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        if (gameStarted.Value)
        {
            response.Approved = false;
            response.CreatePlayerObject = false;
            response.Reason = "Game already started";

            //Debug.Log("Connection denied: Game already started.");
        }
        else
        {
            response.Approved = true;
            response.CreatePlayerObject = true;
            //Debug.Log("Connection approved.");

            connectedPlayers++;
            connectedPlayersTxt.text = "Players Connected: " + connectedPlayers;
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (IsServer)
        {
            connectedPlayers--;
            connectedPlayersTxt.text = "Players Connected: " + connectedPlayers;
            return;
        }

        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            var reason = NetworkManager.Singleton.DisconnectReason;
            Debug.Log("Disconnected: " + reason);

            if (reason.Contains("Game already started"))
            {
                LobbyUI.SetActive(false);
                hostStartedGameUI.SetActive(true);
            }
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
            gameStarted.Value = true;

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
}