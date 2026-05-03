using UnityEngine;

public class RootController : MonoBehaviour
{
    public Transform flowerTarget;
    public CameraFollowRoot followCam;

    private bool isMagnetized = false;

    public bool IsMagnetActive()
    {
        return isMagnetized;
    }

    public void ActivateMagnet()
    {
        if (isMagnetized) return;

        isMagnetized = true;
        Debug.Log("🔥 MAGNET ACTIVATED");

        if (followCam != null)
        {
            followCam.isMagnetMode = true;
        }
    }
}