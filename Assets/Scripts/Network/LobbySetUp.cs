using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;

public class LobbySetUp : NetworkBehaviour
{
    public static LobbySetUp instance;

    private NetworkVariable<bool> canStartGame = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<bool> gameStarted = new(false);

    [SerializeField] private TMP_InputField lobbyCode;
    [SerializeField] private TMP_Text joinCode;

    [SerializeField] private GameObject hostingUI;

    [SerializeField] private GameObject UIRootObj;
    [SerializeField] private GameObject startUI;

    [SerializeField] private GameObject hostStartedGameUI;
    [SerializeField] private GameObject LobbyUI;

    [SerializeField] private GameObject hostLeftGameUI;
    [SerializeField] private GameObject failedJoinCodeUI;

    [SerializeField] private GameObject startGameBtn;

    [SerializeField] private TMP_Text connectedPlayersTxt;

    private int connectedPlayers = -1;
    private bool singlePlayer = false;

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
        NetworkManager.Singleton.OnClientConnectedCallback += OnHostCOnnected;
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
            if (clientId == 0)
            {
                startGameBtn.SetActive(false);
                return;//host dc
            }

            //Debug.Log("Client disconnected: " + clientId);
            connectedPlayers = -1;
            connectedPlayersTxt.text = "Players Connected: " + connectedPlayers;
            return;
        }

        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            var reason = NetworkManager.Singleton.DisconnectReason;

            if (reason.Contains("Game already started"))
            {
                LobbyUI.SetActive(false);
                hostStartedGameUI.SetActive(true);
            }
            else if (reason.Contains("host shutting down"))//server dc
            {
                LobbyUI.SetActive(false);
                hostLeftGameUI.SetActive(true);
            }
        }
    }

    private void OnHostCOnnected(ulong clientId)
    {
        if (!IsServer) return;
        startGameBtn.SetActive(true);
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

    public void failedToJoinGame()
    {
        failedJoinCodeUI.SetActive(true);
        LobbyUI.SetActive(false);
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
        canStartGame.Value = true;

        Vector3 spawnPos = GetSpawnPosition(NetworkManager.Singleton.LocalClientId);
        Quaternion spawnRot = Quaternion.identity;

        GameObject player = Instantiate(NetworkManager.Singleton.NetworkConfig.PlayerPrefab, spawnPos, spawnRot);
        //syncedCountdown.instance.StartCountdown();
        
    }

    public void onJoinGamePressed()
    {
        Relay.instance.JoinGame(lobbyCode.text);
    }

    public bool canStart()
    {
        if (singlePlayer) return true;
        else return canStartGame.Value;
    }

    public void setCanStart(bool val)
    {
        canStartGame.Value = val;
    }

    [Rpc(SendTo.Server)]
    public void allowStartRpc()
    {
        canStartGame.Value = true;
    }

    public void dissconnect()
    {
        NetworkManager.Singleton.Shutdown();
    }

    public void setSinglePlayer(bool state) { singlePlayer = state; }

    public void showStartUI()
    {
        UIRootObj.SetActive(true);
        startUI.SetActive(true);

        LobbyUI.SetActive(false);
    }

    public void showClientPlayAgainUI()
    {
        UIRootObj.SetActive(true);
        LobbyUI.SetActive(true);

        resetHUD();
    }

    public void showHostPlayAgainUI()
    {
        UIRootObj.SetActive(true);
        hostingUI.SetActive(true);

        canStartGame.Value = false;
        gameStarted.Value = false;

        resetPlayers();
        RaceManager.Instance?.ResetRace();
        resetHUD();
    }

    private void resetHUD()
    {
        RaceHUD.Instance.ResetHUD();
        RaceHUD.Instance.SetFinished(false);
    }

    private void resetPlayers()
    {
        if (!IsServer) return;

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var playerObject = client.PlayerObject;

            if (playerObject != null)
            {
                playerObject.Despawn(true);
                Destroy(playerObject.gameObject);

                Vector3 spawnPos = GetSpawnPosition(client.ClientId);
                Quaternion spawnRot = Quaternion.identity;

                GameObject newPlayer = Instantiate(NetworkManager.Singleton.NetworkConfig.PlayerPrefab, spawnPos, spawnRot);
                var newNetObj = newPlayer.GetComponent<NetworkObject>();
                newNetObj.SpawnAsPlayerObject(client.ClientId);
            }
        }
    }

    //spawn pos func is from the hamsterball controller need to put into manager script septrately=====================
    private Vector3 GetSpawnPosition(ulong clientId)
    {
        int id = (int)OwnerClientId;
        float spawnRadius = 7.5f;
        float angle = id * 90f;
        Vector3 offset = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0f, Mathf.Sin(angle * Mathf.Deg2Rad)) * spawnRadius;
        
        return new Vector3(offset.x, 5f, offset.z);
    }
}