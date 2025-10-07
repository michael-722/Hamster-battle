using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;



public class Relay : MonoBehaviour
{

    public static Relay instance;
    [SerializeField] private string offlineTag = "OfflinePlayer"; // tag for offline ball

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync();

            AuthenticationService.Instance.SignedIn += () =>
            {
                //Debug.Log("signed in " + AuthenticationService.Instance.PlayerId);
            };
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"failed to initialize unity services: {e.Message}");
        }
    }

    public async void CreateRelay()
    {
        try
        {
            CleanupOfflinePlayers(); // destroy offline player


            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log($"join code: {joinCode}");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
                );

            NetworkManager.Singleton.StartHost();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"failed to create relay: {e.Message}");
        }
    }

    public async void JoinRelay(string joinCode)
    {
        try
        {

            CleanupOfflinePlayers(); // destroy offline player


            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            Debug.Log($"joined relay with allocation id: {allocation.AllocationId}");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData,
                allocation.HostConnectionData
                );

            NetworkManager.Singleton.StartClient();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"failed to join relay: {e.Message}");
        }
    }

    
    private void CleanupOfflinePlayers()
    {
        var offlinePlayers = GameObject.FindGameObjectsWithTag(offlineTag);
        foreach (var obj in offlinePlayers)
        {
            Destroy(obj);
        }
    }
}
