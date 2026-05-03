using UnityEngine;

public class CameraController : MonoBehaviour
{
    public CameraFollowRoot followCam;

    // 🔥 Player calls THIS
    public void TriggerEdgeRotation(int direction)
    {
        if (followCam != null)
        {
            followCam.RotateCamera(direction);
            Debug.Log("EDGE ROTATE: " + direction);
        }
    }
}