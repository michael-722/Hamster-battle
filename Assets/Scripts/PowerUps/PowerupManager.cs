using System.Collections.Generic;
using UnityEngine;

// Manages active powerups on a player
public class PowerupManager : MonoBehaviour
{
    private static System.Collections.Generic.Dictionary<string, PowerupDef> _byId; 

    // register a def at runtime 
    public static void RegisterDef(PowerupDef def)
    {
        if (def == null || string.IsNullOrWhiteSpace(def.id)) return; // guard
        _byId ??= new System.Collections.Generic.Dictionary<string, PowerupDef>();
        _byId[def.id] = def; 
    }

    public static PowerupDef FindById(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null; // guard

        
        if (_byId != null && _byId.TryGetValue(id, out var hit)) return hit; // fast path

        // lazy-load from Resources/Powerups as a fallback
        _byId ??= new System.Collections.Generic.Dictionary<string, PowerupDef>();
        var all = Resources.LoadAll<PowerupDef>("Powerups"); // put assets under Resources/Powerups
        foreach (var p in all)
            if (p != null && !string.IsNullOrWhiteSpace(p.id))
                _byId[p.id] = p;

        return _byId.TryGetValue(id, out var def) ? def : null; // lookup
    }

    public void GiveById(string id) { var def = FindById(id); if (def != null) Give(def); } // uniform entry

    private class Active // runtime entry
    {
        public PowerupDef def;   // data
        public float timeLeft;   // seconds
        public Active(PowerupDef d) { def = d; timeLeft = d.duration; }
    }

    public HamsterBallController controller; // target to modify

    private readonly List<Active> _actives = new(); // active list
    private float _speedMult = 1f; // combined speed
    private float _sizeMult = 1f;  // combined size
    private Vector3 _baseScale;    // original scale

    private void Awake()
    {
        if (controller == null) controller = GetComponent<HamsterBallController>(); // auto
        _baseScale = transform.localScale; // remember base
        Reapply(); // reset
    }

    private void Update()
    {
        if (_actives.Count == 0) return; // nothing to tick
        for (int i = _actives.Count - 1; i >= 0; i--) // tick timers
        {
            _actives[i].timeLeft -= Time.deltaTime; // countdown
            if (_actives[i].timeLeft <= 0f) _actives.RemoveAt(i); // expire
        }
        Reapply(); 
    }

    public void Give(PowerupDef def)
    {
        if (def == null) return; // guard
        if (!def.stacks) // no stacking: refresh or replace
        {
            var found = _actives.Find(a => a.def == def); // find same
            if (found != null) { found.timeLeft = def.duration; Reapply(); return; } // refresh
        }
        _actives.Add(new Active(def)); // add new
        Reapply(); // apply
    }

    private void Reapply()
    {
        _speedMult = 1f; // reset totals
        _sizeMult = 1f;

        foreach (var a in _actives) // combine as multiply
        {
            _speedMult *= Mathf.Max(0.01f, a.def.speedMult); 
            _sizeMult *= Mathf.Max(0.01f, a.def.sizeMult);  
        }

        controller?.ApplyExternalMultipliers(_speedMult, _sizeMult); // push into controller
        transform.localScale = _baseScale * _sizeMult; // scale ball
    }

    public (float speedMult, float sizeMult) GetTotals() => (_speedMult, _sizeMult); // totals
}

//ngl, the ID stuff is Mainly the AI inside of VS as it was not going well (ish)
