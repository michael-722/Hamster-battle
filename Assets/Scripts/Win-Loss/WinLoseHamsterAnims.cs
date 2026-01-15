using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class WinLoseHamsterAnims : MonoBehaviour
{
    public static WinLoseHamsterAnims instance;

    [SerializeField] private List<Transform> hamsters = new List<Transform>();

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        for (int i = 0; i < hamsters.Count; i++)
            hamsters[i].gameObject.SetActive(false);
    }

    public void ActivateWinHamsters()
    {
        Dictionary<ulong, int> players = new Dictionary<ulong, int>();

        var raceHUD = FindObjectsByType<RaceHUD>(FindObjectsSortMode.None);
        foreach (RaceHUD hud in raceHUD)
        {
            int rank = hud.GetLocalPlayerRank();
            ulong id = hud.GetComponent<NetworkObject>().OwnerClientId;
            players.Add(id, rank);
        }

        var sortedPlayers = new List<KeyValuePair<ulong, int>>(players);
        sortedPlayers.Sort((a, b) => a.Value.CompareTo(b.Value));

        for (int i = 0; i < hamsters.Count; i++)
            hamsters[i].gameObject.SetActive(false);

        for (int i = 0; i < sortedPlayers.Count && i < hamsters.Count; i++)
        {
            hamsters[i].gameObject.SetActive(true);

            ulong playerId = sortedPlayers[i].Key;
            RaceHUD playerHUD = null;
            foreach (RaceHUD hud in raceHUD)
            {
                if (hud.GetComponent<NetworkObject>().OwnerClientId == playerId)
                {
                    playerHUD = hud;
                    break;
                }
            }

            var playerSkinsRootObj = FindObjectsByType<HamsterBallController>(FindObjectsSortMode.None);
            foreach (HamsterBallController controller in playerSkinsRootObj)
            {
                if (controller.GetComponent<NetworkObject>().OwnerClientId == playerId)
                {
                    var skinSelector = controller.GetComponentInChildren<HamsterSkinSelector>();
                    hamsters[i].GetComponent<Renderer>().material = skinSelector.GetHamsterMaterial();
                    break;
                }
            }
        }
    }

    public void ActivateHamstersForLobbyPlayers(int playerCount, List<Transform> playerSkins)
    {
        for (int i = 0; i < hamsters.Count; i++)
            hamsters[i].gameObject.SetActive(false);

        for (int i = 0; i < playerCount && i < hamsters.Count && i < playerSkins.Count; i++)
        {
            hamsters[i].gameObject.SetActive(true);

        }
    }
}
