using UnityEngine;

// Data-only powerup definition
[CreateAssetMenu(fileName = "PowerupDef", menuName = "Powerup", order = 0)]
public class PowerupDef : ScriptableObject
{
    public string id;                // unique id
    public Sprite icon;              // ui icon 
    public float duration = 5f;      // seconds
    public bool stacks = false;      // allow multiple at once

    [Header("Effects (multipliers)")]
    public float speedMult = 1f;     // 1 = no change
    public float sizeMult = 1f;      // 1 = no change
}
