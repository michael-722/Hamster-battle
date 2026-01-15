using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using System.Runtime.InteropServices.WindowsRuntime;

public class LevelLayout : NetworkBehaviour
{
    public static LevelLayout instance;

    public NetworkVariable<int> currentLayoutid;

    [SerializeField] private List<GameObject> singlePlayerLayouts;
    [SerializeField] private List<GameObject> multiPlayerLayouts;

    private GameObject activeLayout = null;
    public NetworkVariable<bool> isMultiplayer = new(false);

    private int activeLayoutId = -1;
    private bool activeLayoutIsMultiplayer = false;

    void Start()
    {
        instance = this;
    }

    public void SetSinglePlayerLayout()
    {
        if (!IsServer) return;

        if (singlePlayerLayouts.Count == 0)
        {
            Debug.LogError("no single player layouts set");
            return;
        }

        isMultiplayer.Value = false;

        currentLayoutid.Value = Random.Range(0, singlePlayerLayouts.Count);
        if (activeLayout != singlePlayerLayouts[currentLayoutid.Value])
        {
            Destroy(activeLayout);
            activeLayout = Instantiate(singlePlayerLayouts[currentLayoutid.Value], new Vector3(-35.5f, 0f, 56f), Quaternion.identity);
        }
    }

    public void SetMultiPlayerLayout()
    {
        if (!IsServer) return;

        if (multiPlayerLayouts.Count == 0)
        {
            Debug.LogError("no multi player layouts set");
            return;
        }

        isMultiplayer.Value = true;

        currentLayoutid.Value = Random.Range(0, multiPlayerLayouts.Count);
        if (activeLayout != multiPlayerLayouts[currentLayoutid.Value])
        {
            Destroy(activeLayout);
            activeLayout = Instantiate(multiPlayerLayouts[currentLayoutid.Value], new Vector3(-35.5f, 0f, 56f), Quaternion.identity);
        }
    }

    private void Update()
    {
        int desiredId = currentLayoutid.Value;
        bool desiredIsMultiplayer = isMultiplayer.Value;

        if (activeLayout != null && activeLayoutId == desiredId && activeLayoutIsMultiplayer == desiredIsMultiplayer)
        {
            return;
        }

        List<GameObject> levelList = desiredIsMultiplayer ? multiPlayerLayouts : singlePlayerLayouts;
        if (levelList == null || levelList.Count == 0)
        {
            Debug.LogError(desiredIsMultiplayer ? "no multi player layouts set" : "no single player layouts set");
            return;
        }

        if (desiredId < 0 || desiredId >= levelList.Count)
        {
            Debug.LogWarning($"currentLayoutid ({desiredId}) out of range for {(desiredIsMultiplayer ? "multi" : "single")}player layouts");
            return;
        }

        Destroy(activeLayout);
        activeLayout = Instantiate(levelList[desiredId], new Vector3(-35.5f, 0f, 56f), Quaternion.identity).gameObject;

        activeLayoutId = desiredId;
        activeLayoutIsMultiplayer = desiredIsMultiplayer;
    }

    public GameObject GetActiveLayout()
    {
        return activeLayout;
    }
}