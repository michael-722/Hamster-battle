using UnityEngine;

public class Spin : MonoBehaviour
{
    public float Xspeed = 90f; // degrees per second
    public float Yspeed = 90f; // degrees per second
    public float Zspeed = 90f; // degrees per second


    void Update()
    {
        transform.Rotate(Xspeed * Time.deltaTime, Yspeed * Time.deltaTime, Zspeed * Time.deltaTime);
    }
}
