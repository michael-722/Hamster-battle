using UnityEngine;
using TMPro;
using Unity.Netcode;
using System.Collections;

public class syncedCountdown : NetworkBehaviour
{
    public static syncedCountdown instance;

    [SerializeField] private NetworkVariable<float> countdownTime = new(5f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private TMP_Text countdownText;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    void Start()
    {
        countdownText.gameObject.SetActive(false);
    }

    public void StartCountdown()
    {
        countdownText.gameObject.SetActive(true);
        StartCoroutine(CountdownCoroutine());
    }

    private IEnumerator CountdownCoroutine()
    {
        float remainingTime = countdownTime.Value;
        while (remainingTime > 0)
        {
            countdownText.text = Mathf.Ceil(remainingTime).ToString();
            yield return new WaitForSeconds(1f);
            remainingTime -= 1f;
        }

        if (NetworkManager.Singleton.IsConnectedClient) LobbySetUp.instance.allowStartRpc();
        else LobbySetUp.instance.setSinglePlayer(true);

        countdownText.text = "GO!";

        foreach (var ctrl in FindObjectsByType<HamsterBallController>(sortMode:FindObjectsSortMode.None))
        {
            if (ctrl.IsOwner)
            {
                ctrl.SetCanMove(true);
                ctrl.GetComponent<PlayerRace>().startRaceTimerRpc();
            }
        }

        yield return new WaitForSeconds(1f);
        countdownText.gameObject.SetActive(false);
    }
}