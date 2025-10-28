using UnityEngine;
using Unity.Netcode;

public class PickupSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject[] pickupPrefabs;
    [SerializeField] private Transform[] spawnPoints;  // I seriealized the fields this time!!!!!
    [SerializeField] private float respawnDelay = 10f;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        SpawnAll();
    }

    private void SpawnAll()
    {
        foreach (var point in spawnPoints)
        {
            var prefab = pickupPrefabs[Random.Range(0, pickupPrefabs.Length)];
            var go = Instantiate(prefab, point.position, point.rotation);
            go.GetComponent<NetworkObject>().Spawn(true);
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
        var go = Instantiate(prefab, pos, Quaternion.identity);
        go.GetComponent<NetworkObject>().Spawn(true);
    }
}
