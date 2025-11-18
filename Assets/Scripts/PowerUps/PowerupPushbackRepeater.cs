using System.Collections;
using UnityEngine;

public class PowerupPushbackRepeater : MonoBehaviour
{
    private PowerupDef _def;
    private PlayerRace _picker;
    private Vector3 _initialCenter;
    private float _endTime;
    private readonly WaitForFixedUpdate _fixedDelay = new WaitForFixedUpdate();

    public void Initialise(PowerupDef def, PlayerRace picker, Vector3 center)
    {
        _def = def;
        _picker = picker;
        _initialCenter = center;
        _endTime = Time.time + Mathf.Max(0f, def.duration);

        StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        while (Time.time < _endTime)
        {
            ApplyPush();
            yield return _fixedDelay;
        }

        Destroy(gameObject);
    }

    private void ApplyPush()
    {
        if (_def == null || _def.pushBack <= 0f) return;

        var players = Object.FindObjectsOfType<PlayerRace>();
        var center = _def.centredOnPicker && _picker != null ? _picker.transform.position : _initialCenter;

        foreach (var candidate in players)
        {
            if (!PowerupService.IsTarget(_def, _picker, candidate, center)) continue;

            var rb = candidate.GetComponent<Rigidbody>();
            if (rb == null) continue;

            var dir = candidate.transform.position - center;
            if (dir.sqrMagnitude < 0.0001f) continue;

            rb.AddForce(dir.normalized * _def.pushBack, ForceMode.Acceleration);
        }
    }
}
