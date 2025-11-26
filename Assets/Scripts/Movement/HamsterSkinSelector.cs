using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class HamsterSkinSelector : MonoBehaviour
{
    [SerializeField] private List<Material> hamsterSkins;

    private void Start()
    {
        if (hamsterSkins.Count > 0)
        {
            int randomIndex = Random.Range(0, hamsterSkins.Count);
            this.GetComponent<Renderer>().material = hamsterSkins[randomIndex];
        }
    }
}
