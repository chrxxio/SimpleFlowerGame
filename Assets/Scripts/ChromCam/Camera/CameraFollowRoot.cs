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
    public float zoomMultiplier = 1.2f; // 1 = same, >1 = further out

    private Vector3 velocity;

    private Vector3 runtimeOffset;
    private bool initialized = false;

    private bool isRotating = false;

    void Start()
    {
        InitializeOffset();
    }

    void InitializeOffset()
    {
        if (!target) return;

        runtimeOffset = transform.position - target.position;
        initialized = true;
    }

    void LateUpdate()
    {
        if (!target || isRotating) return;

        if (!initialized)
            InitializeOffset();

        Vector3 toPlayer = target.position - levelCenter.position;

        Vector3 faceDir;

        if (Mathf.Abs(toPlayer.x) > Mathf.Abs(toPlayer.z))
            faceDir = new Vector3(Mathf.Sign(toPlayer.x), 0, 0);
        else
            faceDir = new Vector3(0, 0, Mathf.Sign(toPlayer.z));

        // rotate ORIGINAL offset (this preserves your starting camera angle)
        Vector3 rotatedOffset =
    Quaternion.LookRotation(-faceDir) *
    new Vector3(0, runtimeOffset.y, runtimeOffset.z * zoomMultiplier);

        Vector3 desiredPosition = target.position + rotatedOffset;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref velocity,
            1f / followSmooth
        );

        transform.LookAt(target.position + Vector3.up * 1.2f);
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

            transform.LookAt(target.position + Vector3.up * 1.2f);

            time += Time.deltaTime;
            yield return null;
        }

        // snap final
        Vector3 finalDir = Quaternion.Euler(0, angle, 0) * startDir;
        transform.position = pivot + finalDir;

        transform.LookAt(target.position + Vector3.up * 1.2f);

        isRotating = false;
    }
}