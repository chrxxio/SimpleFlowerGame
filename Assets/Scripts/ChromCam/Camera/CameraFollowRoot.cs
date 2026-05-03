using UnityEngine;
using System.Collections;

public class CameraFollowRoot : MonoBehaviour
{
    public Transform target;
    public Transform levelCenter;

    [Header("Follow")]
    public float followSmooth = 5f;

    [Header("Rotation")]
    public float rotationDuration = 0.4f;

    [Header("Zoom")]
    public float zoomDistance = -10f; // ← THIS is what you want

    private Vector3 velocity;
    private bool isRotating = false;

    // ✅ runtime offset (correct place)
    private Vector3 runtimeOffset;
    private bool initialized = false;
    public bool isMagnetMode = false;

    void Start()
    {
        if (target)
        {
            runtimeOffset = transform.position - target.position;
            initialized = true;
        }
    }

    void LateUpdate()
    {
        if (!target || isRotating) return;

        if (isMagnetMode)
        {
            // 🔥 SIMPLE FOLLOW (NO ROTATION LOGIC)
            Vector3 desired = target.position + new Vector3(0, runtimeOffset.y, zoomDistance);

            transform.position = Vector3.SmoothDamp(
            transform.position,
            desired,
            ref velocity,
            1f / followSmooth
        );

            transform.LookAt(target.position);
            return;
        }

        if (!initialized)
        {
            runtimeOffset = transform.position - target.position;
            initialized = true;
        }

        Vector3 toPlayer = target.position - levelCenter.position;

        Vector3 faceDir;

        if (Mathf.Abs(toPlayer.x) > Mathf.Abs(toPlayer.z))
            faceDir = new Vector3(Mathf.Sign(toPlayer.x), 0, 0);
        else
            faceDir = new Vector3(0, 0, Mathf.Sign(toPlayer.z));

        // ✅ use runtime offset (NO snapping anymore)
        Vector3 rotatedOffset =
            Quaternion.LookRotation(-faceDir) *
            new Vector3(0, runtimeOffset.y, zoomDistance);

        Vector3 desiredPosition = target.position + rotatedOffset;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref velocity,
            1f / followSmooth
        );

        Vector3 lookTarget = target.position;
        lookTarget.y = transform.position.y;

        transform.LookAt(lookTarget);
    }

    public void RotateCamera(int direction)
    {
        if (isRotating) return;

        StartCoroutine(RotateAroundCenter(90f * direction));
    }

    IEnumerator RotateAroundCenter(float angle)
    {
        isRotating = true;

        float time = 0f;
        float duration = rotationDuration;

        Vector3 pivot = levelCenter.position;
        Vector3 startPos = transform.position;
        Vector3 startDir = startPos - pivot;

        while (time < duration)
        {
            float t = time / duration;

            float step = Mathf.Lerp(0f, angle, t);

            Vector3 newDir = Quaternion.Euler(0, step, 0) * startDir;
            transform.position = pivot + newDir;

            transform.LookAt(target.position + Vector3.up * 0.75f);

            time += Time.deltaTime;
            yield return null;
        }

        Vector3 finalDir = Quaternion.Euler(0, angle, 0) * startDir;
        transform.position = pivot + finalDir;

        transform.LookAt(target.position + Vector3.up * 0.75f);

        isRotating = false;
    }
}