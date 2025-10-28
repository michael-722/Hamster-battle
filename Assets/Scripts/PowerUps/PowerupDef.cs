using UnityEngine;

// Data-only powerup definition
[CreateAssetMenu(fileName = "PowerupDef", menuName = "Powerup", order = 0)]
public class PowerupDef : ScriptableObject
{
    public string id;                // unique id
    public Sprite icon;              // ui icon 
    public float duration = 5f;      // seconds
    public bool stacks = false;      // allow multiple at once

    public enum PowerupScope {Self, OthersInRadius, AllInRadius } // who gets (de)buff

    [Header("Effects (multipliers)")]
    public float speedMult = 1f;     // 1 = no change
    public float sizeMult = 1f;      // 1 = no change

    [Header("Targeting")]
    public PowerupScope scope = PowerupScope.Self; // Self by default
    public bool centredOnPicker = true; // for radius effects
    public float aoeRadius = 10f;    // for radius effects
}
