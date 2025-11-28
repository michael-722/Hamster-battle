using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class PickupSpawner : NetworkBehaviour
{
    public static PickupSpawner Instance;

    [SerializeField] private GameObject[] pickupPrefabs;
    [SerializeField] private Transform[] spawnPoints;  // I seriealized the fields this time!!!!!
    [SerializeField] private float respawnDelay = 10f;

    private List<GameObject> spawnedPickups = new List<GameObject>();

    private void Start()
    {
        Instance = this;
    }

    public void SpawnAll()
    {
        if (!IsServer) return;
        foreach (var point in spawnPoints)
        {
            var prefab = pickupPrefabs[Random.Range(0, pickupPrefabs.Length)];
            var go = Instantiate(prefab, point.position, prefab.transform.rotation);
            go.GetComponent<NetworkObject>().Spawn(true);
            spawnedPickups.Add(go);
        }
    }

    public void RespawnPickup(Vector3 position)
    {
        StartCoroutine(RespawnRoutine(position));
    }

    private System.Collections.IEnumerator RespawnRoutine(Vector3 pos)
    {
        yield return new WaitForSeconds(respawnDelay);
        if (!IsServer) yield break;

        var prefab = pickupPrefabs[Random.Range(0, pickupPrefabs.Length)];
        var go = Instantiate(prefab, pos, prefab.transform.rotation);
        go.GetComponent<NetworkObject>().Spawn(true);
    }

    public void DespawnAll()
    {
        if (!IsServer) return;

        if (spawnedPickups == null || spawnedPickups.Count == 0) return;

        for (int i = spawnedPickups.Count - 1; i >= 0; i--)
        {
            var pickup = spawnedPickups[i];
            if (pickup != null)
            {
                var netObj = pickup.GetComponent<NetworkObject>();
                if (netObj != null && netObj.IsSpawned)
                {
                    netObj.Despawn(true);
                }
                Destroy(pickup);
            }
            spawnedPickups.RemoveAt(i);
        }
    }
}
