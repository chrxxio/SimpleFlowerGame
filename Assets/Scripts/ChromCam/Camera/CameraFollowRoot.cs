using UnityEngine;

public class CameraFollowRoot : MonoBehaviour
{
    public Transform target;

    [Header("Settings")]
    public float smoothSpeed = 5f;
    public Vector3 lookOffset = new Vector3(0, 1.2f, 0);

    private Vector3 velocity;

    private Vector3 cachedOffset;
    private bool hasInitialized = false;

    void OnEnable()
    {
        // nothing
    }

    void InitializeOffset()
    {
        if (!target) return;

        cachedOffset = transform.position - target.position;
        hasInitialized = true;
    }

    void LateUpdate()
    {
        if (!target || !hasInitialized) return;

        Vector3 desiredPosition = target.position + cachedOffset;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref velocity,
            1f / smoothSpeed
        );

        transform.LookAt(target.position + lookOffset);
    }

    public void ResetOffset()
    {
        InitializeOffset();
    }
}