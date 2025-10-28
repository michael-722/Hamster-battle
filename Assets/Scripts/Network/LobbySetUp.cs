using UnityEngine;
using TMPro;
using Unity.Netcode;
using UnityEngine.UI;

public class LobbySetUp : NetworkBehaviour
{
    public static LobbySetUp instance;

    private NetworkVariable<bool> canStartGame = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [SerializeField] private TMP_InputField lobbyCode;
    [SerializeField] private TMP_Text joinCode;

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
