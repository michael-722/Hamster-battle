using System.Collections.Generic;
using UnityEngine;

// Manages active powerups on a player
public class PowerupManager : MonoBehaviour
{
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
        Reapply(); // rebuild totals each frame (cheap)
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
            _speedMult *= Mathf.Max(0.01f, a.def.speedMult); // safe
            _sizeMult *= Mathf.Max(0.01f, a.def.sizeMult);  // safe
        }

        controller?.ApplyExternalMultipliers(_speedMult, _sizeMult); // push into controller
        transform.localScale = _baseScale * _sizeMult; // scale ball
    }

    
    public (float speedMult, float sizeMult) GetTotals() => (_speedMult, _sizeMult); // totals
}
