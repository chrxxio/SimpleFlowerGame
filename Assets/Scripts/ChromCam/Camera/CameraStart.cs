using UnityEngine;

public class CameraStart : MonoBehaviour
{
    public Transform lookTarget;
    public Transform cameraPivot; // 👈 assign Main Camera here
    public float duration = 2f;

    private float timer;
    public bool isFinished;

    void Update()
    {
        if (isFinished) return;

        timer += Time.deltaTime;

        if (lookTarget && cameraPivot)
        {
            // 👇 rotate ONLY the camera, not the rig
            cameraPivot.LookAt(lookTarget.position + Vector3.up * 1.2f);
        }

        if (timer >= duration)
        {
            isFinished = true;
            enabled = false; // important
        }
    }
}