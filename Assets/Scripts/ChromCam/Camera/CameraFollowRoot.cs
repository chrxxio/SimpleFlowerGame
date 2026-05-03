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
    public float zoomMultiplier = 1.2f;

    [Header("Edge Detection")]
    public float edgeDistance = 4f;
    public float resetDistance = 2f;

    private Vector3 velocity;
    private Vector3 runtimeOffset;

    private bool initialized;
    private bool isRotating;
    private bool canRotateAgain = true;

    private float currentYRotation;

    void Start()
    {
        InitializeOffset();
        currentYRotation = transform.eulerAngles.y;
    }

    void InitializeOffset()
    {
        if (!target) return;

        runtimeOffset = transform.position - target.position;
        initialized = true;
    }

    void LateUpdate()
    {
        if (!target || !levelCenter) return;

        if (!initialized)
            InitializeOffset();

        Vector3 rotatedOffset =
            Quaternion.Euler(0f, currentYRotation, 0f) *
            new Vector3(
                runtimeOffset.x,
                runtimeOffset.y,
                runtimeOffset.z * zoomMultiplier
            );

        Vector3 desiredPosition = target.position + rotatedOffset;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref velocity,
            1f / followSmooth
        );

        // Lock X and Z, only Y changes.
        transform.rotation = Quaternion.Euler(0f, currentYRotation, 0f);

        CheckForEdgeRotation();
    }

    void CheckForEdgeRotation()
    {
        if (isRotating) return;

        Vector3 fromCenter = target.position - levelCenter.position;

        if (canRotateAgain && fromCenter.x > edgeDistance)
        {
            canRotateAgain = false;
            RotateCamera(1);
        }
        else if (canRotateAgain && fromCenter.x < -edgeDistance)
        {
            canRotateAgain = false;
            RotateCamera(-1);
        }

        if (Mathf.Abs(fromCenter.x) < resetDistance)
        {
            canRotateAgain = true;
        }
    }

    public void RotateCamera(int direction)
    {
        if (isRotating) return;

        StartCoroutine(RotateY(90f * direction));
    }

    IEnumerator RotateY(float amount)
    {
        isRotating = true;

        float startY = currentYRotation;
        float endY = currentYRotation + amount;

        float time = 0f;

        while (time < rotationDuration)
        {
            float t = time / rotationDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            currentYRotation = Mathf.Lerp(startY, endY, t);

            time += Time.deltaTime;
            yield return null;
        }

        currentYRotation = endY;
        isRotating = false;
    }
}