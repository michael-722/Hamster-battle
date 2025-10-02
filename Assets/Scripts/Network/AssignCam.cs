using UnityEngine;
using Unity.Netcode;//networking libary

public class AssignCam : NetworkBehaviour//monobehaviour but with networking features
{
    private void Start()
    {
        if (IsOwner)
        {
            Camera.main.GetComponent<BallCameraFollow>().target = transform;//set the main cams target to this obj
        }
    }
}