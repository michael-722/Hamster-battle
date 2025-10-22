using UnityEngine;
using TMPro;
using Unity.Netcode;

public class LobbySetUp : NetworkBehaviour
{
    public static LobbySetUp instance;

    private NetworkVariable<bool> canStartGame = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [SerializeField] private TMP_InputField lobbyCode;
    [SerializeField] private TMP_Text joinCode;

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
}
