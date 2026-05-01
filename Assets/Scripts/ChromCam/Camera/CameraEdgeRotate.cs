using UnityEngine;

public class CameraEdgeRotate : MonoBehaviour
{
    public Transform target;
    public Transform orbitCenter;

    public float duration = 0.5f; // total time
    public float rotationAmount = 90f;

    private float timer = 0f;
    private bool isRotating = false;

    private Quaternion startRot;
    private Quaternion endRot;

    public System.Action onComplete;

    public void TriggerRotation()
    {
        if (!target || !orbitCenter) return;

        timer = 0f;
        isRotating = true;

        startRot = transform.rotation;

        // compute target rotation (90° around Y)
        endRot = Quaternion.Euler(0, rotationAmount, 0) * startRot;

        enabled = true;
    }

    void Update()
    {
        if (!isRotating) return;

        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / duration);

        // ease in/out (smooth cinematic)
        float smoothT = Mathf.SmoothStep(0, 1, t);

        Vector3 center = orbitCenter.position;

        // rotate position around center
        transform.RotateAround(center, Vector3.up, (rotationAmount * Time.deltaTime) / duration);

        // smooth rotation
        transform.rotation = Quaternion.Slerp(startRot, endRot, smoothT);

        transform.LookAt(target.position + Vector3.up * 1.2f);

        if (t >= 1f)
        {
            isRotating = false;
            enabled = false;

            onComplete?.Invoke();
        }
    }
}