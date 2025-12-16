using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;

public class LevelLayout : NetworkBehaviour
{
    public static LevelLayout instance;

    public NetworkVariable<int> currentLayoutid;

    [SerializeField] private List<GameObject> singlePlayerLayouts;
    [SerializeField] private List<GameObject> multiPlayerLayouts;

    private GameObject activeLayout;
    public NetworkVariable<bool> isMultiplayer = new(false);

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
        if (isMultiplayer.Value)
        {
            if (activeLayout != multiPlayerLayouts[currentLayoutid.Value])
            {
                Destroy(activeLayout);
                activeLayout = Instantiate(multiPlayerLayouts[currentLayoutid.Value], new Vector3(-35.5f, 0f, 56f), Quaternion.identity).gameObject;
                //Debug.Log("multiplayer layout set to id: " + currentLayoutid.Value);
            }
        }
        else
        {
            if (activeLayout != singlePlayerLayouts[currentLayoutid.Value])
            {
                Destroy(activeLayout);
                activeLayout = Instantiate(singlePlayerLayouts[currentLayoutid.Value], new Vector3(-35.5f, 0f, 56f), Quaternion.identity).gameObject;
                //Debug.Log("single player layout set to id: " + currentLayoutid.Value);
            }
        }
    }

    public GameObject GetActiveLayout()
    {
        return activeLayout;
    }
}