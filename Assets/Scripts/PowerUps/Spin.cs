using UnityEngine;

public class Spin : MonoBehaviour
{
    [SerializeField] public float Xspeed = 90f; // degrees per second
    [SerializeField] public float Yspeed = 90f; // degrees per second
    [SerializeField] public float Zspeed = 90f; // degrees per second

    private Vector3 initialEulerAngles;
    private Vector3 currentEulerAngles;

    void Start()
    {
        initialEulerAngles = transform.localEulerAngles;
        currentEulerAngles = initialEulerAngles;
    }

    void Update()
    {
        float xDelta = Xspeed * Time.deltaTime;
        float yDelta = Yspeed * Time.deltaTime;
        float zDelta = Zspeed * Time.deltaTime;

        if (Xspeed == 0f) currentEulerAngles.x = initialEulerAngles.x;
        else currentEulerAngles.x += xDelta;

        if (Yspeed == 0f) currentEulerAngles.y = initialEulerAngles.y;
        else currentEulerAngles.y += yDelta;

        if (Zspeed == 0f) currentEulerAngles.z = initialEulerAngles.z;
        else currentEulerAngles.z += zDelta;

        transform.localEulerAngles = currentEulerAngles;
    }
}
