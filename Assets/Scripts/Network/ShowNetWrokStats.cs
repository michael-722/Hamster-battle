using Unity.Multiplayer.Tools.NetStatsMonitor;
using UnityEngine;

public class ShowNetworkStats : MonoBehaviour
{
    public static ShowNetworkStats instance;

    private RuntimeNetStatsMonitor statsMonitor;

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

    private void Start()
    {
        statsMonitor = this.gameObject.GetComponent<RuntimeNetStatsMonitor>();
    }

    public void ToggleNetworkStats()
    {
        statsMonitor.Visible = !statsMonitor.Visible;
    }
}
