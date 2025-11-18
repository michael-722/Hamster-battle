using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbySetUp : NetworkBehaviour
{
    public static LobbySetUp instance;

    private NetworkVariable<bool> canStartGame = new(false);
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

    private bool hasHostDC = false;

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

            response.Position = GetSpawnPosition((ulong)connectedPlayers);
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (clientId == 0)//host dc
        {
            startGameBtn.SetActive(false);
            connectedPlayers = -1;
            return;
        }

        if (IsServer)
        {
            //Debug.Log("Client disconnected: " + clientId);
            connectedPlayers--;
            connectedPlayersTxt.text = "Players Connected: " + connectedPlayers;
            return;
        }

        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            //Debug.Log("Client disconnected from server. Reason: " + NetworkManager.Singleton.DisconnectReason);
            var reason = NetworkManager.Singleton.DisconnectReason;

            if (reason.Contains("Game already started"))
            {
                LobbyUI.SetActive(false);
                hostStartedGameUI.SetActive(true);
            }
            else if (reason.Contains("host shutting down"))//server dc
            {
                startUI.SetActive(false);
                LobbyUI.SetActive(false);

                hasHostDC = true;
                hostLeftGameUI.SetActive(true);
                UIRootObj.SetActive(true);

                syncedCountdown.instance.StopCountdown();
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
        lobbyCode.text = "";
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
        connectedPlayers = -1;
    }

    public void setSinglePlayer(bool state) { singlePlayer = state; }

    public void showStartUI()
    {
        UIRootObj.SetActive(true);
        startUI.SetActive(true);

        LobbyUI.SetActive(false);

        resetValuesRpc();
    }

    private void resetValuesRpc()
    {
        canStartGame = new(false);
        gameStarted = new(false);
    }

    public void showHostPlayAgainUI()
    {
        if (hasHostDC)
        {
            showClientPlayAgainUI();
            hasHostDC = false;
            return;
        }

        UIRootObj.SetActive(true);
        hostingUI.SetActive(true);

        resetValuesRpc();

        resetPlayers();
        RaceManager.Instance?.ResetRace();
        resetHUD();
    }

    public void showClientPlayAgainUI()
    {
        hostingUI.SetActive(false);
        UIRootObj.SetActive(true);

        if (hostLeftGameUI.activeInHierarchy) return;
        
        LobbyUI.SetActive(true);

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
        int id = (int)clientId;
        float spawnRadius = 7.5f;
        float angle = id * 90f;
        Vector3 offset = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0f, Mathf.Sin(angle * Mathf.Deg2Rad)) * spawnRadius;
        
        return new Vector3(offset.x, 5f, offset.z);
    }
}